using ODEliteTracker.Models.Market;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Market
{
    public sealed class StationMarketVM : ODObservableObject
    {
        public StationMarketVM(StationMarket marketInfo)
        {
            MarketID = marketInfo.MarketID;
            StationName = marketInfo.StationName;
            StarSystem = marketInfo.StarSystem;
            ItemsForSale = [.. marketInfo.ItemsForSale.Where(x => x.Stock > 0).Select(x => new StationCommodityVM(x, false))];
        }

        public StationMarketVM(ulong marketId, string stationName, string starSystem)
        {
            MarketID = marketId;
            StationName = stationName;
            StarSystem = starSystem;
            ItemsForSale = [];
        }

        public ulong MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public ObservableCollection<StationCommodityVM> ItemsForSale { get; set; }

        public static StationMarketVM CreateEmptyItemMarket(StationMarket market)
        {
            return new(market.MarketID, market.StationName, market.StarSystem);
        }

        internal void UpdateWathedMarketDemands(WatchedMarketVM known)
        {
            List<StationCommodityVM> itemsToRemove = [];

            foreach (var item in ItemsForSale)
            {
                var knownHasDemand = known.Commodities.FirstOrDefault(x => string.Equals(x.Name, item.Name));

                if (knownHasDemand != null && knownHasDemand.Demand > 0)
                {
                    item.Required = knownHasDemand.Demand;
                    continue;
                }
                itemsToRemove.Add(item);
            }

            if(itemsToRemove.Count > 0)
            {
                ItemsForSale.RemoveRange(itemsToRemove);
            }
        }
    }
}
