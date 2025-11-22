using Newtonsoft.Json.Linq;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Market;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class ShoppingListPopOutVM : PopOutViewModel
    {
        public ShoppingListPopOutVM(ColonisationStore store, SharedDataStore sharedData, FleetCarrierDataStore fcDataStore, SettingsStore settings)
        {
            colonisationStore = store;
            this.sharedData = sharedData;
            this.fcDataStore = fcDataStore;
            this.settings = settings;

            this.settings.ColonisationSettings.ShoppingListSortingChanged += ColonisationSettings_ShoppingListSortingChanged;
            colonisationStore.StoreLive += OnColonisationStoreLive;
            colonisationStore.ShoppingListUpdated += OnShoppingListUpdated;
            this.colonisationStore.DepotUpdated += OnDepotUpdated;

            if (colonisationStore.IsLive)
            {
                OnColonisationStoreLive(colonisationStore, true);
            }

            this.sharedData.MarketEvent += OnMarketEvent;

            if (sharedData.IsLive)
                CheckMarket();

            this.fcDataStore.CarrierStockUpdated += OnCarrierStockUpdated;
            this.fcDataStore.StoreLive += OnFcStoreLive;

            if (fcDataStore.IsLive)
            {
                OnFcStoreLive(null, true);
            }
        }

        protected override void Dispose()
        {
            this.settings.ColonisationSettings.ShoppingListSortingChanged -= ColonisationSettings_ShoppingListSortingChanged;
            colonisationStore.StoreLive -= OnColonisationStoreLive;
            colonisationStore.ShoppingListUpdated -= OnShoppingListUpdated;
            this.colonisationStore.DepotUpdated -= OnDepotUpdated;
            this.sharedData.MarketEvent -= OnMarketEvent;
            this.fcDataStore.CarrierStockUpdated -= OnCarrierStockUpdated;
            this.fcDataStore.StoreLive -= OnFcStoreLive;
        }

        private readonly ColonisationStore colonisationStore;
        private readonly SharedDataStore sharedData;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly SettingsStore settings;

        public override string Name => "Shopping List";

        public override bool IsLive => colonisationStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/ShoppingCart.png", UriKind.Relative);

        public ColonisationPopOutSettingsVM Settings { get; } = new();
        public ColonisationShoppingList ShoppingList { get; } = new();

        public CommoditySorting ShoppingListCommoditySorting
        {
            get => settings.ColonisationSettings.ShoppingListSorting;
            set
            {
                settings.ColonisationSettings.ShoppingListSorting = value;
                OnPropertyChanged(nameof(ShoppingListResources));
            }
        }

        public IEnumerable<ConstructionResourceVM>? ShoppingListResources
        {
            get
            {
                if (ShoppingList.Depots.Count == 0)
                    return null;

                return ShoppingListCommoditySorting switch
                {
                    CommoditySorting.ShowAll => ShoppingList.Resources,
                    CommoditySorting.Category => ShoppingList.Resources.Where(x => x.RemainingCount > 0).OrderBy(x => x.Category).ThenBy(x => x.LocalName),
                    CommoditySorting.Remaining => ShoppingList.Resources.Where(x => x.RemainingCount > 0).OrderByDescending(x => x.RemainingCount),
                    _ => ShoppingList.Resources.Where(x => x.RemainingCount > 0).OrderBy(x => x.LocalName),
                };
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

        private void OnColonisationStoreLive(object? sender, bool e)
        {
            if (e == false)
                return;

            BuildShoppingList();
            OnModelLive();
        }

        private void OnShoppingListUpdated(object? sender, bool e)
        {
            BuildShoppingList();
        }

        private void BuildShoppingList()
        {
            ShoppingList.Depots.ClearCollection();
            var depots = colonisationStore.Depots.Where(x => colonisationStore.ShoppingList.Contains(Tuple.Create(x.MarketID, x.SystemAddress)));

            if (depots.Any())
            {
                var shoppingList = depots.Select(x => new ConstructionDepotVM(x));

                ShoppingList.AddDepots(shoppingList, fcDataStore.CarrierData, sharedData.MarketPurchases);                
            }
            OnPropertyChanged(nameof(ShoppingListResources));
        }
        private void ColonisationSettings_ShoppingListSortingChanged(object? sender, CommoditySorting e)
        {
            OnPropertyChanged(nameof(ShoppingListResources));
        }

        private void OnDepotUpdated(object? sender, ConstructionDepot? e)
        {
            if (e == null)
                return;

            ShoppingList.UpdateDepot(e, fcDataStore.CarrierData);
            CheckMarket();
            OnPropertyChanged(nameof(ShoppingListResources));
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
            ShoppingList.UpdateCarrierStock(e);
            CheckMarket();
            OnPropertyChanged(nameof(ShoppingListResources));
        }

        private void OnMarketEvent(object? sender, StationMarket? e)
        {
            CheckMarket();
        }

        private void CheckMarket()
        {
            if (sharedData.CurrentMarket == null)
                return;

            foreach (var item in ShoppingList.Resources)
            {
                var inStock = sharedData.CurrentMarket.ItemsForSale.FirstOrDefault(x => x.Name == item.FDEVName);

                item.UpdateMarketStock(inStock?.Stock ?? 0);
            }
        }

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 1000, 450);
        }
    }
}
