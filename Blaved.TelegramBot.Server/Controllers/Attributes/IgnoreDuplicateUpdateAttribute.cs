using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Telegram.Bot.Types;

namespace Bleved.TelegramBot.Server.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class IgnoreDuplicateUpdateAttribute : Attribute, IAsyncActionFilter
    {
        private readonly ConcurrentDictionary<int, DateTime> _processedUpdates = new ConcurrentDictionary<int, DateTime>();

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.TryGetValue("update", out var updateObject) && updateObject is Update update)
            {
                if (_processedUpdates.ContainsKey(update.Id))
                {
                    context.Result = new OkResult();
                    return;
                }
                else
                {
                    _processedUpdates.TryAdd(update.Id, DateTime.Now);
                    CleanUpProcessedUpdates();
                    await next();
                }
            }
            else
            {
                throw new ArgumentException("Update object is null or invalid.");
            }
        }

        private void CleanUpProcessedUpdates()
        {
            var now = DateTime.Now;
            foreach (var item in _processedUpdates.ToArray())
            {
                if ((now - item.Value).TotalHours > 1)
                {
                    _processedUpdates.TryRemove(item.Key, out _);
                }
            }
        }
    }
}