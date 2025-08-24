using ODEliteTracker.Helpers;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Market;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class ColonisationViewModel : ODViewModel
    {
        public ColonisationViewModel(ColonisationStore colonisationStore,
                                     SharedDataStore sharedDataStore,
                                     FleetCarrierDataStore fcDataStore,
                                     SettingsStore settings,
                                     NotificationService notification)
        {
            this.colonisationStore = colonisationStore;
            this.sharedData = sharedDataStore;
            this.fcDataStore = fcDataStore;
            this.settings = settings;
            this.notification = notification;
            this.colonisationStore.StoreLive += OnStoreLive;
            this.colonisationStore.DepotUpdated += OnDepotUpdated;
            this.colonisationStore.NewDepot += OnNewDepot;
            this.colonisationStore.NewCommanderSystem += OnNewCommanderSystem;
            this.colonisationStore.ReleaseCommanderSystem += OnReleaseCommandSystem;

            this.sharedData.MarketEvent += OnMarketEvent;
            this.sharedData.ShipChangedEvent += OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent += OnCargoUpdated;
            this.sharedData.PurchasesUpdated += OnPurchasesUpdated;
            this.sharedData.CurrentSystemChanged += OnCurrentSystemChanged;
            this.sharedData.WatchedMarketUpdated += OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated += OnWatchedMarketsUpdated;

            if (this.sharedData.IsLive)
            {
                if (this.sharedData.CurrentShipInfo != null)
                    OnShipChanged(null, this.sharedData.CurrentShipInfo);
                if(this.sharedData.CurrentShipCargo != null)
                    OnCargoUpdated(null, this.sharedData.CurrentShipCargo);
            }

            SetSelectedDepotCommand = new ODRelayCommand<ConstructionDepotVM?>(SetSelectedDepot);
            SetSelectedCommanderSystemCommand = new ODRelayCommand<CommanderSystemVM?>(SetSelectedCommanderSystem);
            SetActiveStateCommand = new ODRelayCommand<ConstructionDepotVM>(SetDepotActiveState);
            CreatePostCommand = new ODRelayCommand<ColonisationPostType>(CreatePost);
            SetClipboardCommand = new ODRelayCommand<string>(CopyToClipboard);
            AddShoppingListCommand = new ODRelayCommand<ConstructionDepotVM?>(OnAddShoppingList);
            AddRemoveWatchedMarketsCommand = new ODRelayCommand<ulong>(OnAddRemoveWatchedMarket);

            Depots.CollectionChanged += Depots_CollectionChanged;

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

        public override void Dispose()
        {
            this.colonisationStore.StoreLive -= OnStoreLive;
            this.colonisationStore.DepotUpdated -= OnDepotUpdated;
            this.colonisationStore.NewDepot -= OnNewDepot;
            this.colonisationStore.NewCommanderSystem -= OnNewCommanderSystem;
            this.colonisationStore.ReleaseCommanderSystem -= OnReleaseCommandSystem;

            this.sharedData.MarketEvent -= OnMarketEvent;
            this.sharedData.ShipChangedEvent -= OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent -= OnCargoUpdated;
            this.sharedData.PurchasesUpdated -= OnPurchasesUpdated;
            this.sharedData.CurrentSystemChanged -= OnCurrentSystemChanged;
            this.sharedData.WatchedMarketUpdated -= OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated -= OnWatchedMarketsUpdated;

            this.fcDataStore.CarrierStockUpdated -= OnCarrierStockUpdated;
            this.fcDataStore.StoreLive -= OnFcStoreLive;

            this.settings.ColonisationSettings.CommoditySortingChanged -= ColonisationSettings_CommoditySortingChanged;
        }

        #region Private fields
        private readonly ColonisationStore colonisationStore;
        private readonly SharedDataStore sharedData;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly SettingsStore settings;
        private readonly NotificationService notification;
        #endregion

        #region Commands
        public ICommand SetSelectedDepotCommand { get; }
        public ICommand SetActiveStateCommand { get; }
        public ICommand SetSelectedCommanderSystemCommand { get; }
        public ICommand CreatePostCommand { get; }
        public ICommand SetClipboardCommand { get; }
        public ICommand AddShoppingListCommand { get; }
        public ICommand AddRemoveWatchedMarketsCommand { get; }
        #endregion

        #region Public properties
        public override bool IsLive { get => colonisationStore.IsLive; }
        public GridSize ColonisationGridSize => settings.ColonisationSettings.ColonisationGridSize;
        public string ActiveButtonText
        {
            get
            {
                if (selectedDepot == null)
                    return string.Empty;

                return selectedDepot.Inactive ? "Set Active" : "Set Inactive";
            }
        }

        public CommoditySorting CommoditySorting
        {
            get => settings.ColonisationSettings.ColonisationCommoditySorting;
            set
            {
                settings.ColonisationSettings.ColonisationCommoditySorting = value;
                OnPropertyChanged(nameof(SelectedDepotResources));
                OnPropertyChanged(nameof(CurrentMarketItems));
            }
        }

        public CommoditySorting ShoppingListCommoditySorting
        {
            get => settings.ColonisationSettings.ShoppingListSorting;
            set
            {
                settings.ColonisationSettings.ShoppingListSorting = value;
                OnPropertyChanged(nameof(ShoppingListResources));
            }
        }

        public CommoditySorting WatchedMarketSorting
        {
            get => settings.ColonisationSettings.WatchedMarketSorting;
            set
            {
                settings.ColonisationSettings.WatchedMarketSorting = value;
                OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
            }
        }

        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                tabIndex = value;
                OnPropertyChanged(nameof(TabIndex));
            }
        }

        public ObservableCollection<ConstructionDepotVM> Depots { get; } = [];
        public ColonisationShoppingList ShoppingList { get; } = new();
        public IEnumerable<ConstructionDepotVM> ActiveDepots => Depots.Where(x => x.Inactive == false);
        public IEnumerable<ConstructionDepotVM> InactiveDepots => Depots.Where(x => x.Inactive == true);
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

        private ObservableCollection<WatchedMarketVM> watchedMarkets = [];
        public ObservableCollection<WatchedMarketVM> WatchedMarkets
        {
            get => watchedMarkets;
            set
            {
                watchedMarkets = value;
                OnPropertyChanged(nameof(WatchedMarkets));
            }
        }

        private WatchedMarketVM? selectedWatchedMarket;
        public WatchedMarketVM? SelectedWatchedMarket
        {
            get => selectedWatchedMarket;
            set
            {
                if (value != selectedWatchedMarket)
                {
                    selectedWatchedMarket = value;
                    CheckMarket();
                    OnPropertyChanged(nameof(SelectedWatchedMarket));
                    OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
                }
            }
        }

        public IEnumerable<StationCommodityVM>? SelectedWatchedMarketCommodities
        {
            get
            {
                return WatchedMarketSorting switch
                {
                    CommoditySorting.Name => selectedWatchedMarket?.Commodities.OrderBy(x => x.Name),
                    CommoditySorting.Category => selectedWatchedMarket?.Commodities.OrderBy(x => x.Category),
                    CommoditySorting.Remaining => selectedWatchedMarket?.Commodities.OrderByDescending(x => x.Demand),
                    _ => selectedWatchedMarket?.Commodities,
                };
            }
        }

        private ConstructionResourceVM? selectedResource;
        public ConstructionResourceVM? SelectedResource
        {
            get
            {
                return selectedResource;
            }
            set
            {
                selectedResource = value;
                if(selectedResource != null)
                    TabIndex = 3;
                OnPropertyChanged(nameof(SelectedResource));
                OnPropertyChanged(nameof(Purchases));
            }
        }

        private ConstructionDepotVM? selectedDepot;
        public ConstructionDepotVM? SelectedDepot
        {
            get => selectedDepot;
            set
            {
                selectedDepot = value;

                CheckMarket();
                OnFcStoreLive(null, true);
                OnPropertyChanged(nameof(SelectedDepot));
                OnPropertyChanged(nameof(ActiveButtonText));
                OnPropertyChanged(nameof(SelectedDepotResources));
                if (selectedDepot != null)
                {
                    selectedResource = selectedDepot.Resources.FirstOrDefault();
                }
                OnPropertyChanged(nameof(SelectedResource));
            }
        }

        public IEnumerable<MarketPurchaseVM>? Purchases => sharedData.MarketPurchases.FirstOrDefault(x => x.Key == SelectedResource?.commodity).Value?
                                                                                     .OrderByDescending(x => x.PurchaseDate)
                                                                                     .Select(x => new MarketPurchaseVM(x, sharedData.CurrentSystem));

        private readonly ObservableCollection<CommanderSystemVM> commanderSystems = [];
        public IEnumerable<CommanderSystemVM> CommanderSystems => commanderSystems.Where(x => x.Depots.Count > 0);

        private CommanderSystemVM? selectedCommanderSystem;
        public CommanderSystemVM? SelectedCommanderSystem
        {
            get => selectedCommanderSystem;
            set
            {
                selectedCommanderSystem = value;
                OnPropertyChanged(nameof(SelectedCommanderSystem));
            }
        }
        public string? CurrentSystem { get; set; }

        private StationMarketVM? currentMarket;
        public StationMarketVM? CurrentMarket
        {
            get => currentMarket;
            set
            {
                currentMarket = value;
                OnPropertyChanged(nameof(CurrentMarket));
                OnPropertyChanged(nameof(CurrentMarketItems));
            }
        }

        public IEnumerable<StationCommodityVM>? CurrentMarketItems
        {
            get
            {
                if (CurrentMarket is null)
                    return null;
                return CommoditySorting switch
                {
                    CommoditySorting.Category => CurrentMarket.ItemsForSale.Where(x => x.RequiredResource).OrderBy(x => x.Category_Localised).ThenBy(x => x.Name_Localised),
                    CommoditySorting.Name => CurrentMarket.ItemsForSale.Where(x => x.RequiredResource).OrderBy(x => x.Name_Localised),
                    CommoditySorting.Remaining => CurrentMarket.ItemsForSale.Where(x => x.RequiredResource).OrderByDescending(x => x.Required),
                    _ => CurrentMarket.ItemsForSale.OrderBy(x => x.Name_Localised),
                };
            }
        }

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

        private string discordButtonText = "Create Post";
        public string DiscordButtonText
        {
            get => discordButtonText;
            set
            {
                discordButtonText = value;
                OnPropertyChanged(nameof(DiscordButtonText));
            }
        }

        public int SelectedDepotTab
        {
            get => settings.ColonisationSettings.SelectedDepotTab;
            set
            {
                settings.ColonisationSettings.SelectedDepotTab = value;
                CheckMarket();
                OnPropertyChanged(nameof(SelectedDepotTab));
            }
        }
        #endregion

        private void CreatePost(ColonisationPostType type)
        {
            //Single Depot
            if (SelectedDepotTab == 0)
            {
                if (SelectedDepot is null)
                    return;

                if (DiscordPostCreator.CreateColonisationPost(SelectedDepot, SelectedDepotResources, type))
                {
                    DiscordButtonText = "Post Created";
                    notification.ShowBasicNotification(new("Clipboard", ["Construction Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                    Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                }
                return;
            }
            //Shopping List
            if (ShoppingList is null || ShoppingList.Depots.Count == 0)
                return;

            if (DiscordPostCreator.CreateColonisationPost(ShoppingList, ShoppingListResources, type))
            {
                DiscordButtonText = "Post Created";
                notification.ShowBasicNotification(new("Clipboard", ["Construction Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
            }
        }

        private void Depots_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ActiveDepots));
            OnPropertyChanged(nameof(InactiveDepots));
        }

        private void SetSelectedDepot(ConstructionDepotVM? vM)
        {
            SelectedDepot = vM;
        }

        private void SetDepotActiveState(ConstructionDepotVM vM)
        {
            var inactive = vM.Inactive;
            vM.Inactive = !inactive;
            colonisationStore.SetDepotActiveState(vM);

            if (vM.Inactive)
            {
                var shoppingListItem = ShoppingList.Depots.FirstOrDefault(x => x.MarketID == vM.MarketID);

                if (shoppingListItem != null)
                {
                    OnAddShoppingList(shoppingListItem);
                }
            }
            OnPropertyChanged(nameof(ActiveDepots));
            OnPropertyChanged(nameof(InactiveDepots));
            OnPropertyChanged(nameof(ActiveButtonText));
        }

        private void OnNewDepot(object? sender, ConstructionDepot? e)
        {
            if (e == null)
                return;

            var newDepot = new ConstructionDepotVM(e);
            newDepot.UpdateMostRecentPurchase(sharedData.MarketPurchases);
            Depots.AddItem(newDepot);
            SelectedDepot = newDepot;

            var cmdrSystem = commanderSystems.FirstOrDefault(x => x.SystemAddress == newDepot.SystemAddress);
            cmdrSystem?.Depots.AddItem(newDepot);

            OnPropertyChanged(nameof(Depots));
        }

        private void OnDepotUpdated(object? sender, ConstructionDepot? e)
        {
            if (e == null)
                return;
            var known = Depots.FirstOrDefault(x => x.MarketID == e.MarketID);

            if (known != null)
            {
                known.Update(e);
                known.UpdateMostRecentPurchase(sharedData.MarketPurchases);
                SelectedDepot = known;
                if(known.Complete)
                {
                    known.Inactive = true;
                    OnPropertyChanged(nameof(ActiveDepots));
                    OnPropertyChanged(nameof(InactiveDepots));
                }
                OnPropertyChanged(nameof(SelectedDepotResources));
                CheckMarket();
                if (colonisationStore.ShoppingList.Contains(Tuple.Create(known.MarketID, known.SystemAddress, known.StationName)))
                {
                    ShoppingList.PopulateResources(fcDataStore.CarrierData, sharedData.MarketPurchases);
                    OnPropertyChanged(nameof(ShoppingListResources));
                }
                return;
            }

            //Must be new or one we missed?
            OnNewDepot(sender, e);
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e == false)
                return;

            if(colonisationStore.CommanderSystems.Any())
            {
                foreach (var system in colonisationStore.CommanderSystems)
                {
                   var newSystem = new CommanderSystemVM(system.SystemAddress, system.SystemName);
                    commanderSystems.AddItem(newSystem);
                }
            }

            if (colonisationStore.Depots.Any())
            {
   
                foreach (var depot in colonisationStore.Depots)
                {
                    if (depot.Progress >= 1)
                    {
                        depot.Inactive = true;
                    }
                    var newDepot = new ConstructionDepotVM(depot);
                    newDepot.UpdateMostRecentPurchase(sharedData.MarketPurchases);
                    Depots.AddItem(newDepot);

                    if (depot.Inactive)
                        continue;
                    var cmdrSystem = commanderSystems.FirstOrDefault(x => x.SystemAddress == newDepot.SystemAddress);
                    cmdrSystem?.Depots.AddItem(newDepot);
                }

                WatchedMarkets.ClearCollection();
                WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));

                OnPropertyChanged(nameof(ActiveDepots));
                OnPropertyChanged(nameof(InactiveDepots));
            }

            if (colonisationStore.ShoppingList.Count > 0)
            {
                var depots = Depots.Where(x => colonisationStore.ShoppingList.Contains(Tuple.Create(x.MarketID, x.SystemAddress, x.StationName)));

                if (depots.Any())
                {
                    ShoppingList.AddDepots(depots, fcDataStore.CarrierData, sharedData.MarketPurchases);
                    OnPropertyChanged(nameof(ShoppingListResources));
                }
            }
            SelectedDepot = Depots.Where(x => x.Inactive == false).LastOrDefault();
            SelectedCommanderSystem = commanderSystems.LastOrDefault();
            OnModelLive(true);
        }

        private void OnMarketEvent(object? sender, StationMarket? e)
        {
            CheckMarket();
        }

        private void CheckMarket()
        {
            if(SelectedDepotTab == 0 && selectedDepot == null  || SelectedDepotTab == 1 && ShoppingList.Resources.Count == 0 || sharedData.CurrentMarket == null)
                return; 

            var market = StationMarketVM.CreateEmptyItemMarket(sharedData.CurrentMarket);

            foreach(var item in sharedData.CurrentMarket.ItemsForSale)
            {
                //Carrier market data includes items which are sold out as it doesn't cancel the sell order when you do
                //So if the stock level is 0 then we just ignore it
                if (item.Stock <= 0)
                    continue;

                //Selected Depot
                if (SelectedDepotTab == 0 && selectedDepot != null)
                {
                    var required = selectedDepot.FilteredResources.FirstOrDefault(x => x.FDEVName == item.Name && x.RemainingCount > 0);

                    if (required == null)
                        continue;
                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required?.RemainingCount ?? 0,
                        CarrierStockValue = required?.CarrierStockValue ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                    continue;
                }

                if (SelectedDepotTab == 1 && ShoppingList.Resources.Count > 0)
                {
                    var required = ShoppingList.Resources.FirstOrDefault(x => x.FDEVName == item.Name && x.RemainingCount > 0);

                    if (required == null)
                        continue;

                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required?.RemainingCount ?? 0,
                        CarrierStockValue = required?.CarrierStockValue ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                }

                if (SelectedDepotTab == 2 && SelectedWatchedMarket != null)
                {
                    var required = SelectedWatchedMarketCommodities?.FirstOrDefault(x => string.Equals(x.Name, item.Name) && x.Demand > 0);

                    if (required == null)
                        continue;

                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required?.Demand ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                }
            }

            Application.Current.Dispatcher.Invoke(() => CurrentMarket = market);
        }

        private void OnWatchedMarketsUpdated(object? sender, List<WatchedMarket> e)
        {
            WatchedMarkets.ClearCollection();
            WatchedMarkets.AddRange(e.Select(x => new WatchedMarketVM(x)));
            SelectedWatchedMarket ??= WatchedMarkets.FirstOrDefault();
        }

        private void OnWatchedMarketUpdated(object? sender, WatchedMarket e)
        {
            //TODO update just the market
            WatchedMarkets.ClearCollection();
            WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));
            SelectedWatchedMarket ??= WatchedMarkets.FirstOrDefault(x => x.MarketId == e.MarketID);
        }

        private void OnShipChanged(object? sender, ShipInfo? e)
        {
            CurrentShip = e == null ? null : new(e);
            OnCargoUpdated(null, sharedData.CurrentShipCargo);
        }

        private void OnCargoUpdated(object? sender, IEnumerable<ShipCargo>? e)
        {
            CurrentShip?.OnCargoUpdated(e);
        }

        private void OnReleaseCommandSystem(object? sender, CommanderSystem e)
        {
            var cmdrSystem = commanderSystems.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);

            if (cmdrSystem != null)
            {
                commanderSystems.RemoveItem(cmdrSystem);
                SelectedCommanderSystem = commanderSystems.LastOrDefault();
            }
        }

        private void OnNewCommanderSystem(object? sender, CommanderSystem e)
        {
            var cmdrSystem = commanderSystems.FirstOrDefault(x => x.SystemAddress == e.SystemAddress);

            if (cmdrSystem == null)
            {
                cmdrSystem = new(e.SystemAddress, e.SystemName);
                commanderSystems.AddItem(cmdrSystem);
                SelectedCommanderSystem = cmdrSystem;
            }
        }
        private void SetSelectedCommanderSystem(CommanderSystemVM? vM)
        {
            SelectedCommanderSystem = vM;
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
            if(SelectedDepot == null || SelectedDepotResources == null)
            {
                return;
            }

            foreach(var item in SelectedDepotResources)
            {
                var onCarrier = e.Stock.FirstOrDefault(x => string.Equals(x.commodity.FdevName, item.FDEVName, StringComparison.OrdinalIgnoreCase)
                                    && x.Stolen == false);

                if (onCarrier == null)
                {
                    item.SetCarrierStock(0);
                    continue;
                }
                item.SetCarrierStock(onCarrier.StockCount);
            }

            ShoppingList.UpdateCarrierStock(e);
            CheckMarket();
            OnPropertyChanged(nameof(ShoppingListResources));
            OnPropertyChanged(nameof(SelectedDepotResources));
        }

        public void CopyToClipboard(string? value)
        {
            notification.SetClipboard(value);
        }

        private void ColonisationSettings_CommoditySortingChanged(object? sender, CommoditySorting e)
        {
            OnPropertyChanged(nameof(CommoditySorting));
            OnPropertyChanged(nameof(SelectedDepotResources));
        }

        private void OnAddShoppingList(ConstructionDepotVM? depot)
        {
            if (depot == null)
            {
                return;
            }

            if(colonisationStore.SetDepotShoppingState(depot))
            {
                ShoppingList.AddDepot(depot, fcDataStore.CarrierData, sharedData.MarketPurchases);
                OnPropertyChanged(nameof(ShoppingListResources));
                return;
            }

            ShoppingList.RemoveDepot(depot, fcDataStore.CarrierData, sharedData.MarketPurchases);
            OnPropertyChanged(nameof(ShoppingListResources));
        }

        private void OnAddRemoveWatchedMarket(ulong obj)
        {

            sharedData.AddRemoveWatchedMarket(obj);
        }

        private void OnPurchasesUpdated(object? sender, CommodityPurchase e)
        {
            foreach(var depot in Depots)
            {
                depot.UpdateMostRecentPurchase(sharedData.MarketPurchases);
            }

            ShoppingList.UpdateMostRecentPurchase(sharedData.MarketPurchases);
        }

        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            OnPropertyChanged(nameof(Purchases));
        }
    }
}
