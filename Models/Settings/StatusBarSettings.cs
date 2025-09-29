namespace ODEliteTracker.Models.Settings
{
    public sealed class StatusBarSettings
    {
        public SystemWebSite SystemSitePreference { get; set; } = SystemWebSite.Inara;
        public SystemWebSite StationBodyPreference { get; set; } = SystemWebSite.Inara;
        public ShipyardWebSite ShipyardPreference { get; set; } = ShipyardWebSite.EDSY;
    }
}
