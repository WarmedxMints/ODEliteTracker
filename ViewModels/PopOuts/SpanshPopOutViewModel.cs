using ODEliteTracker.Models;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class SpanshPopOutViewModel : PopOutViewModel
    {
        public SpanshPopOutViewModel(SpanshRouteStore spanshStore, SpanshRouteController routeController) 
        {
            this.spanshStore = spanshStore;
            this.routeController = routeController;

            this.spanshStore.StoreLive += OnSpanshStoreLive;

            if (this.spanshStore.IsLive)
                OnSpanshStoreLive(this.spanshStore, true);
        }

        protected override void Dispose()
        {
            //routeController.Dispose();
            this.spanshStore.StoreLive -= OnSpanshStoreLive;
        }

        private readonly SpanshRouteStore spanshStore;
        private readonly SpanshRouteController routeController;
        public SpanshRouteController RouteController => routeController;
        public override string Name => "Spansh Overlay";

        public override bool IsLive => spanshStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/spanshbtn.png", UriKind.Relative);

        public SpanshRouteTypeComboBox CurrentTypeBox
        {
            get
            {
                if (Enum.TryParse<SpanshRouteTypeComboBox>(RouteController.CurrentType.ToString(), out var result))
                {
                    return result;
                }
                return SpanshRouteTypeComboBox.RoadToRiches;
            }
            set
            {
                if (Enum.TryParse<SpanshRouteType>(value.ToString(), out var result))
                {
                    RouteController.SetCurrentRouteTypeCommand.Execute(result);
                }

                OnPropertyChanged(nameof(CurrentTypeBox));
            }
        }

        private Task OnSpanshStoreLive(object? sender, bool e)
        {
            OnModelLive();
            return Task.CompletedTask;
        }

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 900, 500);
        }
    }
}
