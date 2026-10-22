using ODEliteTracker.Models;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;

namespace ODEliteTracker.Helpers
{
    internal class StarSystemHelpers
    {
        internal static void OpenSystemPage(SystemWebSite site, StarSystemVM system)
        {
            switch (site)
            {
                case SystemWebSite.Inara:
                    ODMVVM.Helpers.OperatingSystem.OpenInaraSystem(system.Name);
                    break;
                case SystemWebSite.Spansh:
                    ODMVVM.Helpers.OperatingSystem.OpenSpanshSystem(system.Address);
                    break;
                case SystemWebSite.RavenColonial:
                    ODMVVM.Helpers.OperatingSystem.OpenRavenColonia(system.Name);
                    break;
            }
        }

        internal static void OpenSystemPage(SystemWebSite site, StarSystem system)
        {
            switch (site)
            {
                case SystemWebSite.Inara:
                    ODMVVM.Helpers.OperatingSystem.OpenInaraSystem(system.Name);
                    break;
                case SystemWebSite.Spansh:
                    ODMVVM.Helpers.OperatingSystem.OpenSpanshSystem(system.Address);
                    break;
                case SystemWebSite.RavenColonial:
                    ODMVVM.Helpers.OperatingSystem.OpenRavenColonia(system.Name);
                    break;
            }
        }

        internal static void OpenSystemPage(SystemWebSite site, ColonisationRouteSystemVM system)
        {
            switch (site)
            {
                case SystemWebSite.Inara:
                    ODMVVM.Helpers.OperatingSystem.OpenInaraSystem(system.SystemName);
                    break;
                case SystemWebSite.Spansh:                   
                    ODMVVM.Helpers.OperatingSystem.OpenSpanshSystem(system.SystemAddress);
                    break;
                case SystemWebSite.RavenColonial:
                    ODMVVM.Helpers.OperatingSystem.OpenRavenColonia(system.SystemName);
                    break;
            }
        }
    }
}
