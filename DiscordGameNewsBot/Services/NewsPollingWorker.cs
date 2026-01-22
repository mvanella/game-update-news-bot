using DiscordGameNewsBot.Models;
using DiscordGameNewsBot.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DiscordGameNewsBot.Services;

public class NewsPollingWorker
         : BackgroundService
{
    private readonly ILogger<NewsPollingWorker> _newsLogger;
    private readonly SteamNewsService _steamNewsService;
    private readonly DiscordNotifier _discordNotifier;
    private readonly AppSettings _appSettings;
    private readonly Dictionary<int, string> _lastSeen = new();
    private readonly string _storePath = "Data/LastSeenStore.json";

    public NewsPollingWorker(
        AppSettings appSettings,
        ILogger<NewsPollingWorker> newsLogger,
        SteamNewsService steamNewsService,
        DiscordNotifier discordNotifier)
    {
        _newsLogger = newsLogger;
        _steamNewsService = steamNewsService;
        _discordNotifier = discordNotifier;
        _appSettings = appSettings;

        LoadLastSeen();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = _appSettings.Polling.IntervalMinutes;

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckGamesAsync();
            await Task.Delay(TimeSpan.FromMinutes(interval), stoppingToken);
        }
    }

    private async Task CheckGamesAsync()
    {
        var games = _appSettings.Games!;

        foreach (var game in games)
        {
            var latest = await _steamNewsService.GetLatestNewsAsync(game.AppId);
            if (latest == null)
            {
                continue;
            }

            if (!_lastSeen.TryGetValue(game.AppId, out var lastGid) || lastGid != latest.Gid)
            {
                var imageUrl = latest.ImageUrl;

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    imageUrl = $"{_appSettings.Steam.CdnBaseUrl}/{game.AppId}/header.jpg";

                }

                //var msg = $"📰 **{game.Name} Update**\n{latest.Title}\n{latest.Url}";
                //await _discordNotifier.SendMessageAsync(msg);

                var embed = new
                {
                    title = latest.Title,
                    url = latest.Url,
                    thumbnail = new { url = game.ThumbnailUrl },
                    image = new { url = imageUrl },
                    description = $"New update for **{game.Name}**",
                    timestamp = DateTime.UtcNow.ToString("o"),
                    color = 0x00AEEF, // nice blue
                    fields = new[]
                    {
                        new { name = "Game", value = game.Name, inline = true },
                        new { name = "News ID", value = latest.Gid, inline = true }
                    },
                    footer = new
                    {
                        text = $"Steam App ID: {game.AppId}"
                    }
                };
                await _discordNotifier.SendEmbedAsync(embed);

                _lastSeen[game.AppId] = latest.Gid;
                SaveLastSeen();
            }
        }
    }

    private void LoadLastSeen()
    {
        if (File.Exists(_storePath))
        {
            var json = File.ReadAllText(_storePath);
            var data = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
            if (data != null)
                foreach (var kv in data)
                    _lastSeen[kv.Key] = kv.Value;
        }
    }

    private void SaveLastSeen()
    {
        Directory.CreateDirectory("Data");
        var json = JsonSerializer.Serialize(_lastSeen, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_storePath, json);
    }
}