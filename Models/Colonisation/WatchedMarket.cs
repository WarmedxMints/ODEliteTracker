using EliteJournalReader.Events;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models.Market;
using ODMVVM.Helpers.IO;

namespace ODEliteTracker.Models.Colonisation
{
    public sealed class WatchedMarket
    {
       public WatchedMarket(MarketInfo marketInfo)
        {
            Name = marketInfo.StationName;
            MarketID = marketInfo.MarketID;
            ItemsForPurchase = [.. marketInfo.Items.Where(x => x.Demand > 0).Select(x => new StationCommodity(x))];
        }

        public WatchedMarket(WatchedMarketDTO watchedMarketDTO)
        {
            Name = watchedMarketDTO.Name;
            MarketID = watchedMarketDTO.MarketID;
            try
            {
                ItemsForPurchase = Json.DeserialiseJsonFromString<List<StationCommodity>>(watchedMarketDTO.CommoditiesJson) ?? [];

                var total = ItemsForPurchase.FirstOrDefault(x => x.Name == "Total");
                if (total == null)
                    return;

                ItemsForPurchase.Remove(total);
            }
            catch
            {
                ItemsForPurchase = [];
            }
        }

        public WatchedMarket(StationMarket currentMarket)
        {
            Name = currentMarket.StationName;
            MarketID = currentMarket.MarketID;
            ItemsForPurchase = [.. currentMarket.ItemsForPurchase.Select(x => new StationCommodity(x))];
        }

        public string Name { get; set; }
        public ulong MarketID { get; set; }
        public List<StationCommodity> ItemsForPurchase { get; set; } = [];

        internal void UpdatePurchaseOrders(MarketInfo market)
        {
            ItemsForPurchase.Clear();

            foreach (var item in market.Items)
            {
                if (item.Demand > 0)
                {
                    ItemsForPurchase.Add(new StationCommodity(item));
                }
            }
        }
    }
}
