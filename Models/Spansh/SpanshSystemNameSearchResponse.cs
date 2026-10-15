using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed class SpanshSystemNameSearchResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("query")]
        public required string Query { get; set; }

        [JsonProperty("results")]
        public required List<SpanshSystemNameSearchResult> Results { get; set; }
    }

    public partial class SpanshSystemNameSearchResult
    {
        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
}
