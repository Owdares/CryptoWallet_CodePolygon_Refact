using Blaved.Core.Interfaces.Services.Binance;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Microsoft.Extensions.Hosting;

namespace Blaved.Core.Services.Backgrounds
{
    public class WithdrawControlBGService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<WithdrawControlBGService> _logger;
        private readonly AppConfig _appConfig;
        public WithdrawControlBGService(IServiceScopeFactory serviceScopeFactory, ILogger<WithdrawControlBGService> logger,
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
                        var withdrawService = scope.ServiceProvider.GetRequiredService<IWalletService>();

                        await withdrawService.WithdrawValidate();
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
