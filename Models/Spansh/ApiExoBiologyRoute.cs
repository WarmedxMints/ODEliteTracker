using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    internal sealed record ApiExoviologyRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public required ApiExobiologyRouteParameters Parameters { get; set; }

        [JsonProperty("result")]
        public required IReadOnlyList<ApiRoadToRichesReult> Result { get; set; }
    }

    internal record ApiExobiologyRouteParameters : ApiRoadToRichesParameters
    {
        public override SpanshRouteType GetRouteType() => SpanshRouteType.Exobiology;
    }
}
