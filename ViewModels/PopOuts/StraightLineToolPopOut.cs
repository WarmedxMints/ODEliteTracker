using ODEliteTracker.Stores;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class StraightLineToolPopOut : PopOutViewModel
    {
        public StraightLineToolPopOut(ColonisationStore colonisationStore, ColonisationRoutePlannerViewModel routePlanner)
        {
            planner = routePlanner;
            this.colonisationStore = colonisationStore;
            this.colonisationStore.StoreLive += OnStoreLive;

            if (this.colonisationStore.IsLive)
            {
                _ = OnStoreLive(null, true);
            }
        }

        private readonly ColonisationRoutePlannerViewModel planner;
        private readonly ColonisationStore colonisationStore;

        public ColonisationRoutePlannerViewModel Planner => planner;
        public override string Name => "Straight Line Tool Overlay";

        public override bool IsLive => colonisationStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/ColonisationBtn.png", UriKind.Relative);

        protected override void Dispose()
        {
            planner.Dispose();
            this.colonisationStore.StoreLive -= OnStoreLive;
        }
        private async Task OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                OnModelLive();
            }
        }
    }
}
