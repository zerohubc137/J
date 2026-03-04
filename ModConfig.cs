namespace DiscordBridge
{
    public class ModConfig
    {
        // ── Discord Webhook ──────────────────────────────────
        // ใส่ URL ได้เลยในเกม — ไม่ต้องแก้ไฟล์
        public string WebhookUrl       { get; set; } = "";

        // ── Discord Bot (optional) ───────────────────────────
        // ไม่บังคับ — ต้องการถ้าอยากเห็นรายชื่อคนใน Voice Channel
        public string BotToken         { get; set; } = "";
        public string GuildId          { get; set; } = "";
        public string VoiceChannelId   { get; set; } = "";

        // ── ลิงก์ invite voice channel ──────────────────────
        public string VoiceInviteLink  { get; set; } = "";

        // ── ชื่อที่แสดงใน Discord ───────────────────────────
        // ว่าง = ใช้ชื่อ farmer อัตโนมัติ
        public string DisplayName      { get; set; } = "";

        // ── การแจ้งเตือน ────────────────────────────────────
        public bool NotifyOnJoin       { get; set; } = true;
        public bool NotifyOnLeave      { get; set; } = true;
        public bool NotifyOnSleep      { get; set; } = true;
        public bool NotifyOnNewDay     { get; set; } = false;

        // ── HUD ──────────────────────────────────────────────
        public bool ShowHudButton      { get; set; } = true;
        public bool ShowVoiceMembers   { get; set; } = true;
        public bool ShowNotifOnScreen  { get; set; } = true;
        public int  HudButtonX         { get; set; } = 8;
        public int  HudButtonY         { get; set; } = 22;

        // ── Polling ──────────────────────────────────────────
        public bool EnablePolling      { get; set; } = true;
        public int  PollIntervalSec    { get; set; } = 6;
    }
}
