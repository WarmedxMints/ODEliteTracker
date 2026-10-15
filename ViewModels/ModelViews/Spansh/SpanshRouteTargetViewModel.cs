using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class SpanshRouteTargetViewModel
    {
        public SpanshRouteTargetViewModel(SpanshTargetSystem target)
        {
            Target = target;
            BodiesInfo = target.BodiesInfo?.Select(x => new BodiesInfoViewModel(x)).ToList() ?? [];
        }

        public SpanshTargetSystem Target { get; }
        public string SystemName => Target.SystemName;
        public string? Property1 => Target.Property1;
        public string? Property2 => Target.Property2;
        public string? Property3 => Target.Property3;
        public string? Property4 => Target.Property4;
        public bool FuelStar => string.Equals(Target.Property3, "Yes", StringComparison.OrdinalIgnoreCase) && Property4 != null;
        public List<BodiesInfoViewModel> BodiesInfo { get; private set; }
    }
}
