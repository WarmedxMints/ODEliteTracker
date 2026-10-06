using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class MarketWatchlistPopOutSettings
    {
        public bool ShowMarket { get; set; } = false;
        public Visibility CategoryColumnVisibilty { get; set; } = Visibility.Visible;
    }
}
