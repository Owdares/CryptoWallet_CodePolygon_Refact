namespace Blaved.Core.Objects.Models.Configurations
{
    public class CryptographyConfiguration
    {
        public string Base64Key { get; init; } = default!;
        public string Base64IV { get; init; } = default!;
    }
}
