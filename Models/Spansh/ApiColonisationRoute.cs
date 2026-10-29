using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiColonisationRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public ApiColonisationParameters? Parameters { get; set; }
        [JsonProperty("result")]
        public ApiColonisationResult? Result { get; set; }
    }

    public sealed record ApiColonisationParameters : SpanshApiParameters
    {
        [JsonProperty("source_system")]
        public string SourceSystem { get; set; } = string.Empty;
        [JsonProperty("destination_system")]
        public string DestinationSystem { get; set; } = string.Empty;

        public override SpanshRouteType GetRouteType() =>  SpanshRouteType.Colonisation;
    }

    public sealed record ApiColonisationResult
    {
        [JsonProperty("jumps")]
        public IReadOnlyList<ApiColonisationSystemJump> Jumps { get; set; } = [];
    }

    public sealed record ApiColonisationSystemJump : ISpanshApiSystem
    {
        [JsonProperty("body_count", NullValueHandling = NullValueHandling.Ignore)]
        public int? BodyCount { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("distance_to_destination")]
        public double DistanceToDestination { get; set; }

        [JsonProperty("estimated_mapping_value")]
        public ulong EstimatedMappingValue { get; set; }

        [JsonProperty("estimated_scan_value")]
        public ulong EstimatedScanValue { get; set; }

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("landmark_value")]
        public ulong LandmarkValue { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
}
