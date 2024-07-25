using Blaved.Core.Interfaces.Services.Binance;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models.Configurations;
using Blaved.Core.Utility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Blaved.Core.Services.Backgrounds
{
    public class InformationBGService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<InformationBGService> _logger;
        private readonly AppConfig _appConfig;
        public InformationBGService(IServiceScopeFactory serviceScopeFactory, ILogger<InformationBGService> logger,
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
                        var binanceService = scope.ServiceProvider.GetRequiredService<IBinanceService>();

                        var convertInfo = await binanceService.GetCoinConvertInfo();
                        var binanceAssetInfo = await binanceService.GetAssetInfo();
                        var coinPriceInfo = await binanceService.GetCoinPriceUSD();

                        if (!binanceAssetInfo.Status || !coinPriceInfo.Status || !convertInfo.Status
                            || binanceAssetInfo.Data == null || coinPriceInfo.Data == null || convertInfo.Data == null)
                        {
                            throw new Exception();
                        }
                        await JsonFileManager.PutToJsonAsync(_appConfig.PathConfiguration.InfoForBinanceAsset, binanceAssetInfo.Data);
                        await JsonFileManager.PutToJsonAsync(_appConfig.PathConfiguration.InfoForConvert, convertInfo.Data);
                        await JsonFileManager.PutToJsonAsync(_appConfig.PathConfiguration.InfoForPriceCoin, coinPriceInfo.Data);

                    }
                    await Task.Delay(1000000, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task Exeption");
                    await Task.Delay(1000000, stoppingToken);
                }
            }
        }
    }
}
