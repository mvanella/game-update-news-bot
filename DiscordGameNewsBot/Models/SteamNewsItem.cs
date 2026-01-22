namespace DiscordGameNewsBot.Models
{
    public class SteamNewsItem
    {
        public string Gid { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string? ImageUrl { get; set; }
    }
}