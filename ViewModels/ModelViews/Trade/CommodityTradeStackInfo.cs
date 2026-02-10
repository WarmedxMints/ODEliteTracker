using ODMVVM.ViewModels;
using System.Security.Cryptography.X509Certificates;

namespace ODEliteTracker.ViewModels.ModelViews.Trade
{
    public sealed class CommodityTradeStackInfo : ODObservableObject
    {
        public CommodityTradeStackInfo(IEnumerable<TradeMissionVM> missions)
        {
            FdevCommodity = missions.First().FdevCommodity;
            Commodity = missions.First().Commodity_Localised;
            MissionCountInt = missions.Count();

            int commodityCount = 0;            
            int deliveredCount = 0;
            int value = 0;

            foreach (var mission in missions)
            {
                commodityCount += mission.Count;
                deliveredCount += mission.ItemsDelivered;
                value += mission.Reward;
            }

            CommodityCountInt = commodityCount;
            DeliveredCountInt = deliveredCount;
            RemainingCountInt = commodityCount - deliveredCount;
            CreditPerTInt = value / commodityCount;
            ValueInt = value;

        }
        public string FdevCommodity { get; }
        public string Commodity { get; }
        public string CommodityCount => $"{CommodityCountInt:N0}";
        public int CommodityCountInt { get; }
        public string MissionCount => $"{MissionCountInt:N0}";
        public int MissionCountInt { get; }
        public string DeliveredCount => $"{DeliveredCountInt:N0}";
        public int DeliveredCountInt { get; }
        public string RemainingCount => $"{CommodityCountInt - DeliveredCountInt:N0}";
        public int RemainingCountInt { get; }
        public string Value => $"{ValueInt:N0}";
        public int ValueInt { get; }
        public string CreditPerT => $"{CreditPerTInt:N0} cr";
        public int CreditPerTInt { get; }
        public string CarrierStock => CarrierStockInt == 0 ? string.Empty : $"{CarrierStockInt:N0}";

        private int carrierStockInt;          
        public int CarrierStockInt
        {
            get => carrierStockInt;
            set
            {
                carrierStockInt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CarrierStock));
            }
        }
        public string CarrierDiff => CarrierStockInt == 0 ? string.Empty : $"{CarrierDiffInt:N0}";

        private int carrierDiffInt;
        public int CarrierDiffInt
        {
            get => carrierDiffInt;
            set
            {
                carrierDiffInt = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CarrierDiff));
            }
        }
    }
}
