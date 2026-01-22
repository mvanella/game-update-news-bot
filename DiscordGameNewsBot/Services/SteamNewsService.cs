using System.Net.Http.Json;
using System.Text.RegularExpressions;
using DiscordGameNewsBot.Models;
using DiscordGameNewsBot.Settings;

namespace DiscordGameNewsBot.Services
{
    public class SteamNewsService(HttpClient http, AppSettings appSettings)
    {
        private readonly HttpClient _http = http;
        private readonly SteamSettings _steamSettings = appSettings.Steam;

        public async Task<SteamNewsItem?> GetLatestNewsAsync(int appId, int count = 1)
        {
            var url = $"{_steamSettings.BaseUrl}{_steamSettings.GetNewsForAppEndpoint}?appid={appId}&count={count}";
            var response = await _http.GetFromJsonAsync<SteamApiResponse>(url);
            var item = response?.AppNews?.NewsItems?.FirstOrDefault();

            if (item == null) { return null; }

            return new SteamNewsItem
            {
                Gid = item.Gid,
                Title = item.Title,
                Url = item.Url,
                ImageUrl = ExtractFirstImageUrl(item.Contents)

            };
        }

        private static string? ExtractFirstImageUrl(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            var match = Regex.Match(
                html,
                "<img[^>]+src=[\"'](?<url>[^\"']+)[\"']",
                RegexOptions.IgnoreCase
            );

            return match.Success ? match.Groups["url"].Value : null;
        }
    }


    internal class SteamApiResponse
    {
        public AppNewsData? AppNews { get; set; }
    }

    internal class AppNewsData
    {
        public List<SteamApiNewsItem>? NewsItems { get; set; }
    }

    internal class SteamApiNewsItem
    {
        public string Gid { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Contents { get; set; } = "";
    }
}