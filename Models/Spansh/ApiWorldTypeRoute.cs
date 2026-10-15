using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiWorldTypeRoute([property: JsonProperty("parameters")] ApiWorldTypeParameters Parameters,
        [property: JsonProperty("result")] IReadOnlyList<ApiWorldTypeResult> Result) : SpanshResponseBase;

    public record ApiWorldTypeParameters : ApiRoadToRichesParameters
    {
        public override SpanshRouteType GetRouteType()
        {
            return SpanshRouteType.WorldTypeRoute;
        }
    }

    public record ApiWorldTypeResult([property: JsonProperty("bodies")] IReadOnlyList<ApiWorldTypeBody> Bodies,
        [property: JsonProperty("id64")] ulong Id64,
        [property: JsonProperty("jumps")] int Jumps,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("x")] double? X,
        [property: JsonProperty("y")] double? Y,
        [property: JsonProperty("z")] double? Z) : ISpanshApiSystem;

    public record ApiWorldTypeBody([property: JsonProperty("distance_to_arrival")] double? DistanceToArrival,
        [property: JsonProperty("estimated_mapping_value")] int EstimatedMappingValue,
        [property: JsonProperty("estimated_scan_value")] int EstimatedScanValue,
        [property: JsonProperty("id")] ulong Id,
        [property: JsonProperty("id64")] string Id64,
        [property: JsonProperty("is_terraformable")] bool? IsTerraformable,
        [property: JsonProperty("landmark_value")] object LandmarkValue,
        [property: JsonProperty("name")] string Name,
        [property: JsonProperty("subtype")] string Subtype,
        [property: JsonProperty("type")] string Type);
}
