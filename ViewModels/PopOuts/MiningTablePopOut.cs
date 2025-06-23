using ODEliteTracker.Models.Ship;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODEliteTracker.ViewModels.ModelViews.Mining;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class MiningTablePopOut : PopOutViewModel
    {
        private readonly MiningDataStore miningData;
        private readonly SharedDataStore sharedData;

        public MiningTablePopOut(MiningDataStore miningData, SharedDataStore sharedData) 
        {
            this.miningData = miningData;
            this.sharedData = sharedData;

            CurrentContainer = new(miningData);

            this.miningData.StoreLive += OnStoreLive;

            this.sharedData.ShipChangedEvent += OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent += OnCargoUpdated;

            if (this.miningData.IsLive)
                OnStoreLive(this.miningData, true);
        }

        protected override void Dispose()
        {
            CurrentContainer.Dispose();
            this.miningData.StoreLive -= OnStoreLive;
            this.sharedData.ShipChangedEvent -= OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent -= OnCargoUpdated;
        }

        public override string Name => "Mining Table";

        public override bool IsLive => miningData.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/Asteroid.png", UriKind.Relative);

        public MiningCurrentSessionContainer CurrentContainer { get; }

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

        public string LimpetCount
        {
            get
            {
                var limpets = currentShip?.ShipCargo.Where(x => string.Equals(x.Name, "Limpet")).FirstOrDefault();

                var count = limpets == null ? 0 : limpets.Count;

                return $"Limpets Left : {count:N0}";
            }
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                CurrentContainer.OnCurrentSessionUpdated(miningData, EventArgs.Empty);
                OnShipChanged(sharedData, sharedData.CurrentShipInfo);
                OnCargoUpdated(sharedData, sharedData.CurrentShipCargo);
            }
            OnModelLive();
        }

        private void OnShipChanged(object? sender, ShipInfo? e)
        {
            CurrentShip = e == null ? null : new(e);
        }

        private void OnCargoUpdated(object? sender, IEnumerable<ShipCargo>? e)
        {
            CurrentShip?.OnCargoUpdated(e, "Limpet");
            OnPropertyChanged(nameof(LimpetCount));
        }
    }
}
