using NetTopologySuite.GeometriesGraph;
using ODEliteTracker.Controls;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class MarketWatchListPopOutViewModel : PopOutViewModel
    {
        public MarketWatchListPopOutViewModel(ColonisationStore store, SharedDataStore sharedData, FleetCarrierDataStore fcDataStore, SettingsStore settings)
        {
            this.colonisationStore = store;
            this.sharedData = sharedData;
            this.settings = settings;

            this.colonisationStore.StoreLive += OnStoreLive;
            this.sharedData.WatchedMarketUpdated += OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated += OnWatchedMarketsUpdated;
            this.sharedData.MarketEvent += (_, _) => CheckMarket();
            this.settings.ColonisationSettings.WatchedMarketSortingChanged += ColonisationSettings_WatchedMarketSortingChanged;

            AddRemoveWatchedMarketsCommand = new ODRelayCommand<ulong>(OnAddRemoveWatchedMarket);

            if (colonisationStore.IsLive)
            {
                OnStoreLive(null, true);
            }
        }

        protected override void Dispose()
        {
            this.colonisationStore.StoreLive -= OnStoreLive;

            this.sharedData.MarketEvent -= (_, _) => CheckMarket();

            this.sharedData.WatchedMarketUpdated -= OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated -= OnWatchedMarketsUpdated;
            this.settings.ColonisationSettings.WatchedMarketSortingChanged -= ColonisationSettings_WatchedMarketSortingChanged;
        }

        private readonly ColonisationStore colonisationStore;
        private readonly SharedDataStore sharedData;
        private readonly SettingsStore settings;

        public override string Name => "Market Watchlist";

        public override bool IsLive => colonisationStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/watchlist.png", UriKind.Relative);

        public ICommand AddRemoveWatchedMarketsCommand { get; }

        public CommoditySorting CommoditySorting
        {
            get => settings.ColonisationSettings.ColonisationCommoditySorting;
            set
            {
                settings.ColonisationSettings.ColonisationCommoditySorting = value;
                OnPropertyChanged(nameof(CurrentMarketItems));
            }
        }

        public string HeaderText => IsMouseOver ? "Markets" : selectedWatchedMarket?.Name ?? string.Empty;

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

        public CommoditySorting WatchedMarketSorting
        {
            get => settings.ColonisationSettings.WatchedMarketSorting;
            set
            {
                settings.ColonisationSettings.WatchedMarketSorting = value;
                OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
            }
        }



        private void ColonisationSettings_WatchedMarketSortingChanged(object? sender, CommoditySorting e)
        {
            OnPropertyChanged(nameof(WatchedMarketSorting));
            OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
        }

        private void CheckMarket()
        {
            if (sharedData.CurrentMarket == null)
                return;

            var market = StationMarketVM.CreateEmptyItemMarket(sharedData.CurrentMarket);

            foreach (var item in sharedData.CurrentMarket.ItemsForSale)
            {
                //Carrier market data includes items which are sold out as it doesn't cancel the sell order when you do
                //So if the stock level is 0 then we just ignore it
                if (item.Stock <= 0)
                    continue;

                var required = SelectedWatchedMarketCommodities?.FirstOrDefault(x => string.Equals(x.Name, item.Name) && x.Demand > 0);

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

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                WatchedMarkets.ClearCollection();
                WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));
                SelectedWatchedMarket = WatchedMarkets.FirstOrDefault();
                OnPropertyChanged(nameof(HeaderText));
                OnModelLive();
            }
        }

        private void OnWatchedMarketUpdated(object? sender, WatchedMarket e)
        {
            //TODO update just the market
            WatchedMarkets.ClearCollection();
            WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));
            SelectedWatchedMarket ??= WatchedMarkets.FirstOrDefault(x => x.MarketId == e.MarketID);
        }

        private void OnWatchedMarketsUpdated(object? sender, List<WatchedMarket> e)
        {
            WatchedMarkets.ClearCollection();
            WatchedMarkets.AddRange(e.Select(x => new WatchedMarketVM(x)));
            SelectedWatchedMarket ??= WatchedMarkets.FirstOrDefault();
        }

        private void OnAddRemoveWatchedMarket(ulong obj)
        {
            sharedData.AddRemoveWatchedMarket(obj);
        }

        public override void OnMouseEnter_Leave(bool mouseLeave)
        {
            base.OnMouseEnter_Leave(mouseLeave);
            OnPropertyChanged(nameof(HeaderText));  
        }
    }
}
