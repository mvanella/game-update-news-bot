using System.Text;
using System.Text.Json;
using DiscordGameNewsBot.Settings;

namespace DiscordGameNewsBot.Services;

public class DiscordNotifier(AppSettings appSettings, HttpClient http)
{
    private readonly string _webhookUrl = appSettings.Discord.WebhookUrl!;
    private readonly HttpClient _http = http;

    public async Task SendMessageAsync(string message)
    {
        var payload = new { content = message };
        var json = JsonSerializer.Serialize(payload);

        await _http.PostAsync(_webhookUrl, new StringContent(json, Encoding.UTF8, "application/json"));
    }

    public async Task SendEmbedAsync(object embed)
    {
        var payload = new
        {
            embeds = new[] { embed }
        };
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _http.PostAsync(_webhookUrl, content);
    }
}
