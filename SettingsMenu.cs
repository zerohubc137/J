using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace DiscordBridge
{
    /// <summary>
    /// เมนูตั้งค่าในเกม — กดปุ่ม Discord บน HUD เพื่อเปิด
    /// ไม่ต้องใช้ GMCM ใช้ IClickableMenu ของ SMAPI โดยตรง
    /// รองรับทั้ง keyboard และ mobile touch
    /// </summary>
    public class SettingsMenu : IClickableMenu
    {
        private readonly ModConfig          Cfg;
        private readonly Action<ModConfig>  OnSave;
        private readonly Action             OnTest;

        // ── Text fields ─────────────────────────────────────
        private TextBox WebhookBox;
        private TextBox BotTokenBox;
        private TextBox GuildIdBox;
        private TextBox VoiceChannelBox;
        private TextBox InviteLinkBox;
        private TextBox DisplayNameBox;
        private TextBox ActiveBox;   // กล่องที่ focus อยู่

        // ── Toggles ──────────────────────────────────────────
        private readonly Dictionary<string, bool> Toggles;

        // ── UI state ─────────────────────────────────────────
        private readonly List<ClickableComponent> Tabs       = new();
        private readonly List<ClickableComponent> SaveBtn    = new();
        private readonly List<ClickableComponent> TestBtn    = new();
        private readonly List<ClickableComponent> CloseBtn   = new();
        private int    CurrentTab  = 0;   // 0=ทั่วไป 1=แจ้งเตือน 2=HUD
        private string StatusMsg   = "";
        private Color  StatusColor = Color.White;
        private int    StatusTimer = 0;

        private const int W = 680;
        private const int H = 520;
        private const int PAD = 24;

        public SettingsMenu(ModConfig cfg, Action<ModConfig> onSave, Action onTest)
            : base(Game1.uiViewport.Width  / 2 - W / 2,
                   Game1.uiViewport.Height / 2 - H / 2,
                   W, H, showUpperRightCloseButton: true)
        {
            Cfg    = cfg;
            OnSave = onSave;
            OnTest = onTest;

            Toggles = new Dictionary<string, bool>
            {
                ["NotifyOnJoin"]       = cfg.NotifyOnJoin,
                ["NotifyOnLeave"]      = cfg.NotifyOnLeave,
                ["NotifyOnSleep"]      = cfg.NotifyOnSleep,
                ["NotifyOnNewDay"]     = cfg.NotifyOnNewDay,
                ["ShowHudButton"]      = cfg.ShowHudButton,
                ["ShowVoiceMembers"]   = cfg.ShowVoiceMembers,
                ["ShowNotifOnScreen"]  = cfg.ShowNotifOnScreen,
            };

            InitUI();
        }

        private void InitUI()
        {
            int bx = xPositionOnScreen, by = yPositionOnScreen;

            // Tabs
            string[] tabNames = { "Discord", "แจ้งเตือน", "HUD" };
            for (int i = 0; i < tabNames.Length; i++)
            {
                Tabs.Add(new ClickableComponent(new Rectangle(bx + PAD + i * 160, by + 50, 150, 34), tabNames[i]));
            }

            // Buttons
            SaveBtn.Add(new ClickableComponent(new Rectangle(bx + W - 230, by + H - 52, 100, 38), "บันทึก"));
            TestBtn.Add(new ClickableComponent(new Rectangle(bx + W - 120, by + H - 52, 100, 38), "ทดสอบ"));

            // Text boxes
            int fieldX = bx + PAD + 200;
            int fieldW = W - PAD * 2 - 200;

            WebhookBox     = MakeBox(fieldX, by + 110, fieldW, Cfg.WebhookUrl);
            BotTokenBox    = MakeBox(fieldX, by + 175, fieldW, Cfg.BotToken);
            GuildIdBox     = MakeBox(fieldX, by + 240, fieldW, Cfg.GuildId);
            VoiceChannelBox= MakeBox(fieldX, by + 305, fieldW, Cfg.VoiceChannelId);
            InviteLinkBox  = MakeBox(fieldX, by + 370, fieldW, Cfg.VoiceInviteLink);
            DisplayNameBox = MakeBox(fieldX, by + 430, fieldW, Cfg.DisplayName);
        }

        private TextBox MakeBox(int x, int y, int w, string val)
        {
            var tb = new TextBox(null, null, Game1.smallFont, Color.Black)
            {
                X = x, Y = y, Width = w,
                Text = val ?? "",
                limitWidth = false
            };
            return tb;
        }

        // ── Draw ─────────────────────────────────────────────
        public override void draw(SpriteBatch b)
        {
            // Dim background
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.5f);

            // Window
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            // Title
            string title = "⚙️  Discord Voice Bridge — ตั้งค่า";
            b.DrawString(Game1.dialogueFont, title,
                new Vector2(xPositionOnScreen + PAD, yPositionOnScreen + 16),
                new Color(88, 101, 242));

            DrawTabs(b);

            if (CurrentTab == 0) DrawDiscordTab(b);
            else if (CurrentTab == 1) DrawNotifyTab(b);
            else DrawHudTab(b);

            // Buttons
            DrawButton(b, SaveBtn[0], new Color(87, 242, 135), Color.Black, "💾 บันทึก");
            DrawButton(b, TestBtn[0], new Color(88, 101, 242), Color.White, "🔔 ทดสอบ");

            // Status message
            if (StatusTimer > 0 && !string.IsNullOrEmpty(StatusMsg))
            {
                b.DrawString(Game1.smallFont, StatusMsg,
                    new Vector2(xPositionOnScreen + PAD, yPositionOnScreen + H - 48),
                    StatusColor);
                StatusTimer--;
            }

            base.draw(b);
            drawMouse(b);
        }

        private void DrawTabs(SpriteBatch b)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                var t = Tabs[i];
                Color bg = i == CurrentTab ? new Color(88, 101, 242, 220) : new Color(60, 60, 60, 180);
                b.Draw(Game1.staminaRect, t.bounds, bg);
                b.DrawString(Game1.smallFont, t.name,
                    new Vector2(t.bounds.X + 12, t.bounds.Y + 8),
                    i == CurrentTab ? Color.White : Color.LightGray);
            }
        }

        private void DrawDiscordTab(SpriteBatch b)
        {
            int bx = xPositionOnScreen + PAD;
            int by = yPositionOnScreen;

            var labels = new[] {
                ("Webhook URL *", "ต้องมี — ใส่ URL จาก Discord Webhooks"),
                ("Bot Token", "ไม่บังคับ — ใช้ดู Voice Members"),
                ("Guild / Server ID", "ไม่บังคับ — ใช้คู่กับ Bot Token"),
                ("Voice Channel ID", "ไม่บังคับ — กรองเฉพาะ channel นั้น"),
                ("Invite Link", "ลิงก์ join voice channel"),
                ("ชื่อใน Discord", "ชื่อที่แสดงในข้อความ (ว่าง = ชื่อ farmer)"),
            };
            var boxes = new[] { WebhookBox, BotTokenBox, GuildIdBox, VoiceChannelBox, InviteLinkBox, DisplayNameBox };

            for (int i = 0; i < labels.Length; i++)
            {
                int ly = by + 110 + i * 65;
                b.DrawString(Game1.smallFont, labels[i].Item1,
                    new Vector2(bx, ly + 4), Color.White);
                b.DrawString(Game1.tinyFont, labels[i].Item2,
                    new Vector2(bx, ly + 22), Color.Gray);

                // Highlight active box
                bool active = boxes[i] == ActiveBox;
                Color border = active ? new Color(88, 101, 242) : new Color(100, 100, 100);
                b.Draw(Game1.staminaRect,
                    new Rectangle(boxes[i].X - 2, boxes[i].Y - 2, boxes[i].Width + 4, 36),
                    border);
                b.Draw(Game1.staminaRect,
                    new Rectangle(boxes[i].X, boxes[i].Y, boxes[i].Width, 32),
                    new Color(20, 20, 30));
                boxes[i].Draw(b, 0);
            }
        }

        private void DrawNotifyTab(SpriteBatch b)
        {
            int bx = xPositionOnScreen + PAD;
            int by = yPositionOnScreen + 110;

            var rows = new[] {
                ("NotifyOnJoin",      "🟢 แจ้งเตือนเมื่อเข้าเกม"),
                ("NotifyOnLeave",     "🔴 แจ้งเตือนเมื่อออกจากเกม"),
                ("NotifyOnSleep",     "💤 แจ้งเตือนเมื่อนอนหลับ"),
                ("NotifyOnNewDay",    "🌅 แจ้งเตือนเมื่อขึ้นวันใหม่"),
            };
            DrawToggles(b, bx, by, rows);
        }

        private void DrawHudTab(SpriteBatch b)
        {
            int bx = xPositionOnScreen + PAD;
            int by = yPositionOnScreen + 110;

            var rows = new[] {
                ("ShowHudButton",     "🎮 แสดงปุ่ม Discord บน HUD"),
                ("ShowVoiceMembers",  "🔊 แสดงรายชื่อใครอยู่ใน Voice"),
                ("ShowNotifOnScreen", "📢 แสดงแจ้งเตือนบนหน้าจอเกม"),
            };
            DrawToggles(b, bx, by, rows);
        }

        private void DrawToggles(SpriteBatch b, int bx, int by,
                                 (string Key, string Label)[] rows)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                int y = by + i * 56;
                bool on = Toggles.GetValueOrDefault(rows[i].Key);
                Color bg   = on ? new Color(87, 242, 135, 200) : new Color(80, 80, 80, 200);
                string sym = on ? "✔  เปิด" : "✖  ปิด";
                b.DrawString(Game1.smallFont, rows[i].Label,
                    new Vector2(bx, y + 8), Color.White);
                b.Draw(Game1.staminaRect, new Rectangle(bx + 390, y, 90, 34), bg);
                b.DrawString(Game1.smallFont, sym,
                    new Vector2(bx + 398, y + 8), Color.Black);
            }
        }

        private void DrawButton(SpriteBatch b, ClickableComponent btn,
                                Color bg, Color fg, string label)
        {
            b.Draw(Game1.staminaRect, btn.bounds, bg);
            Vector2 sz  = Game1.smallFont.MeasureString(label);
            float   lx  = btn.bounds.X + (btn.bounds.Width  - sz.X) / 2f;
            float   ly  = btn.bounds.Y + (btn.bounds.Height - sz.Y) / 2f;
            b.DrawString(Game1.smallFont, label, new Vector2(lx, ly), fg);
        }

        // ── Input ─────────────────────────────────────────────
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            // Close
            if (upperRightCloseButton?.containsPoint(x, y) == true)
            { exitThisMenu(); return; }

            // Tabs
            for (int i = 0; i < Tabs.Count; i++)
                if (Tabs[i].bounds.Contains(x, y)) { CurrentTab = i; return; }

            // Save
            if (SaveBtn[0].bounds.Contains(x, y)) { Save(); return; }

            // Test
            if (TestBtn[0].bounds.Contains(x, y)) { OnTest?.Invoke(); SetStatus("📤 ส่งข้อความทดสอบแล้ว!", new Color(87, 242, 135)); return; }

            // Text boxes (tab 0 only)
            if (CurrentTab == 0)
            {
                ActiveBox = null;
                var boxes = new[] { WebhookBox, BotTokenBox, GuildIdBox, VoiceChannelBox, InviteLinkBox, DisplayNameBox };
                foreach (var tb in boxes)
                {
                    if (new Rectangle(tb.X, tb.Y, tb.Width, 36).Contains(x, y))
                    { ActiveBox = tb; tb.SelectMe(); return; }
                }
            }

            // Toggles
            if (CurrentTab == 1) ClickToggle(x, y, new[] {
                ("NotifyOnJoin", 0), ("NotifyOnLeave", 1),
                ("NotifyOnSleep", 2), ("NotifyOnNewDay", 3) });
            if (CurrentTab == 2) ClickToggle(x, y, new[] {
                ("ShowHudButton", 0), ("ShowVoiceMembers", 1),
                ("ShowNotifOnScreen", 2) });
        }

        private void ClickToggle(int mx, int my, (string Key, int Idx)[] rows)
        {
            int bx = xPositionOnScreen + PAD + 390;
            int by = yPositionOnScreen + 110;
            foreach (var (key, idx) in rows)
            {
                var r = new Rectangle(bx, by + idx * 56, 90, 34);
                if (r.Contains(mx, my))
                { Toggles[key] = !Toggles[key]; Game1.playSound("drumkit6"); return; }
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Escape || key == Keys.Enter)
            {
                if (ActiveBox != null) { ActiveBox = null; return; }
                exitThisMenu(); return;
            }
            ActiveBox?.RecieveTextInput(key switch {
                Keys.Back => '\b', _ => '\0'
            });
        }

        public override void receiveGamePadButton(Buttons b)
        {
            if (b == Buttons.B || b == Buttons.Back) exitThisMenu();
        }

        // รับ text input จาก keyboard ทุกตัวอักษร
        public void ReceiveCharacter(char c) => ActiveBox?.RecieveTextInput(c);

        private void Save()
        {
            Cfg.WebhookUrl     = WebhookBox.Text.Trim();
            Cfg.BotToken       = BotTokenBox.Text.Trim();
            Cfg.GuildId        = GuildIdBox.Text.Trim();
            Cfg.VoiceChannelId = VoiceChannelBox.Text.Trim();
            Cfg.VoiceInviteLink= InviteLinkBox.Text.Trim();
            Cfg.DisplayName    = DisplayNameBox.Text.Trim();

            Cfg.NotifyOnJoin      = Toggles["NotifyOnJoin"];
            Cfg.NotifyOnLeave     = Toggles["NotifyOnLeave"];
            Cfg.NotifyOnSleep     = Toggles["NotifyOnSleep"];
            Cfg.NotifyOnNewDay    = Toggles["NotifyOnNewDay"];
            Cfg.ShowHudButton     = Toggles["ShowHudButton"];
            Cfg.ShowVoiceMembers  = Toggles["ShowVoiceMembers"];
            Cfg.ShowNotifOnScreen = Toggles["ShowNotifOnScreen"];

            OnSave?.Invoke(Cfg);
            SetStatus("✅ บันทึกแล้ว!", new Color(87, 242, 135));
            Game1.playSound("crystal");
        }

        private void SetStatus(string msg, Color col)
        { StatusMsg = msg; StatusColor = col; StatusTimer = 120; }
    }
}
