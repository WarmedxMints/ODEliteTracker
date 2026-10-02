using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels;
using ODEliteTracker.Views.Dialogs;
using System.Windows;

namespace ODEliteTracker.Services
{
    public static class EliteTrackerDialogService
    {
        public static void ShowBookmarkManager(Window? owner,
                                               SharedDataStore sharedDataStore,
                                               BookmarkDataStore bookmarkData,
                                               EdsmApiService esdmApi,
                                               SettingsStore settings,
                                               ulong systemAddress,
                                               uint bodyID,
                                               double lat,
                                               double lon)
        {
            using var manager = new BookmarkManagerViewModel(sharedDataStore, bookmarkData, esdmApi, settings, systemAddress, bodyID, lat, lon);

            var view = new BookmarkManagerView()
            {
                Owner = owner,
                DataContext = manager,
            };

            view.ShowDialog();
        }
    }
}
