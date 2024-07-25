namespace Blaved.Core.Utility
{
    public static class CalculationTool
    {
        public static decimal AmountToUSD(this decimal sum, decimal coinPriceUSD)
        {
            return (sum * coinPriceUSD).AmountRound();
        }
        public static decimal AmountRound(this decimal amount, int decimals = 8)
        {
            return Math.Round(amount, decimals, MidpointRounding.ToZero);
        }
        public static decimal CalculationHiddenExchangeFee(decimal toAmount, decimal feeInPercent)
        {
            return (toAmount * (feeInPercent / 100m)).AmountRound();
        }
        public static decimal ConvertCoin(decimal amount, decimal coinPriceUSD, decimal toCoinPriceUSD)
        {
            return (amount * (coinPriceUSD / toCoinPriceUSD)).AmountRound();
        }
        public static decimal CalculationReferalRate(decimal amount, int rate)
        {
            return (amount * (rate / 100m)).AmountRound();
        }
    }
}
