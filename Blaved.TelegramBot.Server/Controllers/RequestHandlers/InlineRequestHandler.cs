using Telegram.Bot.Types;
using System.Reflection;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Serilog.Context;
using Bleved.TelegramBot.Server.Controllers.Routes;
using Bleved.TelegramBot.Server.Controllers.Attributes;
using Blaved.Core.Objects.Models;
using Blaved.Core.Interfaces;
using Blaved.Core.Interfaces.Services;
using Blaved.Core.Objects.Models.Configurations;

namespace Bleved.TelegramBot.Server.Controllers.RequestHandlers
{
    public class InlineRequestHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBotMenu _botMenu;
        private readonly IInfoService _infoService;
        private readonly ILogger<InlineRequestHandler> _logger;
        private readonly AppConfig _appConfig;
        public InlineRequestHandler(IUnitOfWork unitOfWork, IBotMenu botMenu, IInfoService infoService, 
            ILogger<InlineRequestHandler> logger, IOptions<AppConfig> appConfig)
        {
            _appConfig = appConfig.Value;
            _unitOfWork = unitOfWork;
            _botMenu = botMenu;
            _infoService = infoService;
            _logger = logger;
        }
        public async Task InlineHandlerAsync(InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            LogContext.PushProperty("InlineQueryId", inlineQuery.Id);
            LogContext.PushProperty("UserId", inlineQuery.From.Id);

            var user = await _unitOfWork.UserRepository.GetUser(inlineQuery.From.Id);

            if (user is not null)
            {
                try
                {
                    if (!user.IsBanned)
                    {
                        await InlineControllerAsync(user, inlineQuery, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing request");
                }
            }
        }
        private async Task InlineControllerAsync(UserModel user, InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            if (inlineQuery.Query is not { } inlineQueryQuery)
                return;

            LogContext.PushProperty("Data", inlineQueryQuery);

            var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(method => method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false).Length > 0)
                .Select(method => new
                {
                    Key = ((BotRequestRouteAttribute)method.GetCustomAttributes(typeof(BotRequestRouteAttribute), false)[0]).Key,
                    Method = (Func<UserModel, InlineQuery, CancellationToken, Task>)Delegate.CreateDelegate(typeof(Func<UserModel, InlineQuery, CancellationToken, Task>), this, method)
                })
                .ToDictionary(entry => entry.Key, entry => entry.Method);

            var match = Regex.Match(inlineQueryQuery, $@"^({_appConfig.UrlConfiguration.CheckUrlClip}|{_appConfig.UrlConfiguration.ReferralUrlClip})");
            string inlinePrefix = match.Success ? match.Value : string.Empty;

            var handler = methods.SingleOrDefault(x => inlinePrefix == x.Key);

            if (handler.Value != null)
            {
                LogContext.PushProperty("MethodProcessor", handler.Value.Method.Name);
                await handler.Value(user, inlineQuery, cancellationToken);
            }
        }


        [BotRequestRoute(InlineRequestRoute.InlineSendReferralInvite)]
        private async Task InlineSendReferralInvite_ProcessorAsync(UserModel user, InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            string[] parts = inlineQuery.Query!.Split(_appConfig.UrlConfiguration.ReferralUrlClip);

            string? pathWhoseReferalId = parts.Length == 2 ? parts[1] : null;

            if (!long.TryParse(pathWhoseReferalId, out long whoseReferalId))
            {
                return;
            }

            var whoseReferalUser = await _unitOfWork.UserRepository.GetUser(whoseReferalId);
            if (whoseReferalUser != null)
            {
                await _botMenu.Settings.InlineSendReferralInvite(user, inlineQuery.Id, whoseReferalId, cancellationToken);
            }
        }
        [BotRequestRoute(InlineRequestRoute.Default)]
        private async Task InlineSendReferralInviteDefault_ProcessorAsync(UserModel user, InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            await _botMenu.Settings.InlineSendReferralInvite(user, inlineQuery.Id, user.UserId, cancellationToken);
            
        }
        [BotRequestRoute(InlineRequestRoute.InlineChareCheck)]
        private async Task InlineChareCheck_ProcessorAsync(UserModel user, InlineQuery inlineQuery, CancellationToken cancellationToken)
        {
            string[] parts = inlineQuery.Query!.Split(_appConfig.UrlConfiguration.CheckUrlClip);

            string? checkUrl = parts.Length == 2 ? parts[1] : null;
            if(string.IsNullOrEmpty(checkUrl))
            {
                return;
            }

            if (!_appConfig.FunctionConfiguration.FunctionEnable.Check)
            {
                return;
            }

            var check = await _unitOfWork.CheckRepository.GetCheck(checkUrl);
            if (check != null && check.Count > 0)
            {
                await _botMenu.Check.InlineChareCheck(user, inlineQuery.Id, check, cancellationToken);
            }
        }
    }
}
