using ODEliteTracker.Models.Market;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Market
{
    public sealed class StationCommodityVM : ODObservableObject
    {
        public StationCommodityVM(StationCommodity item, bool required)
        {
            Name = item.Name;
            name_Localised = item.Name_Localised;
            Category = item.Category;
            category_Localised = item.Category_Localised;
            BuyPrice = item.BuyPrice;
            SellPrice = item.SellPrice;
            Stock = item.Stock;
            Demand = item.Demand;
            RequiredResource = required;
        }

        public bool RequiredResource { get; set; }
        public string Name { get; set; }

        private string name_Localised;
        public string Name_Localised 
        { 
            get
            {
                return name_Localised ?? Name;
            }
        }
        public string Category { get; set; }

        private string category_Localised;
        public string Category_Localised
        {
            get
            {
                return category_Localised ?? Category;
            }
        }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int Stock { get; set; }
        public int Demand { get; set; }
        public string DemandString => $"{Demand:N0} t";
        public int Required { get; set; }
        public long CarrierStockValue { get; set; }
        public string CarrierStock => CarrierStockValue > 0 ? $"{CarrierStockValue:N0} t" : string.Empty;
        public string CarrierStockDiff => CarrierStockValue > 0 ? $"{CarrierStockValue - Required:N0} t" : string.Empty;
    }
}
