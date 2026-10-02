using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODEliteTracker.Services
{
    public enum Website
    {
        Inara,
        Spansh,
        Edsm
    }

    public static class WebsiteService
    {
        public static void OpenWebSite(Website website, string name = "", ulong address = 0, string url = "")
        {
            switch (website)
            {
                case Website.Inara:
                    OpenInara(name);
                    break;
                case Website.Spansh:
                    OpenSpansh(address);
                    break;
                case Website.Edsm:
                    OpenEdsm(url);
                    break;
            }
        }

        public static void OpenInara(string name)
        {
            ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/galaxy-starsystem/?search={name.Replace(' ', '+')}");
        }

        public static void OpenSpansh(ulong address)
        {
            ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://spansh.co.uk/system/{address}");
        }

        public static void OpenEdsm(string url)
        {
            ODMVVM.Helpers.OperatingSystem.OpenUrl(url);
        }
    }
}
