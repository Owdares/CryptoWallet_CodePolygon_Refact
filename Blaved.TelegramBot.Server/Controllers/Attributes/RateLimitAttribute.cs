using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bleved.TelegramBot.Server.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RateLimitAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, TokenBucket> TokenBuckets = new ConcurrentDictionary<string, TokenBucket>();

        private readonly string _key;
        private readonly int _tokens;
        private readonly int _seconds;

        public RateLimitAttribute(string key, int tokens, int seconds)
        {
            _key = key;
            _tokens = tokens;
            _seconds = seconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var ip = GetClientIp(filterContext.HttpContext.Request);

            var tokenBucketKey = $"{_key}_{ip}";

            var tokenBucket = TokenBuckets.GetOrAdd(tokenBucketKey, new TokenBucket(_tokens, _seconds));

            if (!tokenBucket.TryConsume())
            {
                filterContext.Result = new ContentResult
                {
                    Content = "Too Many Requests",
                    StatusCode = 429
                };
            }
        }

        private string GetClientIp(HttpRequest request)
        {
            var ipAddress = request.Headers["X-Forwarded-For"].ToString();

            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            return ipAddress ?? string.Empty;
        }
    }

    public class TokenBucket
    {
        private readonly int _capacity;
        private readonly int _tokensPerSecond;
        private int _tokens;
        private DateTime _lastRefillTime;

        public TokenBucket(int capacity, int seconds)
        {
            _capacity = capacity;
            _tokensPerSecond = capacity / seconds;
            _tokens = capacity;
            _lastRefillTime = DateTime.Now;
        }

        public bool TryConsume()
        {
            Refill();

            lock (this)
            {
                if (_tokens > 0)
                {
                    _tokens--;
                    return true;
                }

                return false;
            }
        }

        private void Refill()
        {
            var now = DateTime.Now;

            lock (this)
            {
                var elapsedSeconds = (int)(now - _lastRefillTime).TotalSeconds;

                if (elapsedSeconds > 0)
                {
                    _tokens = Math.Min(_capacity, _tokens + elapsedSeconds * _tokensPerSecond);
                    _lastRefillTime = now;
                }
            }
        }
    }
}
