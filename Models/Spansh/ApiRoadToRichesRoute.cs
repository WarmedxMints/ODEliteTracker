using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiRoadToRichesRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public required ApiRoadToRichesParameters Parameters { get; set; }

        [JsonProperty("result")]
        public required IReadOnlyList<ApiRoadToRichesReult> Result { get; set; }
    }

    public record ApiRoadToRichesParameters : SpanshApiParameters
    {
        [JsonProperty("to")]
        public string? To { get; set; }

        [JsonProperty("destinations")]
        public IReadOnlyList<string> DestinationSystems { get; set; } = [];

        [JsonProperty("avoid_thargoids")]
        public int AvoidThargoids { get; set; }

        [JsonProperty("loop")]
        public int Loop { get; set; }

        [JsonProperty("max_results")]
        public long MaxResults { get; set; }

        [JsonProperty("radius")]
        public double Radius { get; set; }

        [JsonProperty("range")]
        public double Range { get; set; }

        [JsonProperty("from")]
        public required string From { get; set; }

        [JsonProperty("source")]
        public string? Source { get; set; }

        [JsonProperty("use_mapping_value")]
        public int UseMappingValue { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("body_types")]
        public List<string>? BodyTypes { get; set; }

        [JsonProperty("max_distance")]
        public long? MaxDistance { get; set; }

        [JsonProperty("min_value")]
        public long? MinValue { get; set; }

        public override SpanshRouteType GetRouteType()
        {
            return SpanshRouteType.RoadToRiches;
        }
    }

    public sealed record ApiRoadToRichesReult : ISpanshApiSystem
    {
        [JsonProperty("bodies")]
        public IReadOnlyList<RoadToRichesBody> Bodies { get; set; } = [];

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("jumps")]
        public int Jumps { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    public sealed record RoadToRichesBody
    {
        [JsonProperty("distance_to_arrival")]
        public double DistanceToArrival { get; set; }

        [JsonProperty("estimated_mapping_value")]
        public int EstimatedMappingValue { get; set; }

        [JsonProperty("estimated_scan_value")]
        public int EstimatedScanValue { get; set; }

        [JsonProperty("id")]
        public ulong Id { get; set; }

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("is_terraformable")]
        public bool IsTerraformable { get; set; }

        [JsonProperty("landmark_value")]
        public long? LandmarkValue { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("subtype")]
        public string? Subtype { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("landmarks")]
        public List<BodyLandmarks> Landmarks { get; set; } = [];
    }

    public sealed record BodyLandmarks
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("subtype")]
        public string? Subtype { get; set; }

        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
