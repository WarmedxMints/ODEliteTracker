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

        public StationCommodity(SaleElement item)
        {
            SellPrice = item.Price;
            Stock = item.Stock;
            Name = item.Name ?? "Unknown";
            var commod = EliteCommodityHelpers.GetCommodityFromPartial(item.Name ?? string.Empty, item.Name ?? string.Empty);
            if (commod != null)
            {
                Id = commod.Id;
                Name = commod.FdevName;
                Name_Localised = commod.EnglishName;
                Category = commod.EnglishCategory;
   
            }
        }

        public StationCommodity(Purchase item)
        {
            BuyPrice = item.Price;
            Demand = item.Outstanding;
            Name = item.Name ?? "Unknown";
            Name_Localised = item.LocName ?? "Unknown";
            var commod = EliteCommodityHelpers.GetCommodityFromPartial(item.Name ?? string.Empty, item.LocName ?? string.Empty);
            if (commod != null)
            {
                Id = commod.Id;
                Name = commod.FdevName;
                if (string.IsNullOrEmpty(item.LocName))
                    Name_Localised = commod.EnglishName;
                Category = commod.EnglishCategory;
       
            }
        }

        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Name_Localised { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Category_Localised { get; set; } = string.Empty;
        public int BuyPrice { get; set; }
        public long SellPrice { get; set; }
        public long Stock { get; set; }
        public long Demand { get; set; }
    }
}
