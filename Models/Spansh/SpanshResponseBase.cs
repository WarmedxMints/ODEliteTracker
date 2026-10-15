
using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public record SpanshResponseBase()
    {
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTime UpdateAt { get; set; }
        [JsonProperty("job")]
        public required string Job { get; set;  }
        [JsonProperty("state")]
        public required string State { get; set; }
        [JsonProperty("status")]
        public required string Status { get; set; }

        public static SpanshResponseBase FromJobID(string id)
        {
            return new()
            {
                Job = id,
                State = string.Empty,
                Status = string.Empty,
            };
        }
    }
}
