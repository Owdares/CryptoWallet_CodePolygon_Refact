namespace Blaved.Core.Objects.Models.Configurations
{
    public class BotConfiguration
    {
        public string BotToken { get; init; } = default!;
        public string HostAddress { get; init; } = default!;
        public string BotRoute { get; init; } = default!;
        public string SecretToken { get; init; } = default!;

    }
}
