using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace DiscordBridge
{
    public class DiscordClient : IDisposable
    {
        private readonly IMonitor   Log;
        private ModConfig           Cfg;
        private readonly HttpClient Http;

        public List<string> VoiceMembers    { get; private set; } = new();
        public bool         IsConfigured    => !string.IsNullOrWhiteSpace(Cfg.WebhookUrl);

        public DiscordClient(IMonitor log, ModConfig cfg)
        {
            Log  = log;
            Cfg  = cfg;
            Http = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };
            Http.DefaultRequestHeaders.Add("User-Agent", "StardewDiscordBridge/4.0");
        }

        public void UpdateConfig(ModConfig cfg) => Cfg = cfg;

        // ── Webhook ────────────────────────────────────────────
        public async Task SendAsync(string message = "", DiscordEmbed embed = null)
        {
            if (string.IsNullOrWhiteSpace(Cfg.WebhookUrl))
            {
                Log.Log("[Discord] WebhookUrl ยังไม่ได้ตั้งค่า — เปิดเมนูตั้งค่าในเกม", LogLevel.Warn);
                return;
            }
            try
            {
                string name = string.IsNullOrWhiteSpace(Cfg.DisplayName) ? "Stardew Bridge 🌾" : Cfg.DisplayName;
                var body = new JsonObject { ["username"] = name, ["content"] = message ?? "" };

                if (embed != null)
                {
                    var e = new JsonObject { ["title"] = embed.Title ?? "", ["color"] = embed.Color };
                    if (!string.IsNullOrEmpty(embed.Description)) e["description"] = embed.Description;
                    if (!string.IsNullOrEmpty(embed.Footer))
                    {
                        var f = new JsonObject { ["text"] = embed.Footer };
                        e["footer"] = f;
                    }
                    body["embeds"] = new JsonArray { e };
                }

                var res = await Http.PostAsync(Cfg.WebhookUrl,
                    new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json"));

                if (!res.IsSuccessStatusCode)
                    Log.Log($"[Discord] Webhook HTTP {(int)res.StatusCode}", LogLevel.Warn);
            }
            catch (Exception ex) { Log.Log("[Discord] Webhook: " + ex.Message, LogLevel.Warn); }
        }

        // ── Bot: ดึง voice members ────────────────────────────
        public async Task PollVoiceAsync()
        {
            if (string.IsNullOrWhiteSpace(Cfg.BotToken) || string.IsNullOrWhiteSpace(Cfg.GuildId)) return;
            try
            {
                Http.DefaultRequestHeaders.Remove("Authorization");
                Http.DefaultRequestHeaders.Add("Authorization", "Bot " + Cfg.BotToken);
                var res = await Http.GetAsync($"https://discord.com/api/v10/guilds/{Cfg.GuildId}/voice-states");
                if (!res.IsSuccessStatusCode) return;

                var doc  = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                var list = new List<string>();
                foreach (var st in doc.RootElement.EnumerateArray())
                {
                    // กรองเฉพาะ voice channel ที่ระบุ (ถ้าตั้งค่าไว้)
                    if (!string.IsNullOrWhiteSpace(Cfg.VoiceChannelId)
                        && st.TryGetProperty("channel_id", out var cid)
                        && cid.GetString() != Cfg.VoiceChannelId) continue;

                    if (st.TryGetProperty("member", out var mem))
                    {
                        string nick = null;
                        if (mem.TryGetProperty("nick", out var n) && n.ValueKind != JsonValueKind.Null)
                            nick = n.GetString();
                        else if (mem.TryGetProperty("user", out var u))
                        {
                            if (u.TryGetProperty("global_name", out var gn) && gn.ValueKind != JsonValueKind.Null)
                                nick = gn.GetString();
                            else if (u.TryGetProperty("username", out var un))
                                nick = un.GetString();
                        }
                        if (nick != null) list.Add(nick);
                    }
                }
                VoiceMembers = list;
            }
            catch (Exception ex) { Log.Log("[Discord] Poll: " + ex.Message, LogLevel.Debug); }
        }

        // ── Preset messages ────────────────────────────────────
        public Task SendJoin(string name, string loc)
            => SendAsync("", new DiscordEmbed { Title = $"🟢 {name} เข้าเกมแล้ว!", Description = $"📍 {loc}", Color = 0x57F287, Footer = "Stardew Valley" });

        public Task SendLeave(string name)
            => SendAsync("", new DiscordEmbed { Title = $"🔴 {name} ออกจากเกม", Color = 0xED4245, Footer = "Stardew Valley" });

        public Task SendSleep(string name, int day, string season, int year)
            => SendAsync("", new DiscordEmbed { Title = $"💤 {name} เข้านอนแล้ว", Description = $"📅 **{day} {season} ปี {year}** — รอเพื่อนนอนด้วยเพื่อข้ามวัน!", Color = 0x5865F2, Footer = "Stardew Valley" });

        public Task SendNewDay(string name, int day, string season)
            => SendAsync("", new DiscordEmbed { Title = $"🌅 วันใหม่เริ่มแล้ว!", Description = $"{name} — {day} {season}", Color = 0xFEE75C, Footer = "Stardew Valley" });

        public Task SendCustom(string msg)
            => SendAsync(msg);

        // ── Test webhook ──────────────────────────────────────
        public async Task<bool> TestWebhookAsync()
        {
            if (string.IsNullOrWhiteSpace(Cfg.WebhookUrl)) return false;
            try
            {
                await SendAsync("", new DiscordEmbed { Title = "✅ เชื่อมต่อสำเร็จ!", Description = "Discord Voice Bridge พร้อมใช้งาน", Color = 0x57F287, Footer = "Stardew Valley" });
                return true;
            }
            catch { return false; }
        }

        public void Dispose() => Http.Dispose();
    }
}
