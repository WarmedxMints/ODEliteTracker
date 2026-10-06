using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class MarketWatchListController : ODObservableObject, IDisposable
    {
        private readonly SharedDataStore sharedData;
        private readonly SettingsStore settings;
        private readonly ColonisationStore colonisationStore;

        public MarketWatchListController(SharedDataStore sharedData, SettingsStore settings, ColonisationStore colonisationStore)
        {
            this.sharedData = sharedData;
            this.settings = settings;
            this.colonisationStore = colonisationStore;
            this.sharedData.WatchedMarketUpdated += OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated += OnWatchedMarketsUpdated;
            this.sharedData.Docked += OnDocked;

            this.colonisationStore.StoreLive += OnStoreLive;

            if (colonisationStore.IsLive)
            {
                _ = OnStoreLive(null, true);
            }
            AddRemoveWatchedMarketsCommand = new ODRelayCommand<ulong>(OnAddRemoveWatchedMarket);
        }

        public EventHandler? RecheckMarket;
        public EventHandler<WatchedMarketVM>? UpdateWatchListMarket;
        public CommoditySorting WatchedMarketSorting
        {
            get => settings.ColonisationSettings.WatchedMarketSorting;
            set
            {
                settings.ColonisationSettings.WatchedMarketSorting = value;
                OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
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
                    if (colonisationStore.IsLive)
                        RecheckMarket?.Invoke(this, EventArgs.Empty);
                }
                OnPropertyChanged(nameof(SelectedWatchedMarket));
                OnPropertyChanged(nameof(SelectedWatchedMarketCommodities));
            }
        }

        public IEnumerable<StationCommodityVM>? SelectedWatchedMarketCommodities
        {
            get
            {
                return WatchedMarketSorting switch
                {
                    CommoditySorting.Name => selectedWatchedMarket?.Commodities.OrderBy(x => string.IsNullOrEmpty(x.Category)).ThenBy(x => x.Name),
                    CommoditySorting.Category => selectedWatchedMarket?.Commodities.OrderBy(x => string.IsNullOrEmpty(x.Category)).ThenBy(x => x.Category).ThenBy(x => x.Name),
                    CommoditySorting.Remaining => selectedWatchedMarket?.Commodities.OrderBy(x => string.IsNullOrEmpty(x.Category)).ThenByDescending(x => x.Demand),
                    _ => selectedWatchedMarket?.Commodities,
                };
            }
        }

        #region Commands and Methods
        public ICommand AddRemoveWatchedMarketsCommand { get; }

        private void OnAddRemoveWatchedMarket(ulong obj)
        {
            sharedData.AddRemoveWatchedMarket(obj);
        }
        #endregion

        #region EventHandler Methods
        private async Task OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                WatchedMarkets.ClearCollection();
                WatchedMarkets.AddRange(sharedData.WatchedMarkets.Select(x => new WatchedMarketVM(x)));
                SelectedWatchedMarket = WatchedMarkets.FirstOrDefault();
            }
        }

        private void OnDocked(object? sender, Station? e)
        {
            if (e == null)
                return;

            var known = WatchedMarkets.FirstOrDefault(x => x.MarketId == e.MarketID);

            if (known == null)
                return;

            Application.Current.Dispatcher.Invoke(() => SelectedWatchedMarket = known);
        }

        private void OnWatchedMarketsUpdated(object? sender, WatchedMarketsUpdatedEventArgs args)
        {
            WatchedMarketVM? marketVm = null;

            switch (args.Status)
            {
                case MarketStatus.Added:
                    marketVm = new WatchedMarketVM(args.Market);
                    WatchedMarkets.AddItem(marketVm);
                    break;
                case MarketStatus.Removed:
                    marketVm = WatchedMarkets.FirstOrDefault(x => x.MarketId == args.Market.MarketID);
                    if (marketVm == null)
                        break;
                    WatchedMarkets.RemoveItem(marketVm);
                    marketVm = WatchedMarkets.FirstOrDefault();
                    break;
            }

            SelectedWatchedMarket = marketVm;
        }

        private void OnWatchedMarketUpdated(object? sender, WatchedMarket e)
        {
            var known = WatchedMarkets.FirstOrDefault(x => x.MarketId == e.MarketID);

            if (known == null)
            {
                known = new WatchedMarketVM(e);
                WatchedMarkets.AddItem(known);
                SelectedWatchedMarket = known;
                if (colonisationStore.IsLive)
                    UpdateWatchListMarket?.Invoke(this, known);
                return;
            }

            known.UpdateMarket();
            SelectedWatchedMarket = known;
            if (colonisationStore.IsLive)
                UpdateWatchListMarket?.Invoke(this, known);
        }
        #endregion

        public void Dispose()
        {
            this.sharedData.WatchedMarketUpdated -= OnWatchedMarketUpdated;
            this.sharedData.WatchedMarketsUpdated -= OnWatchedMarketsUpdated;
            this.sharedData.Docked -= OnDocked;
        }
    }


}
