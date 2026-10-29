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

        private readonly string name_Localised;
        public string Name_Localised 
        { 
            get
            {
                return string.IsNullOrEmpty(name_Localised) ? Name : name_Localised;
            }
        }
        public string Category { get; set; }

        private readonly string category_Localised;
        private long demand;
        private long carrierStockValue;
        private long required;

        public string Category_Localised
        {
            get
            {
                return string.IsNullOrEmpty(category_Localised) ? Category : category_Localised;
            }
        }
        public int BuyPrice { get; set; }
        public long SellPrice { get; set; }
        public long Stock { get; set; }
        public long Demand { get => demand; set { demand = value; OnPropertyChanged(); } }
        public string DemandString => $"{Demand:N0} t";
        public long Required { get => required; set { required = value; OnPropertyChanged(); } }
        public long CarrierStockValue 
        { 
            get => carrierStockValue; 
            set 
            { 
                carrierStockValue = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(CarrierStock));
                OnPropertyChanged(nameof(CarrierStockDiff));
            } 
        }
        public string CarrierStock => CarrierStockValue > 0 ? $"{CarrierStockValue:N0} t" : string.Empty;
        public string CarrierStockDiff => CarrierStockValue > 0 ? $"{CarrierStockValue - Required:N0} t" : string.Empty;
    }
}
