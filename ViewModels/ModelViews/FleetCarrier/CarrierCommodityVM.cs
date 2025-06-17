using ODEliteTracker.Models.FleetCarrier;

namespace ODEliteTracker.ViewModels.ModelViews.FleetCarrier
{
    public sealed class CarrierCommodityVM(CarrierCommodity commodity)
    {
        private readonly CarrierCommodity commodity = commodity;

        public string Name => commodity.Stolen ? $"{commodity.EnglishName} [Stolen]" : commodity.EnglishName;
        public string Category => commodity.EnglishCategory;
        public long StockCount => commodity.StockCount;
        public string Stock => commodity.StockCount == 0 ? string.Empty : $"{StockCount:N0} t";
        public long DemandValue => commodity.BuyOrderCount;
        public string Demand => commodity.BuyOrderCount == 0 ? string.Empty : $"{commodity.BuyOrderCount:N0} t";
        public string SellOrderStockString => commodity.StockCount == 0 ? "Sold Out" : $"{StockCount:N0} t";
        public long SalePriceValue => commodity.SalePrice;
        public string SalePrice => $"{commodity.SalePrice:N0} cr";
    }
}
