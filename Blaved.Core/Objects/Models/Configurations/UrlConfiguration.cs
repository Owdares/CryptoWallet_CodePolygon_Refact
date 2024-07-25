namespace Blaved.Core.Objects.Models.Configurations
{
    public class UrlConfiguration
    {
        public MediaUrl MediaUrl { get; set; }
        public string CheckUrl { get; set; }
        public string ReferralUrl { get; set; }
        public string MainLogoUrl { get; set; }
        public string CheckLogoUrl { get; set; }
        public string CheckUrlClip { get; set; }
        public string ReferralUrlClip { get; set; }
    }
    public class MediaUrl
    {
        public Dictionary<string, string> GroupByLanguage { get; set; }
        public Dictionary<string, string> ChatByLanguage { get; set; }
        public Dictionary<string, string> HelpByLanguage { get; set; }
        public Dictionary<string, string> DepositMoreInfoByLanguage { get; set; }
        public Dictionary<string, string> ExchangeMoreInfoByLanguage { get; set; }
        public Dictionary<string, string> WithdrawMoreInfoByLanguage { get; set; }
        public Dictionary<string, string> CheckMoreInfoByLanguage { get; set; }
        public Dictionary<string, string> BlavedPayIDMoreInfoByLanguage { get; set; }
    }
}
