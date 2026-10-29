using ODEliteTracker.ViewModels;
using ODMVVM.Navigation.Controls;
using ODMVVM.ViewModels;

namespace ODEliteTracker.Services
{
    public sealed class NavigationService(Func<Type, ODViewModel> odViewModelFactory) : ODNavigationService(odViewModelFactory)
    {
        public override bool CanChangeViews { get; set; } = false;
        protected override bool NavigateToView(ODViewModel? newView)
        {
            var currentView = CurrentView;
            CurrentView = _viewModelFactory.Invoke(typeof(LoadingViewModel));
            if (newView?.GetType() == typeof(LoadingViewModel))
                return false;
            _nextCurrentView = newView;
           
            OnPropertyChanged(nameof(CurrentView));
            if (_nextCurrentView != null)
            {
                _nextCurrentView.ModelLive += OnCurrentViewLive;
                _nextCurrentView.NavigatedFrom(currentView);
            }

            return true;
        }

        protected override void OnCurrentViewLive(object? sender, bool v)
        {
            Task.Run(async () =>
            {
                while (!CanChangeViews)
                {
                    await Task.Delay(200);
                }

                base.OnCurrentViewLive(sender, v);
            });
        }
    }
}
