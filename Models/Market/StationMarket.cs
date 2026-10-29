using EliteJournalReader.Events;
using ODCapi.Models;

namespace ODEliteTracker.Models.Market
{
    public sealed class StationMarket
    {
        public StationMarket(MarketInfo market)
        {
            MarketID = market.MarketID;
            StationName = market.StationName;
            StarSystem = market.StarSystem;
            ItemsForSale = [.. market.Items.Where(x => x.BuyPrice > 0).Select(x => new StationCommodity(x))];
            ItemsForPurchase = [.. market.Items.Where(x => x.Demand > 0).Select(x => new StationCommodity(x))];
            TimeStamp = market.Timestamp;
        }

        public StationMarket(MarketInfo marketInfo, MarketEvent.MarketEventArgs marketArgs)
        {
            MarketID = marketArgs.MarketID;
            StationName = marketArgs.StationName;
            StarSystem = marketArgs.StarSystem;
            ItemsForSale = [.. marketInfo.Items.Where(x => x.BuyPrice > 0).Select(x => new StationCommodity(x))];
            ItemsForPurchase = [.. marketInfo.Items.Where(x => x.Demand > 0).Select(x => new StationCommodity(x))];
            TimeStamp = marketInfo.Timestamp;
        }

        public StationMarket(CapiMarket market, DockedEvent.DockedEventArgs docked)
        {
            MarketID = docked.MarketID;
            StationName = docked.StationName;
            StarSystem = docked.StarSystem;
            //ItemsForSale = market.Commodities?.Where(x => x.BuyPrice > 0 && x.Categoryname != "NonMarketable").Select(x => new StationCommodity(x)).ToList() ?? []; 
            //ItemsForPurchase = market.Commodities?.Where(x => x.Demand > 0 && x.Categoryname != "NonMarketable").Select(x => new StationCommodity(x)).ToList() ?? [];
            ItemsForSale = market.Orders?.Commodities?.Sales?.Where(x => !x.Blackmarket).Select(x => new StationCommodity(x)).ToList() ?? [];
            ItemsForPurchase = market.Orders?.Commodities?.Purchases?.Where(x => !x.Blackmarket).Select(x => new StationCommodity(x)).ToList() ?? [];
            TimeStamp = docked.Timestamp;
        }

        public DateTime TimeStamp { get; set; }
        public ulong MarketID { get; set; }
        public string StationName { get; set; }
        public string StarSystem { get; set; }
        public List<StationCommodity> ItemsForSale { get; set; }
        public List<StationCommodity> ItemsForPurchase { get; set; }
    }
}
