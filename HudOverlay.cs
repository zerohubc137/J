using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace DiscordBridge
{
    public class HudOverlay
    {
        private ModConfig    Cfg;
        private bool         BtnWasDown;

        // แสดงข้อความแจ้งเตือนชั่วคราวบนหน้าจอ
        private string NotifText  = "";
        private Color  NotifColor = Color.White;
        private int    NotifTimer = 0;

        public List<string> VoiceMembers { get; set; } = new();

        public HudOverlay(ModConfig cfg) => Cfg = cfg;
        public void UpdateConfig(ModConfig cfg) => Cfg = cfg;

        public void ShowNotif(string text, Color? color = null)
        {
            NotifText  = text;
            NotifColor = color ?? Color.White;
            NotifTimer = 220;
        }

        public void Draw(SpriteBatch sb)
        {
            int vw = Game1.uiViewport.Width;
            int vh = Game1.uiViewport.Height;

            // ── ปุ่ม Discord ──────────────────────────────────
            if (Cfg.ShowHudButton)
            {
                var (bx, by) = ButtonPos(vw, vh);
                // Shadow
                sb.Draw(Game1.staminaRect, new Rectangle(bx + 3, by + 3, 120, 46),
                    Color.Black * 0.4f);
                // Background blurple
                sb.Draw(Game1.staminaRect, new Rectangle(bx, by, 120, 46),
                    new Color(88, 101, 242, 220));
                // Icon + text
                DrawText(sb, "🎙 Discord", Color.White, bx + 12, by + 7);
                if (!string.IsNullOrEmpty(Cfg.VoiceInviteLink))
                    DrawSmall(sb, "tap to join voice", Color.LightGray, bx + 8, by + 26);
            }

            // ── Voice Members list ────────────────────────────
            if (Cfg.ShowVoiceMembers && VoiceMembers.Count > 0)
            {
                var (bx, by) = ButtonPos(vw, vh);
                int listY = by - 30 - VoiceMembers.Count * 24;
                DrawText(sb, "🔊 ใน Voice:", new Color(255, 220, 50), bx, listY);
                foreach (var m in VoiceMembers)
                {
                    listY += 24;
                    DrawText(sb, "  " + m, new Color(150, 255, 150), bx, listY);
                }
            }

            // ── In-game notification ──────────────────────────
            if (Cfg.ShowNotifOnScreen && NotifTimer > 0)
            {
                float alpha = NotifTimer > 40 ? 1f : NotifTimer / 40f;
                int nx = vw / 2 - 200;
                int ny = vh - 180;
                sb.Draw(Game1.staminaRect, new Rectangle(nx - 8, ny - 6, 416, 42),
                    Color.Black * 0.6f * alpha);
                var col = NotifColor * alpha;
                DrawText(sb, NotifText, col, nx, ny);
                NotifTimer--;
            }
        }

        // กดปุ่ม Discord (เปิดเมนูตั้งค่า หรือ invite link)
        public bool IsButtonClicked()
        {
            if (!Cfg.ShowHudButton) return false;
            var ms = Mouse.GetState();
            var (bx, by) = ButtonPos(Game1.uiViewport.Width, Game1.uiViewport.Height);
            bool hit  = new Rectangle(bx, by, 120, 46).Contains(ms.X, ms.Y);
            bool down = ms.LeftButton == ButtonState.Pressed;
            bool clicked = hit && down && !BtnWasDown;
            BtnWasDown = down && hit;
            return clicked;
        }

        private (int x, int y) ButtonPos(int vw, int vh)
        {
            int x = vw - (int)(vw * Cfg.HudButtonX / 100f) - 120;
            int y = vh - (int)(vh * Cfg.HudButtonY / 100f) - 46;
            return (x, y);
        }

        static void DrawText(SpriteBatch sb, string t, Color c, int x, int y)
        {
            sb.DrawString(Game1.smallFont, t, new Vector2(x + 1, y + 1), Color.Black * 0.5f);
            sb.DrawString(Game1.smallFont, t, new Vector2(x, y), c);
        }

        static void DrawSmall(SpriteBatch sb, string t, Color c, int x, int y)
        {
            sb.DrawString(Game1.tinyFont, t, new Vector2(x + 1, y + 1), Color.Black * 0.5f);
            sb.DrawString(Game1.tinyFont, t, new Vector2(x, y), c);
        }
    }
}
