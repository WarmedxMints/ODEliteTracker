﻿using EliteJournalReader.Events;

namespace ODEliteTracker.Models.Market
{
    public sealed class StationCommodity
    {
        public StationCommodity(MarketItem item)
        {
            Name = item.Name;
            Name_Localised = item.Name_Localised;
            Category = item.Category;
            Category_Localised = item.Category_Localised;
            BuyPrice = item.BuyPrice;
            SellPrice = item.SellPrice;
            Stock = item.Stock;
            Demand = item.Demand;
        }

        public string Name { get; set; }
        public string Name_Localised { get; set; }
        public string Category { get; set; }
        public string Category_Localised { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int Stock { get; set; }
        public int Demand { get; set; }
    }
}
