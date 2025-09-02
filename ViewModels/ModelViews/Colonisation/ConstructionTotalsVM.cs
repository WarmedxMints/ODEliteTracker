using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ConstructionTotalsVM(string systemName, long total) : ODObservableObject
    {
        private string systemName = systemName;
        public string SystemName
        {
            get => systemName;
            set
            {
                systemName = value;
                OnPropertyChanged(nameof(SystemName));
            }
        }

        private long total = total;
        public string Total => $"{total:N0} t";
    }
}
