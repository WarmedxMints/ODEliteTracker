using ODEliteTracker.Stores;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Mining
{
    public sealed class MiningProspectorContainer : ODObservableObject, IDisposable
    {
        private readonly MiningDataStore miningData;

        public MiningProspectorContainer(MiningDataStore miningData)
        {
            this.miningData = miningData;
            this.miningData.LatestProspectorUpdated += OnProspectorUpdated;
        }

        public void Dispose()
        {
            miningData.LatestProspectorUpdated -= OnProspectorUpdated;
        }

        private MiningProspectorVM? latestProspector;
        public MiningProspectorVM? LatestProspector
        {
            get => latestProspector;
            set
            {
                latestProspector = value;
                OnPropertyChanged(nameof(LatestProspector));
            }
        }

        public void OnProspectorUpdated(object? sender, EventArgs e)
        {
            if (miningData.LatestProspector != null)
            {
                LatestProspector = new MiningProspectorVM(miningData.LatestProspector);
                return;
            }

            LatestProspector = null;
        }
    }
}
