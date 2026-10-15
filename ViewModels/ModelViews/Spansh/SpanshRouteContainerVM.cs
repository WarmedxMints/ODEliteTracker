using ODEliteTracker.Models.Spansh;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class SpanshRouteContainerVM : ODObservableObject
    {
        public SpanshRouteContainer RouteContainer { get; }
        public DateTime CreatedDate => RouteContainer.CreatedDate;
        public SpanshRouteFormat Format => RouteContainer.Format;
        public SpanshRouteType RouteType => RouteContainer.RouteType;
        public DateTime LastUsed
        {
            get => RouteContainer.LastUsed;
            set
            {
                RouteContainer.LastUsed = value;
                OnPropertyChanged();
            }
        }
        public List<SpanshRouteTargetViewModel> Targets { get; private set; }
        public SpanshRouteTargetViewModel? CurrentTarget { get; set; }
        public string? From => Targets.FirstOrDefault()?.SystemName;
        public string? To => Targets.LastOrDefault()?.SystemName;
        public string SystemCount => $"{Targets.Count:N0}";
        public string ShipName => RouteContainer.ShipName;
        public string ShipNameGrid => RouteContainer.ShipNameGrid;
        public int CurrentIndex => RouteContainer.CurrentIndex;
        public SpanshRouteContainerVM(SpanshRouteContainer routeContainer)
        {
            RouteContainer = routeContainer;
            Targets = [.. routeContainer.Targets.Select(x => new SpanshRouteTargetViewModel(x))];
            CurrentTarget = Targets.FirstOrDefault();
        }
    }
}
