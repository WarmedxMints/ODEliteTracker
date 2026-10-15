using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODMVVM.Commands;
using ODMVVM.ViewModels;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class SpanshViewModel : ODViewModel
    {
        public ICommand OpenPopOut { get; }
        public SpanshViewModel(SpanshRouteStore spanshStore,
                               SpanshRouteController routeController,
                               SpanshAPIController apiController,
                               PopOutService popOutService,
                               SettingsStore settings) 
        {
            this.spanshStore = spanshStore;
            this.routeController = routeController;
            ApiController = apiController;
            this.popOutService = popOutService;
            this.settings = settings;
            this.spanshStore.StoreLive += OnCSVStoreLive;

            OpenPopOut = new ODRelayCommand<Type>(OnOpenPopOut);

            if (this.spanshStore.IsLive)
                OnCSVStoreLive(this.spanshStore, true);
        }

        private void OnOpenPopOut(Type type)
        {
            popOutService.OpenPopOut(type, settings.SelectedCommanderID);
        }

        private Task OnCSVStoreLive(object? sender, bool e)
        {
            OnModelLive(e);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            this.spanshStore.StoreLive -= OnCSVStoreLive;
            ApiController.SaveAPIData();
        }

        private readonly SpanshRouteStore spanshStore;
        private readonly SpanshRouteController routeController;
        private readonly PopOutService popOutService;
        private readonly SettingsStore settings;

        public override bool IsLive => spanshStore.IsLive;

        public SpanshRouteController RouteController => routeController;

        public SpanshAPIController ApiController { get; }
    }
}
