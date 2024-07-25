namespace Blaved.Core.Objects.Models.Configurations
{
    public class BlockChainConfiguration
    {
        public Dictionary<string, string> NetworkNodesUrl { get; init; } = default!;

        public Dictionary<string, string> UserMnemonics { get; init; } = default!;
        public Dictionary<string, string> HotMnemonics { get; init; } = default!;

    }
}
