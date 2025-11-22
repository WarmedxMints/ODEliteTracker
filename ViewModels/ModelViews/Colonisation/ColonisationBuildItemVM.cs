using ODMVVM.Helpers;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ColonisationBuildItemVM(Commodity commodity, int required) : ODObservableObject
    {
        private readonly Commodity commodity = commodity;
        public Commodity Commodity => commodity;
        public string FDEVName => commodity.FdevName;
        public string Name => commodity.EnglishName;
        public string Category => commodity.EnglishCategory;
        public int RequiredAmount { get; private set; } = required;
        public string Required => $"{RequiredAmount:N0} t";
        public long CarrierStockValue { get; set; }
        public string CarrierStock => CarrierStockValue > 0 ? $"{CarrierStockValue:N0} t" : string.Empty;
        public string CarrierStockDifference => CarrierStockValue > 0 ? $"{CarrierStockValue - RequiredAmount:N0} t" : string.Empty;

        internal void SetCarrierStock(long value)
        {
            CarrierStockValue = value;
            OnPropertyChanged(nameof(CarrierStock));
            OnPropertyChanged(nameof(CarrierStockDifference));
        }

        internal void AddToRequired(int value)
        {
            RequiredAmount += value;
            OnPropertyChanged(nameof(RequiredAmount));
            OnPropertyChanged(nameof(CarrierStockDifference));
        }
    }
}