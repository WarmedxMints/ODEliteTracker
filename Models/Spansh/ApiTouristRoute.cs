using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiTouristRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public required ApiTouristRouteApiParameters Parameters { get; set; }

        [JsonProperty("result")]
        public required ApiTouristRouteResult Result { get; set; }
    }

    public sealed record ApiTouristRouteApiParameters : SpanshApiParameters
    {
        [JsonProperty("source")]
        public required string Source { get; set; }
        [JsonProperty("range")]
        public double Range { get; set; }

        [JsonProperty("destination")]
        public List<string>? Destinations { get; set; }

        [JsonProperty("final_destination")]
        public string? FinalDestination { get; set; }

        [JsonProperty("loop")]
        public int Loop { get; set; }

        public override SpanshRouteType GetRouteType() => SpanshRouteType.TouristRoute;
    }

    public sealed record ApiTouristRouteResult
    {
        [JsonProperty("destination_systems")]
        public List<string> DestinationSystems { get; } = [];

        [JsonProperty("job")]
        public string? Job { get; set; }

        [JsonProperty("range")]
        public double Range { get; set; }

        [JsonProperty("source_system")]
        public required string SourceSystem { get; set; }

        [JsonProperty("system_jumps")]
        public List<ApiTouristRouteSystemJump> SystemJumps { get; } = [];
    }

    public sealed record ApiTouristRouteSystemJump : ISpanshApiSystem
    {
        [JsonProperty("distance")]
        public double? Distance { get; set; }

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("jumps")]
        public int? Jumps { get; set; }

        [JsonProperty("system")]
        public required string System { get; set; }

        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }

        [JsonProperty("z")]
        public double? Z { get; set; }
    }
}
