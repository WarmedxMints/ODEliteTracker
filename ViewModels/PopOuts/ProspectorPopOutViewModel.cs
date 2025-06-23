using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Mining;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.PopOuts
{
    internal class ProspectorPopOutViewModel : PopOutViewModel
    {
        private readonly MiningDataStore miningData;

        public ProspectorPopOutViewModel(MiningDataStore miningData)
        {
            this.miningData = miningData;

            this.miningData.StoreLive += OnStoreLive;

            ProspectorContainer = new(miningData);

            if (this.miningData.IsLive)
                OnStoreLive(this.miningData, true);
        }

        protected override void Dispose()
        {
            this.miningData.StoreLive -= OnStoreLive;
            ProspectorContainer.Dispose();
        }

        public override string Name => "Prospector";

        public override bool IsLive => miningData.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/Prospector.png", UriKind.Relative);

        public MiningProspectorContainer ProspectorContainer { get; }

        private void OnStoreLive(object? sender, bool e)
        {
            if(e)
            {
                ProspectorContainer.OnProspectorUpdated(miningData, EventArgs.Empty);
            }
            OnModelLive();
        }

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 300, 300);
        }
    }
}
