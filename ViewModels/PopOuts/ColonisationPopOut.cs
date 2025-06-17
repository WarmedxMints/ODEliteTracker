using Newtonsoft.Json.Linq;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Extensions;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class ColonisationPopOut : PopOutViewModel
    {
        public ColonisationPopOut(ColonisationStore store, SharedDataStore sharedData, FleetCarrierDataStore fcDataStore, SettingsStore settings)
        {
            this.colonisationStore = store;
            this.sharedData = sharedData;
            this.fcDataStore = fcDataStore;
            this.settings = settings;

            this.colonisationStore.StoreLive += OnStoreLive;
            this.colonisationStore.DepotUpdated += OnDepotUpdated;
            this.colonisationStore.NewDepot += OnNewDepot;

            if (this.colonisationStore.IsLive)
            {
                OnStoreLive(null, true);
            }

            this.sharedData.MarketEvent += (_, _) => CheckMarket();

            if (colonisationStore.IsLive)
            {
                OnStoreLive(null, true);
            }

            this.fcDataStore.CarrierStockUpdated += OnCarrierStockUpdated;
            this.fcDataStore.StoreLive += OnFcStoreLive;

            if (fcDataStore.IsLive)
            {
                OnFcStoreLive(null, true);
            }

            this.settings.ColonisationSettings.CommoditySortingChanged += ColonisationSettings_CommoditySortingChanged;
        }

        protected override void Dispose()
        {
            this.colonisationStore.StoreLive -= OnStoreLive;
            this.colonisationStore.DepotUpdated -= OnDepotUpdated;
            this.colonisationStore.NewDepot -= OnNewDepot;

            this.sharedData.MarketEvent -= (_, _) => CheckMarket();

            this.fcDataStore.CarrierStockUpdated -= OnCarrierStockUpdated;
            this.fcDataStore.StoreLive -= OnFcStoreLive;

            this.settings.ColonisationSettings.CommoditySortingChanged -= ColonisationSettings_CommoditySortingChanged;
        }

        private readonly ColonisationStore colonisationStore;
        private readonly SharedDataStore sharedData;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly SettingsStore settings;

        public override string Name => $"Colonisation Overlay";

        public override bool IsLive => colonisationStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/ColonisationBtn.png", UriKind.Relative);

        public ColonisationPopOutSettingsVM Settings { get; } = new();
        public ObservableCollection<ConstructionDepotVM> Depots { get; } = [];

        private ConstructionDepotVM? selectedDepot;
        public ConstructionDepotVM? SelectedDepot
        {
            get => selectedDepot;
            set
            {
                if (selectedDepot == value)
                {
                    OnPropertyChanged(nameof(SelectedDepotResources));
                    return;
                }
                selectedDepot = value;
                CheckMarket();
                OnFcStoreLive(null, true);
                OnPropertyChanged(nameof(SelectedDepot));
                OnPropertyChanged(nameof(SelectedDepotResources));
            }
        }

        public IEnumerable<ConstructionResourceVM>? SelectedDepotResources
        {
            get
            {
                if (SelectedDepot == null)
                    return null;

                return CommoditySorting switch
                {
                    CommoditySorting.ShowAll => SelectedDepot.Resources,
                    CommoditySorting.Category => SelectedDepot.Resources.Where(x => x.RemainingCount > 0).OrderBy(x => x.Category).ThenBy(x => x.LocalName),
                    CommoditySorting.Remaining => SelectedDepot.Resources.Where(x => x.RemainingCount > 0).OrderByDescending(x => x.RemainingCount),
                    _ => SelectedDepot.Resources.Where(x => x.RemainingCount > 0).OrderBy(x => x.LocalName),
                };
            }
        }

        public CommoditySorting CommoditySorting
        {
            get => settings.ColonisationSettings.ColonisationCommoditySorting;
            set
            {
                settings.ColonisationSettings.ColonisationCommoditySorting = value;
                OnPropertyChanged(nameof(SelectedDepotResources));
            }
        }

        protected override void ParamsUpdated()
        {
            var settings = AdditionalSettings?.ToObject<ColonisationPopOutSettings>();
            Settings.LoadSettings(settings ?? new());
        }

        internal override JObject? GetAdditionalSettings()
        {
            return JObject.FromObject(Settings.GetSettings());
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                if (colonisationStore.Depots.Any())
                {
                    Depots.ClearCollection();
                    foreach (var depot in colonisationStore.Depots)
                    {
                        if (depot.Inactive || depot.Complete)
                            continue;

                        var newDepot = new ConstructionDepotVM(depot);
                        Depots.AddItem(newDepot);
                    }
                }
                SelectedDepot = Depots.LastOrDefault();
                OnModelLive();
            }
        }

        private void OnNewDepot(object? sender, ConstructionDepot e)
        {
            if (e == null)
                return;

            var known = Depots.FirstOrDefault(x => x.MarketID == e.MarketID);

            if (known == null)
                return;

            var newDepot = new ConstructionDepotVM(e);
            Depots.AddItem(newDepot);
            SelectedDepot = newDepot;
            OnPropertyChanged(nameof(Depots));
        }

        private void OnDepotUpdated(object? sender, ConstructionDepot e)
        {
            if (e == null)
                return;
            var known = Depots.FirstOrDefault(x => x.MarketID == e.MarketID);

            if (known != null)
            {
                known.Update(e);
                SelectedDepot = known;
                if (known.Complete)
                {
                    known.Inactive = true;
                    Depots.Remove(known);
                    SelectedDepot = Depots.FirstOrDefault();
                }
                CheckMarket();
                OnPropertyChanged(nameof(Depots));
                return;
            }
            //Must be new or one we missed?
            OnNewDepot(sender, e);
        }

        private void CheckMarket()
        {
            if (selectedDepot == null || sharedData.CurrentMarket == null)
                return;

            var market = StationMarketVM.CreateEmptyItemMarket(sharedData.CurrentMarket);

            foreach (var item in sharedData.CurrentMarket.ItemsForSale)
            {
                //Carrier market data includes items which are sold out as it doesn't cancel the sell order when you do
                //So if the stock level is 0 then we just ignore it
                if (item.Stock <= 0)
                    continue;

                var required = selectedDepot.FilteredResources.FirstOrDefault(x => x.FDEVName == item.Name);

                required?.UpdateMarketStock(item.Stock);
            }
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
            if (SelectedDepot == null || SelectedDepot.Resources == null)
            {
                return;
            }

            foreach (var item in SelectedDepot.Resources)
            {
                var onCarrier = e.Stock.FirstOrDefault(x => string.Equals(x.commodity.FdevName, item.FDEVName, StringComparison.OrdinalIgnoreCase) && x.Stolen == false);

                var count = onCarrier?.StockCount ?? 0;
                item.SetCarrierStock(count);
            }
        }

        private void ColonisationSettings_CommoditySortingChanged(object? sender, CommoditySorting e)
        {
            OnPropertyChanged(nameof(CommoditySorting));
            OnPropertyChanged(nameof(SelectedDepotResources));
        }
    }
}
