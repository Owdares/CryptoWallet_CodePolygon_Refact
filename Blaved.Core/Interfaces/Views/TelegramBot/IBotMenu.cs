using Blaved.Core.Interfaces.Views;

namespace Blaved.Core.Interfaces
{
    public interface IBotMenu
    {
        public IExchangeMenu Exchange { get; }
        public ISettingsMenu Settings { get; }
        public IWalletMenu Wallet { get; }
        public IBlavedPayMenu BlavedPay { get; }
        public ICheckMenu Check { get; }
        public IHelpMenu Help { get; }
        public IMainMenu Main { get; }
    }
}
