using DiscordGameNewsBot;
using DiscordGameNewsBot.Services;
using DiscordGameNewsBot.Settings;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<AppSettings>>().Value);
builder.Services.AddSingleton<SteamNewsService>();
builder.Services.AddSingleton<DiscordNotifier>();
builder.Services.AddSingleton<HttpClient>();

builder.Services.AddHostedService<NewsPollingWorker>();

var host = builder.Build();
host.Run();
