using Telegram.Bot;
using Telegram.Bot.Types;

namespace Bleved.TelegramBot.Server.Controllers.RequestHandlers;
public class UpdateHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly ILogger<UpdateHandler> _logger;
    private readonly CallbackRequestHandler _callbackRequestHandler;
    private readonly MessageRequestHandler _messageRequestHandler;
    private readonly InlineRequestHandler _inlineRequestHandler;
    public UpdateHandler(ITelegramBotClient botClient, ILogger<UpdateHandler> logger,
        CallbackRequestHandler callbackRequestHandler, MessageRequestHandler messageRequestHandler, InlineRequestHandler inlineRequestHandler)
    {
        _botClient = botClient;
        _logger = logger;
        _callbackRequestHandler = callbackRequestHandler;
        _messageRequestHandler = messageRequestHandler;
        _inlineRequestHandler = inlineRequestHandler;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        try
        {
            var handler = update switch
            {
                { Message: { } message } => _messageRequestHandler.MessageHandlerAsync(message, cancellationToken),
                { CallbackQuery: { } callbackQuery } => _callbackRequestHandler.CallbackHandlerAsync(callbackQuery, cancellationToken),
                { InlineQuery: { } inlineQuery } => _inlineRequestHandler.InlineHandlerAsync(inlineQuery, cancellationToken),
                _ => UnknownUpdateHandlerAsync(update, cancellationToken)
            };

            await handler;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request");
        }
    }
    private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
    {
        _logger.LogWarning($"Error processing request - Unknown Update");
        return Task.CompletedTask;
    }
}
