namespace DiscordGameNewsBot.Settings
{
    public class AppSettings
    {
        public DiscordSettings Discord { get; set; } = new();
        public PollingSettings Polling { get; set; } = new();
        public SteamSettings Steam { get; set; } = new();
        public List<GameConfigSetting> Games { get; set; } = new();
    }
}