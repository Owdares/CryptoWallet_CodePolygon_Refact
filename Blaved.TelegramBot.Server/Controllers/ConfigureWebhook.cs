using Blaved.Core.Objects.Models.Configurations;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace Bleved.TelegramBot.Server.Controllers;

public class ConfigureWebhook : IHostedService
{
    private readonly ILogger<ConfigureWebhook> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly AppConfig _appConfig;
    private readonly ITelegramBotClient _telegramBotClient;

    public ConfigureWebhook( ILogger<ConfigureWebhook> logger, IServiceProvider serviceProvider, IOptions<AppConfig> appConfig,
        ITelegramBotClient telegramBotClient)
    {
        _telegramBotClient = telegramBotClient;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _appConfig = appConfig.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _telegramBotClient.SetWebhookAsync(
            url: $"{_appConfig.BotConfiguration.HostAddress}{_appConfig.BotConfiguration.BotRoute}",
            allowedUpdates: Array.Empty<UpdateType>(),
            secretToken: _appConfig.BotConfiguration.SecretToken,
            maxConnections: 100,
            cancellationToken: cancellationToken);

        _logger.LogInformation($"Webhook connection with BOT API");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Webhook disabling with BOT API");

        await _telegramBotClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
    }
}
