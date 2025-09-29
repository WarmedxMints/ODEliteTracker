using Newtonsoft.Json.Linq;
using ODMVVM.Helpers;
using static System.Net.WebRequestMethods;

namespace ODEliteTracker.Models.Ship
{
    public record ShipInfo(string Name, string Ident, int CargoCapacity, JObject ShipJson)
    {
        internal void OpenShipyard(ShipyardWebSite shipyardPreference)
        {
            string uri = string.Empty;
            switch (shipyardPreference)
            {
                case ShipyardWebSite.EDSY:
                    uri = "https://edsy.org/" + "#/I=" + ShipJson.ToString().Utf8GZipBase64UriEscape();
                    break;
                case ShipyardWebSite.Coriolis:
                    uri = $"https://coriolis.io/import?data={ShipJson.ToString().GZipBase64EscapeUri()}&bn={Uri.EscapeDataString(Name)}";
                    break;
            }

            if (string.IsNullOrEmpty(uri))
                return;

            ODMVVM.Helpers.OperatingSystem.OpenUrl(uri);
        }
    }
}
