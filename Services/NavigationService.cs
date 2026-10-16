using ODEliteTracker.ViewModels;
using ODMVVM.Navigation;
using ODMVVM.Navigation.Controls;
using ODMVVM.ViewModels;

namespace ODEliteTracker.Services
{
    public sealed class NavigationService(Func<Type, ODViewModel> odViewModelFactory) : ODNavigationService(odViewModelFactory)
    {
        protected override bool NavigateToView(ODViewModel? newView)
        {
            CurrentView = _viewModelFactory.Invoke(typeof(LoadingViewModel));
            if (newView?.GetType() == typeof(LoadingViewModel))
                return false;
            _nextCurrentView = newView;
           
            OnPropertyChanged(nameof(CurrentView));
            if (_nextCurrentView != null)
            {
                _nextCurrentView.ModelLive += OnCurrentViewLive;
                _nextCurrentView.NavigatedFrom(_nextCurrentView);
            }

            return true;
        }
    }
}
