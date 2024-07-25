namespace Bleved.TelegramBot.Server.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class BotRequestRouteAttribute : Attribute
    {
        public string Key { get; }

        public BotRequestRouteAttribute(string key)
        {
            Key = key;
        }
    }
}
