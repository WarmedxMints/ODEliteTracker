using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Missions;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODEliteTracker.ViewModels.ModelViews.Trade;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels
{
    public sealed class TradeMissionViewModel : ODViewModel
    { 
        public TradeMissionViewModel(TradeMissionStore missionStore, SharedDataStore sharedDatastore, SettingsStore settings, FleetCarrierDataStore fcDataStore) 
        {
            this.missionStore = missionStore;
            this.sharedData = sharedDatastore;
            this.settings = settings;
            this.fcDataStore = fcDataStore;
            this.missionStore.StoreLive += OnStoreLive;
            this.missionStore.OnMissionAddedEvent += OnMissionAdded;
            this.missionStore.OnMissionUpdatedEvent += OnMissionUpdated;
            this.missionStore.OnMissionsUpdatedEvent += OnMissionsUpdated;
            this.missionStore.OnCargoDepot += OnCargoDepot;
            this.sharedData.ShipChangedEvent += OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent += OnCargoUpdated;

            //Timer to update expiry time every minute
            expiryTimeUpdateTimer = new Timer(OnUpdateTimes, null, 0, 60000);

            if (this.missionStore.IsLive)
            {
                OnStoreLive(null, true);
            }

            this.fcDataStore.CarrierStockUpdated += OnCarrierStockUpdated;
            this.fcDataStore.StoreLive += OnFcStoreLive;

            if (fcDataStore.IsLive)
            {
                OnFcStoreLive(null, true);
            }
        }

        private readonly TradeMissionStore missionStore;
        private readonly SharedDataStore sharedData;
        private readonly SettingsStore settings;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly Timer expiryTimeUpdateTimer;

        public override bool IsLive => missionStore.IsLive;
        public GridSize ActiveMissionsGridSize => settings.TradeSettings.ActiveMissionsGridSize;
        public List<TradeMissionVM> missions { get; } = [];
        public IEnumerable<TradeMissionVM> ActiveMissions { get; private set; } = [];
        public IEnumerable<TradeMissionVM> CompletedMissions { get; private set; } = [];
        public ObservableCollection<StationTradeStackVM> StationStacks { get; private set; } = [];
        public CommodityTradeStackVM? CommodityTradeStack { get; private set; }

        private ShipInfoVM? currentShip;
        public ShipInfoVM? CurrentShip
        {
            get => currentShip;
            set
            {
                currentShip = value;
                OnPropertyChanged(nameof(CurrentShip));
            }
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if(e)
            {
                if (sharedData.IsLive)
                {
                    if (sharedData.CurrentShipInfo != null)
                        OnShipChanged(null, sharedData.CurrentShipInfo);
                    if (sharedData.CurrentShipCargo != null)
                        OnCargoUpdated(null, sharedData.CurrentShipCargo);
                }

                missions.Clear();
                missions.AddRange(missionStore.Missions.Select(x =>  new TradeMissionVM(x)));
                BuildStacks();
                OnModelLive(true);
            }
        }

        private void OnMissionAdded(object? sender, TradeMission e)
        {
            var known = missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known != null)
                return;

            missions.Add(new(e));
            BuildStacks();
        }

        private void OnMissionUpdated(object? sender, TradeMission e)
        {
            var known = missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known == null)
                return;

            known.Update(e);
            BuildStacks();
        }

        private void OnMissionsUpdated(object? sender, EventArgs e)
        {
            missions.Clear();
            missions.AddRange(missionStore.Missions.Select(x => new TradeMissionVM(x)));
            BuildStacks();
        }

        private void OnCargoDepot(object? sender, TradeMission e)
        {
            var known = missions.FirstOrDefault(x => x.MissionID == e.MissionID);

            if (known == null)
                return;

            known.Update(e);
            BuildStacks();
        }

        private void OnShipChanged(object? sender, ShipInfo? e)
        {
            CurrentShip = e == null ? null : new(e);
        }

        private void OnCargoUpdated(object? sender, IEnumerable<ShipCargo>? e)
        {
            CurrentShip?.OnCargoUpdated(e);
        }

        private void BuildStacks()
        {
            ActiveMissions = missions.Where(x => x.CurrentState < MissionState.Completed);
            CompletedMissions = missions.Where(x => x.CurrentState == MissionState.Completed).OrderByDescending(x => x.CompletionTime);
            OnPropertyChanged(nameof(ActiveMissions));
            OnPropertyChanged(nameof(CompletedMissions));

            CommodityTradeStack = new(ActiveMissions);

            StationStacks.ClearCollection();

            var stations = ActiveMissions.OrderBy(x => x.OriginSystemName).ThenBy(x => x.OriginStationName).GroupBy(x => x.OriginStationName);

            foreach (var station in stations)
            {
                var stack = new StationTradeStackVM(station);

                StationStacks.AddItem(stack);
            }

            OnPropertyChanged(nameof(CommodityTradeStack));
        }

        private void OnUpdateTimes(object? state)
        {
            if (ActiveMissions.Any() == false)
                return;

            foreach( var mission in ActiveMissions)
            {
                mission.UpdateExpiryTime();
            }
        }

        public override void Dispose()
        {
            missionStore.StoreLive -= OnStoreLive;
            missionStore.OnMissionAddedEvent -= OnMissionAdded;
            missionStore.OnMissionUpdatedEvent -= OnMissionUpdated;
            missionStore.OnMissionsUpdatedEvent -= OnMissionsUpdated;
            missionStore.OnCargoDepot -= OnCargoDepot;
            sharedData.ShipChangedEvent -= OnShipChanged;
            sharedData.ShipCargoUpdatedEvent -= OnCargoUpdated;
            expiryTimeUpdateTimer.Dispose();
        }

        private void OnFcStoreLive(object? sender, bool e)
        {
            if (e && fcDataStore.CarrierData != null)
            {
                OnCarrierStockUpdated(sender, fcDataStore.CarrierData);
            }
        }

        private void OnCarrierStockUpdated(object? sender, FleetCarrier e)
        {
            if (e.Stock == null || e.Stock.Count == 0 || CommodityTradeStack == null || CommodityTradeStack.Stacks.Count == 0)
                return;

            foreach(var item in e.Stock)
            {
                var stack = CommodityTradeStack.Stacks.FirstOrDefault(x => string.Equals(x.FdevCommodity, item.FdevName, StringComparison.OrdinalIgnoreCase));
              
                if (stack == null)
                    continue;

                stack.CarrierStockInt = item.StockCount;               
                stack.CarrierDiffInt = item.StockCount == 0 ? 0 : item.StockCount - stack.CommodityCountInt;
            }

           
        }
    }
}
