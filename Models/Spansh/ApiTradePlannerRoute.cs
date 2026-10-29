using Newtonsoft.Json;
using ODEliteTracker.Helpers.JsonCoverters;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiTradePlannerRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public ApiTradePlannerParameters? Parameters { get; set; }

        [JsonProperty("result")]
        public List<ApiTradePlannerResult> Result { get; set; } = [];
    }

    public sealed record ApiTradePlannerParameters : SpanshApiParameters
    {
        [JsonProperty("allow_planetary"), JsonConverter(typeof(BoolConvertor))]
        public bool AllowPlanetary { get; set; }

        [JsonProperty("allow_player_owned"), JsonConverter(typeof(BoolConvertor))]
        public bool AllowPlayerOwned { get; set; }

        [JsonProperty("allow_prohibited"), JsonConverter(typeof(BoolConvertor))]
        public bool AllowProhibited { get; set; }

        [JsonProperty("allow_restricted_access"), JsonConverter(typeof(BoolConvertor))]
        public bool AllowRestrictedAccess { get; set; }

        [JsonProperty("max_cargo")]
        public int MaxCargo { get; set; }

        [JsonProperty("max_hop_distance")]
        public double MaxHopDistance { get; set; }

        [JsonProperty("max_hops")]
        public long MaxHops { get; set; }

        [JsonProperty("max_price_age")]
        public int MaxPriceAge { get; set; }

        [JsonProperty("max_system_distance")]
        public long MaxSystemDistance { get; set; }

        [JsonProperty("permit"), JsonConverter(typeof(BoolConvertor))]
        public bool Permit { get; set; }

        [JsonProperty("requires_large_pad"), JsonConverter(typeof(BoolConvertor))]
        public bool RequiresLargePad { get; set; }

        [JsonProperty("starting_capital")]
        public long StartingCapital { get; set; }

        [JsonProperty("starting_market_id")]
        public long? StartingMarketId { get; set; }

        [JsonProperty("unique"), JsonConverter(typeof(BoolConvertor))]
        public bool Unique { get; set; }
        [JsonProperty("system")]
        public string? System { get; set; }
        [JsonProperty("station")]
        public string? Station { get; set; }

        public override SpanshRouteType GetRouteType() => SpanshRouteType.TradePlanner;
    }

    public sealed record ApiTradePlannerResult : ISpanshApiSystem
    {
        [JsonProperty("commodities")]
        public IReadOnlyList<ApiTradePlannerCommodities> Commodities { get; set; } = [];

        [JsonProperty("cumulative_profit")]
        public long CumulativeProfit { get; set; }

        [JsonProperty("destination")]
        public ApiTradePlannerStation Destination { get; set; } = new();

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("source")]
        public ApiTradePlannerStation Source { get; set; } = new();

        [JsonProperty("total_profit")]
        public long TotalProfit { get; set; }

        public ApiTradePlannerResult Copy()
        {
            var clone = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ApiTradePlannerResult>(clone) ?? new();
        }
    }

    public sealed record ApiTradePlannerCommodities
    {
        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("destination_commodity")]
        public ApiTradePlannerCommodity DestinationCommodity { get; set; } = new();

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("profit")]
        public long Profit { get; set; }

        [JsonProperty("source_commodity")]
        public ApiTradePlannerCommodity SourceCommodity { get; set; } = new();

        [JsonProperty("total_profit")]
        public long TotalProfit { get; set; }
    }


    public sealed record ApiTradePlannerCommodity
    {
        [JsonProperty("buy_price")]
        public long BuyPrice { get; set; }

        [JsonProperty("demand")]
        public long Demand { get; set; }

        [JsonProperty("sell_price")]
        public long SellPrice { get; set; }

        [JsonProperty("supply")]
        public long Supply { get; set; }
    }

    public sealed record ApiTradePlannerStation
    {
        [JsonProperty("distance_to_arrival")]
        public ulong DistanceToArrival { get; set; }

        [JsonProperty("market_id")]
        public ulong MarketId { get; set; }

        [JsonProperty("market_updated_at")]
        public long MarketUpdatedAt { get; set; }

        [JsonProperty("station")]
        public string Station { get; set; } = string.Empty;

        [JsonProperty("system")]
        public string System { get; set; } = string.Empty;

        [JsonProperty("system_id64")]
        public long SystemId64 { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }

        public ApiTradePlannerStation Copy()
        {
            var clone = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<ApiTradePlannerStation>(clone) ?? new();
        }
    }
}
