using ODEliteTracker.Models.Missions;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Trade;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class TradePopoutViewModel : PopOutViewModel
    {
        public TradePopoutViewModel(TradeMissionStore missionStore)
        {
            this.missionStore = missionStore;

            this.missionStore.StoreLive += OnStoreLive;
            this.missionStore.OnMissionAddedEvent += OnMissionAdded;
            this.missionStore.OnMissionUpdatedEvent += OnMissionUpdated;
            this.missionStore.OnMissionsUpdatedEvent += OnMissionsUpdated;
            this.missionStore.OnCargoDepot += OnCargoDepot;

            if (this.missionStore.IsLive)
                _ = OnStoreLive(null, true);
        }

        protected override void Dispose()
        {
            this.missionStore.StoreLive -= OnStoreLive;
            this.missionStore.OnMissionAddedEvent -= OnMissionAdded;
            this.missionStore.OnMissionUpdatedEvent -= OnMissionUpdated;
            this.missionStore.OnMissionsUpdatedEvent -= OnMissionsUpdated;
            this.missionStore.OnCargoDepot -= OnCargoDepot;
        }

        private readonly TradeMissionStore missionStore;

        public override string Name => "Trade Overlay";
        public override bool IsLive => missionStore.IsLive;
        public override Uri TitleBarIcon => new("/Assets/Icons/trade.png", UriKind.Relative);
        private List<TradeMissionVM> Missions { get; } = [];

        public CommodityTradeStackVM? CommodityTradeStack { get; private set; }

        private async Task OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                Missions.Clear();
                Missions.AddRange(missionStore.Missions.Select(x => new TradeMissionVM(x)));
                BuildStacks();
                OnModelLive();
            }
        }

        private void OnMissionAdded(object? sender, TradeMission e)
        {
            var known = Missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known != null)
                return;

            Missions.Add(new(e));
            BuildStacks();
        }

        private void OnMissionUpdated(object? sender, TradeMission e)
        {
            var known = Missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known == null)
                return;

            known.Update(e);
            BuildStacks();
        }

        private void OnMissionsUpdated(object? sender, EventArgs e)
        {
            Missions.Clear();
            Missions.AddRange(missionStore.Missions.Select(x => new TradeMissionVM(x)));
            BuildStacks();
        }

        private void OnCargoDepot(object? sender, TradeMission e)
        {
            var known = Missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known == null)
                return;

            known.Update(e);
            BuildStacks();
        }

        private void BuildStacks()
        {
            var activeMissions = Missions.Where(x => x.CurrentState < MissionState.Completed);

            CommodityTradeStack = new(activeMissions);

            OnPropertyChanged(nameof(CommodityTradeStack));
        }
    }
}
