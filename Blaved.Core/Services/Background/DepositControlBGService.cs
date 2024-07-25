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
    public class DepositControlBGService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DepositControlBGService> _logger;
        private readonly AppConfig _appConfig;
        public DepositControlBGService(IServiceScopeFactory serviceScopeFactory, ILogger<DepositControlBGService> logger,
            IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var chaines = _appConfig.AssetConfiguration.NetworkList;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var tasks = new List<Task>();
                    foreach (var chain in chaines)
                    {
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            var task = Task.Run(async () =>
                            {
                                using (var scopeChaine = _serviceScopeFactory.CreateScope())
                                {
                                    var depositeService = scopeChaine.ServiceProvider.GetRequiredService<IWalletService>();

                                    await depositeService.DepositScanTransaction(chain);
                                }
                            }, stoppingToken);

                            tasks.Add(task);
                        }
                    }
                    await Task.WhenAll(tasks);
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
