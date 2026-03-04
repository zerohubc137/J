using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace DiscordBridge
{
    public class ModEntry : Mod
    {
        private ModConfig      Cfg;
        private DiscordClient  Discord;
        private HudOverlay     Hud;
        private bool           JoinedThisSession;
        private DateTime       LastPoll = DateTime.MinValue;
        private SettingsMenu   OpenMenu;

        public override void Entry(IModHelper h)
        {
            Cfg     = h.ReadConfig<ModConfig>();
            Discord = new DiscordClient(Monitor, Cfg);
            Hud     = new HudOverlay(Cfg);

            h.Events.GameLoop.SaveLoaded           += OnLoad;
            h.Events.GameLoop.ReturnedToTitle       += OnTitle;
            h.Events.GameLoop.DayEnding             += OnSleep;
            h.Events.GameLoop.DayStarted            += OnDayStart;
            h.Events.GameLoop.UpdateTicked          += OnTick;
            h.Events.Display.RenderedHud            += OnHud;
            h.Events.Input.CharacterInput           += OnChar;

            Monitor.Log($"[DiscordBridge v4] พร้อมใช้งาน!", LogLevel.Info);

            if (string.IsNullOrEmpty(Cfg.WebhookUrl))
                Monitor.Log("[DiscordBridge] ยังไม่ได้ตั้งค่า — กดปุ่ม Discord บน HUD เพื่อตั้งค่า", LogLevel.Warn);
        }

        // ── Game events ────────────────────────────────────────
        void OnLoad(object s, SaveLoadedEventArgs e)
        {
            if (!JoinedThisSession && Cfg.NotifyOnJoin)
            {
                JoinedThisSession = true;
                string loc = Game1.player.currentLocation?.Name ?? "Farm";
                Task.Run(() => Discord.SendJoin(Game1.player.Name, loc));
                Hud.ShowNotif($"✅ แจ้ง Discord: {Game1.player.Name} เข้าเกมแล้ว", new Color(87, 242, 135));
            }
        }

        void OnTitle(object s, ReturnedToTitleEventArgs e)
        {
            if (JoinedThisSession && Cfg.NotifyOnLeave)
            {
                string name = Game1.player?.Name ?? "Player";
                Task.Run(() => Discord.SendLeave(name));
                Hud.ShowNotif("🔴 แจ้ง Discord: ออกจากเกมแล้ว", new Color(237, 66, 69));
            }
            JoinedThisSession = false;
        }

        void OnSleep(object s, DayEndingEventArgs e)
        {
            if (!Cfg.NotifyOnSleep) return;
            Task.Run(() => Discord.SendSleep(
                Game1.player.Name, Game1.dayOfMonth,
                Game1.currentSeason, Game1.year));
            Hud.ShowNotif("💤 แจ้ง Discord: นอนหลับแล้ว", new Color(88, 101, 242));
        }

        void OnDayStart(object s, DayStartedEventArgs e)
        {
            if (!Cfg.NotifyOnNewDay) return;
            Task.Run(() => Discord.SendNewDay(Game1.player.Name, Game1.dayOfMonth, Game1.currentSeason));
        }

        // ── Update loop ────────────────────────────────────────
        void OnTick(object s, UpdateTickedEventArgs e)
        {
            if (!e.IsMultipleOf(20)) return;

            // Poll voice members
            if (Context.IsWorldReady
                && Cfg.EnablePolling()
                && (DateTime.Now - LastPoll).TotalSeconds >= Cfg.PollIntervalSec)
            {
                LastPoll = DateTime.Now;
                Task.Run(async () =>
                {
                    await Discord.PollVoiceAsync();
                    Hud.VoiceMembers = Discord.VoiceMembers;
                });
            }

            // HUD button click → open settings or invite
            if (Context.IsWorldReady && Hud.IsButtonClicked() && Game1.activeClickableMenu == null)
                OpenSettings();
        }

        void OpenSettings()
        {
            OpenMenu = new SettingsMenu(
                Cfg,
                SaveConfig,
                () => Task.Run(async () =>
                {
                    bool ok = await Discord.TestWebhookAsync();
                    Hud.ShowNotif(ok ? "✅ Webhook ทดสอบสำเร็จ!" : "❌ Webhook ผิดพลาด ตรวจสอบ URL",
                        ok ? new Color(87, 242, 135) : new Color(237, 66, 69));
                }));
            Game1.activeClickableMenu = OpenMenu;
            Game1.playSound("bigSelect");
        }

        void SaveConfig(ModConfig cfg)
        {
            Helper.WriteConfig(cfg);
            Discord.UpdateConfig(cfg);
            Hud.UpdateConfig(cfg);
            Monitor.Log("[DiscordBridge] บันทึกการตั้งค่าแล้ว", LogLevel.Info);
        }

        // ── Render ─────────────────────────────────────────────
        void OnHud(object s, RenderedHudEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            Hud.Draw(e.SpriteBatch);
        }

        // ── Keyboard text input ────────────────────────────────
        void OnChar(object s, CharacterInputEventArgs e)
        {
            if (Game1.activeClickableMenu is SettingsMenu sm)
                sm.ReceiveCharacter(e.Character);
        }

        protected override void Dispose(bool d) { if (d) Discord?.Dispose(); base.Dispose(d); }
    }

    // Extension เพื่อตรวจว่าการ poll เปิดใช้หรือเปล่า
    internal static class ConfigExt
    {
        public static bool EnablePolling(this ModConfig c)
            => c.EnablePolling && !string.IsNullOrWhiteSpace(c.BotToken);
    }
}
