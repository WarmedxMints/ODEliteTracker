using ODMVVM.Helpers;

namespace ODEliteTracker.Models.FleetCarrier
{
    public sealed class CarrierCommodity(Commodity commodity, bool stolen, long salePrice)
    {
        public readonly Commodity commodity = commodity;
        public string FdevName => commodity.FdevName;
        public string EnglishName => commodity.EnglishName;
        public string FdevCategory => commodity.FdevCategory;
        public string EnglishCategory => commodity.EnglishCategory;
        public bool Stolen { get; } = stolen;
        public bool BlackMarket { get; set; } = false;
        public bool Rare { get; set; } = commodity.Rare;
        public long SalePrice { get; set; } = salePrice;
                
        private long stockCount;
        public long StockCount 
        {
            get => stockCount;
            set
            {
                stockCount = value;
                if(stockCount < 0)
                    stockCount = 0;
            }
        }

        private long buyOrderCount;
        public long BuyOrderCount
        {
            get => buyOrderCount;
            set
            {
                buyOrderCount = value;
                if (buyOrderCount < 0)
                    buyOrderCount = 0;
            }
        }
    }
}
