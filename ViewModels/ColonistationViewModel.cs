using ODEliteTracker.Helpers;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Colonisation.Builds;
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
using ODMVVM.Helpers;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
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
                                     NotificationService notification,
                                     PopOutService popOutService)
        {
            this.colonisationStore = colonisationStore;
            this.sharedData = sharedDataStore;
            this.fcDataStore = fcDataStore;
            this.settings = settings;
            this.notification = notification;
            this.popOutService = popOutService;
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
            this.sharedData.StoreLive += OnSharedDataLive;
            ColonisationBuildTotals = new(this.fcDataStore, this.sharedData);
            
            if (this.sharedData.IsLive)
            {
                OnSharedDataLive(null, true);           
            }

            SetSelectedDepotCommand = new ODRelayCommand<ConstructionDepotVM?>(SetSelectedDepot);
            SetSelectedCommanderSystemCommand = new ODRelayCommand<CommanderSystemVM?>(SetSelectedCommanderSystem);
            SetActiveStateCommand = new ODRelayCommand<ConstructionDepotVM>(SetDepotActiveState);
            CreatePostCommand = new ODRelayCommand<ColonisationPostType>(CreatePost);
            SetClipboardCommand = new ODRelayCommand<string>(CopyToClipboard);
            AddShoppingListCommand = new ODRelayCommand<ConstructionDepotVM?>(OnAddShoppingList);
            AddRemoveWatchedMarketsCommand = new ODRelayCommand<ulong>(OnAddRemoveWatchedMarket);
            OpenPopOut = new ODRelayCommand<Type>(OnOpenPopOut);
            SelectBuild = new ODRelayCommand<ColonisationBuild>(OnSetBuildType);
            AddSelectedToWishList = new ODRelayCommand(OnAddSelectedBuildToWishList);
            RemoveSelectedFromWishList = new ODRelayCommand(OnRemoveSelectedFromWishList);
            AddBuildToWishList = new ODRelayCommand<ColonisationBuild>(OnAddBuildToWishList);
            RemoveBuildFromWishList = new ODRelayCommand<ColonisationBuild>(OnRemoveBuildFromWishList);
            AssetFilterCommand = new ODRelayCommand<AssetFilter>(OnSetAssetFilter);

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
            this.sharedData.StoreLive -= OnSharedDataLive;

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
        private readonly PopOutService popOutService;
        #endregion

        #region Commands
        public ICommand SetSelectedDepotCommand { get; }
        public ICommand SetActiveStateCommand { get; }
        public ICommand SetSelectedCommanderSystemCommand { get; }
        public ICommand CreatePostCommand { get; }
        public ICommand SetClipboardCommand { get; }
        public ICommand AddShoppingListCommand { get; }
        public ICommand AddRemoveWatchedMarketsCommand { get; }
        public ICommand OpenPopOut { get; }
        public ICommand SelectBuild { get; }
        public ICommand AddSelectedToWishList { get; }
        public ICommand RemoveSelectedFromWishList { get; }
        public ICommand AddBuildToWishList { get; }
        public ICommand RemoveBuildFromWishList { get; }
        public ICommand AssetFilterCommand { get; }
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

        public BuildItemSorting BuildItemSorting
        {
            get => settings.ColonisationSettings.BuildTotalSorting;
            set
            {
                settings.ColonisationSettings.BuildTotalSorting = value;
                ColonisationBuildTotals.SortLists(value);
            }
        }

        public ColonisationBuild SelectedBuild
        {
            get => settings.ColonisationSettings.SelectedBuild;
            set => settings.ColonisationSettings.SelectedBuild = value;
        }

        private long activeSelectedDepot = 0;
        private long inactiveSelectedDepot = 0;
        private long commanderSelectedSystem = 0;

        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                tabIndex = value;
                if (tabIndex == 0)
                    SelectedDepot = Depots.FirstOrDefault(x => x.MarketID == activeSelectedDepot) ?? SelectedDepot;
                if (tabIndex == 1)
                    SelectedDepot = InactiveDepots.FirstOrDefault(x => x.MarketID == inactiveSelectedDepot) ?? SelectedDepot;
                if (tabIndex == 2)
                    SelectedDepot = CommanderSystems.FirstOrDefault(x => x.SystemAddress == commanderSelectedSystem)?.Depots.FirstOrDefault() ?? SelectedDepot;

                OnPropertyChanged(nameof(TabIndex));
            }
        }

        public string EstimatedTrips
        {
            get
            {

                if (currentShip == null || currentShip.CargoCapacity <= 0)
                    return "∞";

                var capacity = currentShip.CargoCapacity;
                switch (SelectedDepotTab)
                {
                    case 0:
                        if (selectedDepot == null)
                            return string.Empty;
                        return $"Estimated Trips {Math.Ceiling((double)selectedDepot.Resources.Sum(x => x.RemainingCount) / capacity):N0}";
                    case 1:
                        if (ShoppingList == null || ShoppingList.Depots.Count == 0)
                            return string.Empty;
                        return $"Estimated Trips {Math.Ceiling((double)ShoppingList.Resources.Sum(x => x.RemainingCount) / capacity):N0}";
                    default:
                        return string.Empty;

                }
            }
        }

        public ObservableCollection<ConstructionTotalsVM> ConstructionTotals { get; } = [];
        public ObservableCollection<ConstructionDepotVM> Depots { get; } = [];
        public ColonisationShoppingList ShoppingList { get; } = new();
        public IEnumerable<ConstructionDepotVM> ActiveDepots => Depots.Where(x => x.Inactive == false);
        public IEnumerable<ConstructionDepotVM> InactiveDepots => Depots.Where(x => x.Inactive == true && x.ProgressValue < 1);

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
                    CommoditySorting.Category => selectedWatchedMarket?.Commodities.OrderBy(x => x.Category).ThenBy(x => x.Name),
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
                if (selectedDepot != null)
                {
                    if (tabIndex == 0)
                        activeSelectedDepot = selectedDepot.MarketID;
                    if (tabIndex == 1)
                        inactiveSelectedDepot = selectedDepot.MarketID;
                }
                CheckMarket();
                OnFcStoreLive(null, true);
                OnPropertyChanged(nameof(SelectedDepot));
                OnPropertyChanged(nameof(ActiveButtonText));
                OnPropertyChanged(nameof(SelectedDepotResources));
                OnPropertyChanged(nameof(EstimatedTrips));
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
        public IEnumerable<CommanderSystemVM> CommanderSystems => commanderSystems;

        private CommanderSystemVM? selectedCommanderSystem;
        public CommanderSystemVM? SelectedCommanderSystem
        {
            get => selectedCommanderSystem;
            set
            {
                selectedCommanderSystem = value;
                if (selectedCommanderSystem != null)
                {
                    commanderSelectedSystem = selectedCommanderSystem.SystemAddress;
                    SelectedDepot = selectedCommanderSystem?.Depots.FirstOrDefault();
                }
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
                OnPropertyChanged(nameof(EstimatedTrips));
            }
        }

        public ColonisationBuildTotalsVM ColonisationBuildTotals { get; private set; }
        #endregion

        private void CreatePost(ColonisationPostType type)
        {
            //Single Depot
            if (SelectedDepotTab == 0)
            {
                if (SelectedDepot is null)
                    return;

                if (type == ColonisationPostType.CSV)
                {
                    var filename = ODDialogService.SaveFileDialog("Save CSV File", "csv files (*.csv)|*.csv", $"{SelectedDepot.StationNameSplit}.csv");

                    if (filename != null && DiscordPostCreator.CreateDepotCSV(SelectedDepotResources) is Tuple<bool, string> tuple && tuple.Item1)
                    {
                        WriteToFile(filename, tuple);
                    }
                    return;
                }
                if (DiscordPostCreator.CreateColonisationPost(SelectedDepot, SelectedDepotResources, type))
                {
                    DiscordButtonText = "Post Created";
                    notification.ShowBasicNotification(new("Clipboard", ["Construction Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                    Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                }
                return;
            }
            //Shopping List
            if (SelectedDepotTab == 1)
            {
                if (ShoppingList is null || ShoppingList.Depots.Count == 0)
                    return;

                if (type == ColonisationPostType.CSV)
                {
                    var names = ShoppingList.Depots.Select(x => x.StationNameSplit);

                    var filename = ODDialogService.SaveFileDialog("Save CSV File", "csv files (*.csv)|*.csv", $"{string.Join("-", names)}.csv");

                    if (filename != null && DiscordPostCreator.CreateDepotCSV(ShoppingListResources) is Tuple<bool, string> tuple && tuple.Item1)
                    {
                        WriteToFile(filename, tuple);
                    }
                    return;
                }

                if (DiscordPostCreator.CreateColonisationPost(ShoppingList, ShoppingListResources, type))
                {
                    DiscordButtonText = "Post Created";
                    notification.ShowBasicNotification(new("Clipboard", ["Construction Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                    Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                }
            }
            //Build Totals
            if (SelectedDepotTab == 3)
            {
                if (type == ColonisationPostType.CSV)
                {
                    var filename = ODDialogService.SaveFileDialog("Save CSV File", "csv files (*.csv)|*.csv", $"{SelectedBuild}.csv");

                    if (filename != null && DiscordPostCreator.CreateBuildCsv(ColonisationBuildTotals.WishListItems) is Tuple<bool, string> tuple && tuple.Item1)
                    {
                        WriteToFile(filename, tuple);
                    }
                    return;
                }

                if (DiscordPostCreator.CreateBuildPost(SelectedBuild.GetEnumDescription(), ColonisationBuildTotals.Items, type))
                {
                    DiscordButtonText = "Post Created";
                    notification.ShowBasicNotification(new("Clipboard", ["Build Total Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                    Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                }
            }
            //Build Wishlist
            if (SelectedDepotTab == 4)
            {
                if (type == ColonisationPostType.CSV)
                {
                    var wishlistNames = ColonisationBuildTotals.WishListBuilds.Select(x => x.Build.ToString());

                    var filename = ODDialogService.SaveFileDialog("Save CSV File", "csv files (*.csv)|*.csv", $"{string.Join("-", wishlistNames)}.csv");

                    if (filename != null && DiscordPostCreator.CreateBuildCsv(ColonisationBuildTotals.WishListItems) is Tuple<bool, string> tuple && tuple.Item1)
                    {
                        WriteToFile(filename, tuple);
                    }
                    return;
                }

                
                if (DiscordPostCreator.CreateBuildPost(GetWishListBuildNames(), ColonisationBuildTotals.WishListItems, type))
                {
                    DiscordButtonText = "Post Created";
                    notification.ShowBasicNotification(new("Clipboard", ["Build Wishlist Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                    Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                }
            }
        }

        private string GetWishListBuildNames()
        {
            var builds = ColonisationBuildTotals.WishListBuilds.Select(x => $"{x.BuildName} x{x.Count:N0}");

            return string.Join("\n", builds);
        }

        private static void WriteToFile(string filename, Tuple<bool, string> tuple)
        {
            try
            {
                File.WriteAllText(filename, tuple.Item2);
            }
            catch (Exception)
            {
                _ = ODDialogService.ShowNoOwner("Error", "Error Saving csv");
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
            //Update Totals
            ConstructionTotals.ClearCollection();

            var sortedDict = from entry in colonisationStore.ConstructionTotals orderby entry.Key ascending select entry;

            foreach (var kvp in sortedDict)
            {
                ConstructionTotals.AddItem(new(kvp.Key, kvp.Value));
            }

            ConstructionTotals.AddItem(new("Total", sortedDict.Sum(x => x.Value)));

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

                if (known.Complete == false && known.Inactive)
                {
                    SetDepotActiveState(known);
                }

                OnPropertyChanged(nameof(SelectedDepotResources));
                OnPropertyChanged(nameof(EstimatedTrips));
                CheckMarket();
                if (colonisationStore.ShoppingList.Contains(Tuple.Create(known.MarketID, known.SystemAddress)))
                {
                    ShoppingList.PopulateResources(fcDataStore.CarrierData, sharedData.MarketPurchases);
                    OnPropertyChanged(nameof(ShoppingListResources));
                }
                return;
            }

            //Must be new or one we missed?
            OnNewDepot(sender, e);
        }

        private void OnSharedDataLive(object? sender, bool e)
        {
            if (this.sharedData.CurrentShipInfo != null)
                OnShipChanged(null, this.sharedData.CurrentShipInfo);
            if (this.sharedData.CurrentShipCargo != null)
                OnCargoUpdated(null, this.sharedData.CurrentShipCargo);

            ColonisationBuildTotals.OnSharedDataLive();
        }
        private void OnStoreLive(object? sender, bool e)
        {
            if (e == false)
                return;

            //Update Totals
            ConstructionTotals.ClearCollection();

            var sortedDict = from entry in colonisationStore.ConstructionTotals orderby entry.Key ascending select entry;

            foreach (var kvp in sortedDict)
            {
                ConstructionTotals.AddItem(new(kvp.Key, kvp.Value));
            }

            ConstructionTotals.AddItem(new("Total", sortedDict.Sum(x => x.Value)));

            if (colonisationStore.CommanderSystems.Any())
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

                    var cmdrSystem = commanderSystems.FirstOrDefault(x => x.SystemAddress == newDepot.SystemAddress);
                    cmdrSystem?.Depots.AddItem(newDepot);
                }

                WatchedMarkets.ClearCollection();
                WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));
                SelectedWatchedMarket = WatchedMarkets.FirstOrDefault();
                OnPropertyChanged(nameof(ActiveDepots));
                OnPropertyChanged(nameof(InactiveDepots));
            }

            if (colonisationStore.ShoppingList.Count > 0)
            {
                var depots = Depots.Where(x => colonisationStore.ShoppingList.Contains(Tuple.Create(x.MarketID, x.SystemAddress)));

                if (depots.Any())
                {
                    ShoppingList.AddDepots(depots, fcDataStore.CarrierData, sharedData.MarketPurchases);
                    OnPropertyChanged(nameof(ShoppingListResources));
                }
            }
            SelectedDepot = Depots.FirstOrDefault(x => x.MarketID == colonisationStore.LastDockedDepotID && x.Inactive == false) ?? Depots.Where(x => x.Inactive == false).LastOrDefault();
            selectedCommanderSystem = commanderSystems.LastOrDefault();
            OnPropertyChanged(nameof(SelectedCommanderSystem));
            ColonisationBuildTotals.SortLists(settings.ColonisationSettings.BuildTotalSorting);
            ColonisationBuildTotals.BuildItemList(SelectedBuild);
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
                //Shopping List Tab
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
                //Market Wishlist Tab
                if (SelectedDepotTab == 2 && SelectedWatchedMarket != null)
                {
                    var required = SelectedWatchedMarketCommodities?.FirstOrDefault(x => string.Equals(x.Name, item.Name) && x.Demand > 0);

                    if (required == null)
                        continue;

                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required?.Demand ?? 0,
                        CarrierStockValue = required?.CarrierStockValue ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                    continue;
                }
                //Build Totals Tab
                if (SelectedDepotTab == 3)
                {
                    var required = ColonisationBuildTotals.Items?.FirstOrDefault(x => string.Equals(x.FDEVName, item.Name));

                    if (required == null)
                        continue;

                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required.RequiredAmount,
                        CarrierStockValue = required?.CarrierStockValue ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                    continue;
                }
                //Build Wishlist Tab
                if (SelectedDepotTab == 4)
                {
                    var required = ColonisationBuildTotals.WishListItems?.FirstOrDefault(x => string.Equals(x.FDEVName, item.Name));

                    if (required == null)
                        continue;

                    var commodity = new StationCommodityVM(item, true)
                    {
                        Required = required.RequiredAmount,
                        CarrierStockValue = required?.CarrierStockValue ?? 0
                    };

                    market.ItemsForSale.Add(commodity);
                    continue;
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
            OnPropertyChanged(nameof(EstimatedTrips));
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
            ColonisationBuildTotals.OnCarrierStockUpdated(e);
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
                OnPropertyChanged(nameof(EstimatedTrips));
                return;
            }

            ShoppingList.RemoveDepot(depot, fcDataStore.CarrierData, sharedData.MarketPurchases);
            OnPropertyChanged(nameof(ShoppingListResources));
            OnPropertyChanged(nameof(EstimatedTrips));
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

        private void OnOpenPopOut(Type type)
        {
            popOutService.OpenPopOut(type, settings.SelectedCommanderID);
        }

        private void OnSetBuildType(ColonisationBuild build)
        {
            SelectedBuild = build;
            ColonisationBuildTotals.BuildItemList(build);
        }

        private void OnAddSelectedBuildToWishList(object? obj)
        {
            ColonisationBuildTotals.AddSelectedToWishList();
        }

        private void OnRemoveSelectedFromWishList(object? obj)
        {
            ColonisationBuildTotals.RemoveSelectedFromWishList();
        }

        private void OnRemoveBuildFromWishList(ColonisationBuild build)
        {
            ColonisationBuildTotals.RemoveFromWishlist(build);
        }

        private void OnAddBuildToWishList(ColonisationBuild build)
        {
            ColonisationBuildTotals.AddToWishlist(build);
        }

        private void OnSetAssetFilter(AssetFilter filter)
        {
            if(ColonisationBuildTotals.AssetFilter.HasFlag(filter))
            {
                ColonisationBuildTotals.AssetFilter &= ~filter;
                return;
            }

            ColonisationBuildTotals.AssetFilter |= filter;
        }
    }
}
