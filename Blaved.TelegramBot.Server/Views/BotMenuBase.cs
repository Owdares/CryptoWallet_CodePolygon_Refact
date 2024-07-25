using Blaved.Core.Objects.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bleved.TelegramBot.Server.Views
{
    public class BotMenuBase
    {
        private readonly ITelegramBotClient _botClient;
        private readonly ILogger _logger;
        public BotMenuBase(ITelegramBotClient botClient, ILogger logger) 
        {
            _logger = logger;
            _botClient = botClient;
        }
        public async Task<Message?> SendMessageAsync(UserModel user, string text, InlineKeyboardMarkup inlineKeyboard, bool isEdit = true, CancellationToken cancellationToken = default(CancellationToken))
        {
            text = text.Replace("\\n", "\n").Replace(".", "\\.").Replace(",", "\\,");

            if (isEdit)
            {
                await _botClient.EditMessageTextAsync(
                    chatId: user.UserId,
                messageId: user.MessageId,
                text: text,
                    replyMarkup: inlineKeyboard, parseMode: ParseMode.MarkdownV2, disableWebPagePreview: true, cancellationToken: cancellationToken
                    );
                _logger.LogInformation($"Reply sent to user");
                return null;
            }
            _logger.LogInformation($"Reply sent to user");
            return await _botClient.SendTextMessageAsync(
            chatId: user.UserId,
            text: text,
                   replyMarkup: inlineKeyboard, parseMode: ParseMode.MarkdownV2, disableWebPagePreview: true, cancellationToken: cancellationToken
                   );
        }
        public async Task SendMessageAnswerAsync(string? text, string callbackQueryId, CancellationToken cancellationToken, bool showAlert = true)
        {
            if (text != null)
                text = text.Replace("\\n", "\n");

            await _botClient.AnswerCallbackQueryAsync(
                          callbackQueryId,
                          text: text,
                          showAlert, cancellationToken: cancellationToken);
            _logger.LogInformation($"Reply sent to user");
        }
    }
}
