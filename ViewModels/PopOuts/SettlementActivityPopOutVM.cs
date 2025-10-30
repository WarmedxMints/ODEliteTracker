using ODEliteTracker.Models.BGS;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.BGS;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class SettlementActivityPopOutVM : PopOutViewModel
    {
        public SettlementActivityPopOutVM(BGSDataStore dataStore)
        {
            this.dataStore = dataStore;

            this.dataStore.StoreLive += OnStoreLive;
            this.dataStore.SettlementActivityUpdated += DataStore_SettlementActivityUpdated;

            if (this.dataStore.IsLive)
                OnStoreLive(null, true);
        }

        private readonly BGSDataStore dataStore;

        public override string Name => "Settlement Activity";

        public override bool IsLive => dataStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Notifications/Stations/settlement_sm.png", UriKind.Relative);

        private SettlementActivityVM? settlementActivityVM;
        public SettlementActivityVM? SettlementActivityVM
        {
            get => settlementActivityVM;
            set
            {
                settlementActivityVM = value;
                OnPropertyChanged(nameof(SettlementActivityVM));
            }
        }

        protected override void Dispose()
        {
            this.dataStore.StoreLive -= OnStoreLive;
            this.dataStore.SettlementActivityUpdated -= DataStore_SettlementActivityUpdated;
        }

        private void DataStore_SettlementActivityUpdated(object? sender, SettlementActivity e)
        {
            if (dataStore.IsLive)
            {
                SettlementActivityVM?.Update();
            }
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                SettlementActivityVM = new(dataStore.SettlementActivity);
                OnModelLive();
            }
        }

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 400, 220);
        }
    }
}
