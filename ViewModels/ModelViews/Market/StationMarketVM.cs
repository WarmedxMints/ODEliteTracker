﻿using ODEliteTracker.Models.Market;
using ODMVVM.ViewModels;

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
        public List<StationCommodityVM> ItemsForSale { get; set; }

        public static StationMarketVM CreateEmptyItemMarket(StationMarket market)
        {
            return new(market.MarketID, market.StationName, market.StarSystem);
        }
    }
}
