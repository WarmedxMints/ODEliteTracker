using ODEliteTracker.Models.Spansh;
using System.Net.Http;
using System.Net.Http.Json;

namespace ODEliteTracker.Services
{
    public sealed class SpanshClient(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;

        public async Task<SpanshSystemApiDump?> GetSystem(ulong address)
        {
            if (address == 0)
                return null;

            var ret = await httpClient.GetFromJsonAsync<SpanshSystemApiDump>($"dump/{address}");

            return ret;
        }
    }
}
