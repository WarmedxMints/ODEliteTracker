using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Json;

namespace ODEliteTracker.Services
{
    public sealed class TickResponse
    {
        [JsonProperty("lastGalaxyTick")]
        public DateTime LastGalaxyTick { get; set; } = DateTime.MinValue;
    }

    public sealed class TickInformacerApiService(HttpClient httpClient)
    {
        private readonly HttpClient httpClient = httpClient;
        public event EventHandler<Exception>? OnError;
        private readonly DateTime EDReleaseDate = new(2014, 12, 16);
        public async Task<DateTime> GetLastestTick()
        {
            try
            {
                var content = await httpClient.GetFromJsonAsync<TickResponse>("galtick.json");

                return content?.LastGalaxyTick ?? EDReleaseDate;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(this, ex);
                return EDReleaseDate;
            }
        }
    }
}
