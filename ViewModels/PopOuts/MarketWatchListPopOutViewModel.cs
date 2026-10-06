using Newtonsoft.Json.Linq;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class MarketWatchListPopOutViewModel : PopOutViewModel
    {
        public MarketWatchListPopOutViewModel(ColonisationStore store, SharedDataStore sharedData, SettingsStore settings, MarketWatchListController watchListController)
        {
            this.colonisationStore = store;
            this.sharedData = sharedData;
            this.settings = settings;
            this.watchListController = watchListController;
            this.colonisationStore.StoreLive += OnStoreLive;

            this.watchListController.RecheckMarket += OnWatchlistRecheckMarket;
            this.watchListController.UpdateWatchListMarket += OnUpdateWatchlistMarket;

            OnToggleStationMarket = new ODRelayCommand<bool>(ToggleStationMarket);
            ToggleCategoryColumnCommand = new ODRelayCommand<bool>(ToggleCategoryColumn);

            this.watchListController.PropertyChanged += OnWatchListPropertyChanged;
            if (colonisationStore.IsLive)
            {
                _ = OnStoreLive(null, true);
            }
        }

        protected override void Dispose()
        {
            this.colonisationStore.StoreLive -= OnStoreLive;

            this.sharedData.MarketEvent -= (_, _) => CheckMarket();

            this.watchListController.RecheckMarket += OnWatchlistRecheckMarket;
            this.watchListController.UpdateWatchListMarket += OnUpdateWatchlistMarket;
        }

        private readonly ColonisationStore colonisationStore;
        private readonly SharedDataStore sharedData;
        private readonly SettingsStore settings;
        private readonly MarketWatchListController watchListController;

        public override string Name => "Market Watchlist";

        public override bool IsLive => colonisationStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/watchlist.png", UriKind.Relative);
                public ICommand OnToggleStationMarket { get; }
        public ICommand ToggleCategoryColumnCommand { get; }
        public CommoditySorting CommoditySorting
        {
            get => settings.ColonisationSettings.ColonisationCommoditySorting;
            set
            {
                settings.ColonisationSettings.ColonisationCommoditySorting = value;
                OnPropertyChanged(nameof(CurrentMarketItems));
            }
        }

        public string HeaderText => IsMouseOver ? "Markets" : WatchListController.SelectedWatchedMarket?.Name ?? string.Empty;

        public MarketWatchListController WatchListController => watchListController;
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

        private bool stationMarketVisible;
        public bool StationMarketVisible
        {
            get => stationMarketVisible;
            set
            {
                stationMarketVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StationMarketVisibilty));
                OnPropertyChanged(nameof(StationMarketRowGridLength));
            }
        }

        public Visibility StationMarketVisibilty => StationMarketVisible ? Visibility.Visible : Visibility.Collapsed;

        public GridLength StationMarketRowGridLength => StationMarketVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(30);
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

        public Visibility CategoryColumnVisibilty { get; private set; } = Visibility.Visible;        

        private void CheckMarket()
        {
            if (sharedData.CurrentMarket == null || WatchListController.SelectedWatchedMarket?.MarketId != sharedData.CurrentMarket.MarketID)
                return;

            var market = StationMarketVM.CreateEmptyItemMarket(sharedData.CurrentMarket);

            foreach (var item in sharedData.CurrentMarket.ItemsForSale)
            {
                //Carrier market data includes items which are sold out as it doesn't cancel the sell order when you do
                //So if the stock level is 0 then we just ignore it
                if (item.Stock <= 0)
                    continue;

                var required = WatchListController.SelectedWatchedMarketCommodities?.FirstOrDefault(x => string.Equals(x.Name, item.Name) && x.Demand > 0);

                if (required == null)
                    continue;

                var commodity = new StationCommodityVM(item, true)
                {
                    Required = required?.Demand ?? 0
                };

                market.ItemsForSale.Add(commodity);
            }

            Application.Current.Dispatcher.Invoke(() => CurrentMarket = market);
        }

        private async Task OnStoreLive(object? sender, bool e)
        {
            if (e)
            {  
                OnPropertyChanged(nameof(HeaderText));
                CheckMarket();
                OnModelLive();
            }
        }

        private void OnUpdateWatchlistMarket(object? sender, WatchedMarketVM e)
        {
            if (WatchListController.SelectedWatchedMarket == null)
                return;

            UpdateCurrentMarketDemandForWatched(e);
        }

        private void UpdateCurrentMarketDemandForWatched(WatchedMarketVM vm)
        {
            CurrentMarket?.UpdateWathedMarketDemands(vm);
            OnPropertyChanged(nameof(CurrentMarketItems));
        }

        private void OnWatchlistRecheckMarket(object? sender, EventArgs e)
        {
            CheckMarket();
        }

        private void ToggleCategoryColumn(bool obj)
        {
            CategoryColumnVisibilty = obj ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged(nameof(CategoryColumnVisibilty));
        }

        public override void OnMouseEnter_Leave(bool mouseLeave)
        {
            base.OnMouseEnter_Leave(mouseLeave);
            OnPropertyChanged(nameof(HeaderText));  
        }

        private void ToggleStationMarket(bool visiblt)
        {
            StationMarketVisible = visiblt;
        }

        protected override void ParamsUpdated()
        {
            var settings = AdditionalSettings?.ToObject<MarketWatchlistPopOutSettings>() ?? new();
            StationMarketVisible = settings.ShowMarket;
            CategoryColumnVisibilty = settings.CategoryColumnVisibilty;
        }

        internal override JObject? GetAdditionalSettings()
        {
            return JObject.FromObject(new MarketWatchlistPopOutSettings()
            {
                ShowMarket = StationMarketVisible,
                CategoryColumnVisibilty = this.CategoryColumnVisibilty
            });
        }

        private void OnWatchListPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case nameof(watchListController.SelectedWatchedMarket):
                    OnPropertyChanged(nameof(HeaderText));
                    break;

            }
        }
    }
}
