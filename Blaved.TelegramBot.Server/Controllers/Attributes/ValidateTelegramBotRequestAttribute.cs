using Blaved.Core.Objects.Models.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace Bleved.TelegramBot.Server.Controllers.Attributes
{

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ValidateTelegramBotAttribute : TypeFilterAttribute
    {
        public ValidateTelegramBotAttribute()
            : base(typeof(ValidateTelegramBotFilter))
        {
        }

        private class ValidateTelegramBotFilter : IActionFilter
        {
            private readonly string _secretToken;

            public ValidateTelegramBotFilter(IOptions<AppConfig> _appConfig)
            {
                var botConfiguration = _appConfig.Value.BotConfiguration;
                _secretToken = botConfiguration.SecretToken;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                if (!IsValidRequest(context.HttpContext.Request))
                {
                    context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
                    {
                        StatusCode = 403
                    };
                }
            }

            private bool IsValidRequest(HttpRequest request)
            {
                var isSecretTokenProvided = request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
                if (!isSecretTokenProvided) return false;

                return string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
            }
        }
    }
}