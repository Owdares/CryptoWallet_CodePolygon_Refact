namespace Blaved.Core.Utility
{
    public static class CutTool
    {
        public static string AmountCut(this decimal amount)
        {
            return amount.ToString("0.########");
        }
        public static string AmountCutUSD(this decimal amount)
        {
            return amount.ToString("F2");
        }
        public static string HashCut(this string hash)
        {
            return hash[..4] + "..." + hash[(hash.Length - 4)..];
        }
    }
}
