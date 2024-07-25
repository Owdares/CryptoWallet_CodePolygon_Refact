using Bleved.TelegramBot.Server.Controllers.Attributes;
using Bleved.TelegramBot.Server.Controllers.RequestHandlers;
using Microsoft.AspNetCore.Mvc;
using Serilog.Context;
using Telegram.Bot.Types;

namespace Bleved.TelegramBot.Server.Controllers;

public class BotController : Controller
{
    private readonly ILogger<BotController> _logger;

    public BotController(ILogger<BotController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [RateLimit(key: "Bot", tokens: 50, seconds: 1)]
    [ValidateTelegramBot]
    [IgnoreDuplicateUpdate]
    public async Task<IActionResult> Post(
        [FromBody] Update update,
        [FromServices] UpdateHandler handleUpdateService,
        CancellationToken cancellationToken)
    {
        LogContext.PushProperty("UpdateId", update.Id);
        _logger.LogInformation($"----->  Receipt of a request for API BOT");

        await handleUpdateService.HandleUpdateAsync(update, cancellationToken);

        _logger.LogInformation($"Completed processing request of the API BOT  <-----");

        return Ok();
    }
}
