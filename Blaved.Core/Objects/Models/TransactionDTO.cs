namespace Blaved.Core.Objects.Models
{
    public class TransactionDTO
    {
        public bool isToken {  get; set; }
        public string TransactionHash { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Value { get; set; }
    }
}
