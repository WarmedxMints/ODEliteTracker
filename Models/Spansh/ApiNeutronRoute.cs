using Newtonsoft.Json;
using System.ComponentModel;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiNeutronRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public ApiNeutronParameters? Parameters { get; set; }
        [JsonProperty("result")]
        public ApiNeutronResult? Result { get; set; }
    }

    public sealed record ApiNeutronParameters : SpanshApiParameters
    {
        [JsonProperty("from"), DefaultValue("")]
        public string From { get; set; } = string.Empty;

        [JsonProperty("to"), DefaultValue("")]
        public string To { get; set; } = string.Empty;

        [JsonProperty("range")]
        public double Range { get; set; }

        [JsonProperty("supercharge_multiplier")]
        public long SuperchargeMultiplier { get; set; }

        [JsonProperty("efficiency")]
        public long Efficiency { get; set; }

        [JsonProperty("via")]
        public List<string>? Via { get; set; }

        public override SpanshRouteType GetRouteType()
        {
            return SpanshRouteType.NeutronRoute;
        }
        
    }

    public sealed record ApiNeutronResult
    {
        [JsonProperty("destination_system")]
        public string DestinationSystem { get; set; } = string.Empty;

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("efficiency")]
        public int Efficiency { get; set; }

        [JsonProperty("job")]
        public string Job { get; set; } = string.Empty;

        [JsonProperty("range")]
        public string Range { get; set; } = string.Empty;

        [JsonProperty("source_system")]
        public string SourceSystem { get; set; } = string.Empty;

        [JsonProperty("system_jumps")]
        public IReadOnlyList<ApiNeutronSystemJump> SystemJumps { get; set; } = [];

        [JsonProperty("total_jumps")]
        public int TotalJumps { get; set; }

        [JsonProperty("via")]
        public IReadOnlyList<object>? Via { get; set; }
    }

    public sealed record ApiNeutronSystemJump : ISpanshApiSystem
    {
        [JsonProperty("distance_jumped")]
        public double DistanceJumped { get; set; }

        [JsonProperty("distance_left")]
        public double DistanceLeft { get; set; }

        [JsonProperty("id64")]
        public long Id64 { get; set; }

        [JsonProperty("jumps")]
        public long Jumps { get; set; }

        [JsonProperty("neutron_star")]
        public bool NeutronStar { get; set; }

        [JsonProperty("system")]
        public string System { get; set; } = string.Empty;

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

}
