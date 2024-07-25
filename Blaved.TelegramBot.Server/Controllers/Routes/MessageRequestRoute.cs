namespace Bleved.TelegramBot.Server.Controllers.Routes
{
    public class MessageRequestRoute
    {
        public const string Default = "Default";
        public const string CheckUpdateMessagePasswordPutUrl = "CheckUpdatePassword/Message-Password/Put-Url";
        public const string CheckActivatedMessagePasswordPutUrl = "CheckActivated/Message-Password/Put-Url";
        public const string CheckCreateMessageAmount = "CheckCreate/Message-Amount";
        public const string CheckCreateMessageCount = "CheckCreate/Message-Count";

        public const string WithdrawCreateMessageAmount = "WithdrawCreate/Message-Amount";
        public const string WithdrawCreateMessageAddress = "WithdrawCreate/Message-Address";

        public const string ExchangeCreateMessageAmount = "ExchangeCreate/Message-Amount";

        public const string BlavedPayIDTransferCreateMessageId = "BlavedPayIDTransferCreate/Message-Id";
        public const string BlavedPayIDTransferCreateMessageAmount = "BlavedPayIDTransferCreate/Message-Amount";
    }
}
