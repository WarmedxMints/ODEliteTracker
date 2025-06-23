using ODEliteTracker.Models.Mining;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Mining
{
    public sealed class MiningItemVM(MiningItem miningItem, int totalRockCount) : ODObservableObject
    {
        public readonly MiningItem MiningItem = miningItem;

        public string Name => MiningItem.Name;
        public MiningItemType Type => MiningItem.Type;
        public string RefinedCount => MiningItem.RefinedCount == 0 ? string.Empty : MiningItem.RefinedCount.ToString();
        public string CollectedCount => MiningItem.CollectedCount == 0 ? string.Empty : MiningItem.CollectedCount.ToString();
        public string Prospected => MiningItem.Prospected == 0 ? string.Empty : MiningItem.Prospected.ToString();

        private double ratio = (double)miningItem.Count / totalRockCount * 100;
        public string Ratio => MiningItem.Type == MiningItemType.Ore ? $"{ratio:N2}%" : string.Empty;
        public string MinPercentage => MiningItem.MinPercentage == 0 ? string.Empty : $"{MiningItem.MinPercentage:N2}%";
        public string MaxPercentage => MiningItem.MaxPercentage == 0 ? string.Empty : $"{MiningItem.MaxPercentage:N2}%";
        public string MotherLoad => MiningItem.MotherLoad == 0 ? string.Empty : MiningItem.MotherLoad.ToString();
        public string AsteroidMaterialContents => MiningItem.Type == MiningItemType.Ore ? $"{MiningItem.LowContent:N0} │ {MiningItem.MedContent:N0} │ {MiningItem.HighContent:N0}" : string.Empty; 

        public void Update(int count)
        {
            if (Type == MiningItemType.Ore)
            {
                ratio = (double)MiningItem.Count / count * 100;
                OnPropertyChanged(nameof(Ratio));
            }
            OnPropertyChanged(nameof(RefinedCount));
            OnPropertyChanged(nameof(CollectedCount));
            OnPropertyChanged(nameof(Prospected));
            OnPropertyChanged(nameof(MinPercentage));
            OnPropertyChanged(nameof(MaxPercentage));
            OnPropertyChanged(nameof(MotherLoad));
            OnPropertyChanged(nameof(AsteroidMaterialContents));
        }
    }
}
