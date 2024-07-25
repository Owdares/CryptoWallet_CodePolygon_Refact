using Telegram.Bot.Types;
using Blaved.Core.Objects.Models;

using Blaved.Core.Objects.Models.Cryptocurrency;
using Blaved.Core.Objects.Models.Info;

namespace Blaved.Core.Interfaces.Views
{
    public interface IWalletMenu
    {
        Task<Message?> Wallet(UserModel user, List<CryptoAssetModel> assets, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);

        #region Bonus Balance
        Task BonusBalanceToMainBalanceCompleteAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task BonusBalanceToMainBalanceErrorAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> BonusBalance(UserModel user, List<CryptoAssetModel> assets, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        #endregion

        #region Withdraw
        Task<Message?> WithdrawCreateWaitNetwork(UserModel user, CryptoAssetModel asset, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawCreateWaitAsset(UserModel user, List<CryptoAssetModel> assets, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> WithdrawCreateWaitAddress(UserModel user, CryptoAssetModel assetModel, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawCreateWaitAmount(UserModel user, CryptoAssetModel assetModel, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawConfirm(UserModel user, CryptoAssetModel assetModel, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawCreateNotCorrectAddress(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawCreateNotCorrectAmount(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task WithdrawSentAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task WithdrawCreateInsufficientBalanceAnswer(UserModel user, CryptoAssetModel assetModel, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> WithdrawErrorAlert(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawCompletedAlert(UserModel user, CryptoAssetModel asset, WithdrawModel withdraw, bool isEdit = true);
        #endregion

        #region Deposit
        Task<Message?> DepositWaitAsset(UserModel user, List<CryptoAssetModel> assets, CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> DepositWaitNetwork(UserModel user, CryptoAssetModel asset, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> DepositViewAddress(UserModel user, CryptoAssetModel asset, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> DepositAlert(UserModel user, CryptoAssetModel asset, DepositModel depositModel, bool isEdit = true);
        #endregion

        #region TransactionHistory
        Task<Message?> WalletTransactionHistory(UserModel user, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawHistoryWaitNetwork(UserModel user, List<CryptoNetworkModel> networks, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> DepositHistoryWaitNetwork(UserModel user, List<CryptoNetworkModel> networks, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> DepositHistoryList(UserModel user, List<DepositModel> history,
            CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task<Message?> WithdrawHistoryList(UserModel user, List<WithdrawModel> history,
            CancellationToken cancellationToken, int currentPage = 1, bool isEdit = true);
        Task DepositHistoryNoneAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task WithdrawHistoryNoneAnswer(UserModel user, string callbackQueryId, CancellationToken cancellationToken);
        Task<Message?> DepositHistoryView(UserModel user, CryptoAssetModel asset, DepositModel transaction, CancellationToken cancellationToken, bool isEdit = true);
        Task<Message?> WithdrawHistoryView(UserModel user, CryptoAssetModel asset, WithdrawModel transaction, CancellationToken cancellationToken, bool isEdit = true);
        #endregion
    }
}
