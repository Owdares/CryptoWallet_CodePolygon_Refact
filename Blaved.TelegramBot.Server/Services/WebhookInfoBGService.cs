using Blaved.Core.Interfaces.Services.Binance;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Utility;
using Microsoft.Extensions.Options;
using Telegram.Bot;

namespace Blaved.TelegramBot.Server.Services
{
    public class WebhookInfoBGService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<WebhookInfoBGService> _logger;
        private readonly AppConfig _appConfig;
        public WebhookInfoBGService(IServiceScopeFactory serviceScopeFactory, ILogger<WebhookInfoBGService> logger,
            IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var bot = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

                        var botInfo = await bot.GetWebhookInfoAsync(stoppingToken);

                        _logger.LogInformation("WeebHook info: {@Result}", new { botInfo });
                    }
                    await Task.Delay(20000, stoppingToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task Exeption");
                    await Task.Delay(100000, stoppingToken);
                }
            }
        }
    }
}
