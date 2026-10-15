using EliteJournalReader.Events;
using ODCapi.Models;
using ODMVVM.Helpers;

namespace ODEliteTracker.Models.Market
{
    public sealed class StationCommodity
    {
        public StationCommodity() { }
        public StationCommodity(MarketItem item)
        {
            Id = item.ID;
            Name = item.Name;
            Name_Localised = item.Name_Localised;
            Category = item.Category;
            Category_Localised = item.Category_Localised;
            BuyPrice = item.BuyPrice;
            SellPrice = item.SellPrice;
            Stock = item.Stock;
            Demand = item.Demand;
        }

        public StationCommodity(StationCommodity item)
        {
            Id = item.Id;
            Name = item.Name;
            Name_Localised = item.Name_Localised;
            Category = item.Category;
            Category_Localised = item.Category_Localised;
            BuyPrice = item.BuyPrice;
            SellPrice = item.SellPrice;
            Stock = item.Stock;
            Demand = item.Demand;
        }

        public StationCommodity(MarketCommodity item)
        {            
            Id = item.Id;    
            BuyPrice = item.BuyPrice;
            SellPrice = item.SellPrice;
            Stock = item.Stock;
            Demand = item.Demand;
            Name_Localised = item.LocName;
            var commod = EliteCommodityHelpers.GetCommodityDetailsFromID(item.Id);
            if (commod != null)
            {
                Name = commod.FdevName;             
                Category = commod.EnglishCategory;
                return;
            }
            Name = item.Name;
            Category = item.Categoryname;
        }
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Name_Localised { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Category_Localised { get; set; } = string.Empty;
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int Stock { get; set; }
        public int Demand { get; set; }
    }
}
