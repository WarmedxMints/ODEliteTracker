using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class WatchedMarketVM(WatchedMarket watchedMarket) : ODObservableObject
    {
        private readonly WatchedMarket watchedMarket = watchedMarket;

        public string Name => watchedMarket.Name;
        public ulong MarketId => watchedMarket.MarketID;
        public IEnumerable<StationCommodityVM> Commodities => watchedMarket.ItemsForPurchase.Select(x => new StationCommodityVM(x, false));
    }
}
