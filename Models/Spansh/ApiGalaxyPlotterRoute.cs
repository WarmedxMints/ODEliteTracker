using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ODEliteTracker.Helpers.JsonCoverters;
using System.ComponentModel;

namespace ODEliteTracker.Models.Spansh
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SpanshRoutingAlgorithm
    {
        [Description("Fuel")]
        fuel,
        [Description("Fuel Jumps")]
        fuel_jumps,
        [Description("Guided")]
        guided,
        [Description("Optimistic")]
        optimistic,
        [Description("Pessimistic")]
        pessimistic,
    }

    public sealed record ApiGalaxyPlotterRoute : SpanshResponseBase
    {

        [JsonProperty("parameters")]
        public required ApiGalaxyRouteParameters Parameters { get; set; }

        [JsonProperty("result")]
        public required ApiGalaxyPlotterRouteResult Result { get; set; }

        internal static ApiGalaxyPlotterRoute? FromJobID(SpanshApiParameters parameters)
        {
            if (parameters is ApiGalaxyRouteParameters @params)
            {
                return new()
                {
                    Job = @params.JobId ?? string.Empty,
                    State = string.Empty,
                    Status = string.Empty,
                    Parameters = @params,
                    Result = new()
                };
            }
            return null;
        }
    }

    public sealed record ApiGalaxyRouteParameters : SpanshApiParameters
    {
        [JsonProperty("algorithm")]
        public SpanshRoutingAlgorithm Algorithm { get; set; }

        [JsonProperty("base_mass")]
        public double BaseMass { get; set; }

        [JsonProperty("cargo")]
        public int Cargo { get; set; }

        [JsonProperty("destination_system")]
        public string DestinationSystem { get; set; } = string.Empty;

        [JsonProperty("exclude_secondary")]
        public bool ExcludeSecondary { get; set; }

        [JsonProperty("fuel_multiplier")]
        public double FuelMultiplier { get; set; }

        [JsonProperty("fuel_power")]
        public double FuelPower { get; set; }

        [JsonProperty("injection_multiplier")]
        public int InjectionMultiplier { get; set; }

        [JsonProperty("internal_tank_size")]
        public double InternalTankSize { get; set; }

        [JsonProperty("is_supercharged"), JsonConverter(typeof(BoolConvertor))]
        public bool IsSupercharged { get; set; }

        [JsonProperty("max_fuel_per_jump")]
        public double MaxFuelPerJump { get; set; }

        [JsonProperty("max_time")]
        public int MaxTime { get; set; }

        [JsonProperty("optimal_mass")]
        public double OptimalMass { get; set; }

        [JsonProperty("range_boost")]
        public double RangeBoost { get; set; }

        [JsonProperty("refuel_every_scoopable")]
        public bool RefuelEveryScoopable { get; set; }

        [JsonProperty("reserve_size")]
        public double ReserveSize { get; set; }

        [JsonProperty("ship_build")]
        public string ShipBuild { get; set; } = string.Empty;

        [JsonProperty("source_system")]
        public string SourceSystem { get; set; } = string.Empty;

        [JsonProperty("supercharge_multiplier")]
        public double SuperchargeMultiplier { get; set; }

        [JsonProperty("tank_size")]
        public int TankSize { get; set; }

        [JsonProperty("use_injections")]
        public bool UseInjections { get; set; }

        //[JsonProperty("use_injections_when_required"), JsonConverter(typeof(BoolConvertor<int>))]
        [JsonProperty("use_injections_when_required")]
        public bool UseInjectionsWhenRequired { get; set; }

        [JsonProperty("use_supercharge")]
        public bool UseSupercharge { get; set; }

        public override SpanshRouteType GetRouteType() => SpanshRouteType.GalaxyPlotter;
    }

    public sealed record ApiGalaxyPlotterRouteResult
    {
        [JsonProperty("jumps")]
        public IReadOnlyList<ApiGalaxyPlotterRouteJump> Jumps { get; set; } = [];

        [JsonProperty("refuel_every_scoopable")]
        public bool RefuelEveryScoopable { get; set; }
        [JsonProperty("reason")]
        public string? Reason { get; set; }
    }

    public sealed record ApiGalaxyPlotterRouteJump : ISpanshApiSystem
    {
        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("distance_to_destination")]
        public double DistanceToDestination { get; set; }

        [JsonProperty("fuel_in_tank")]
        public double FuelInTank { get; set; }

        [JsonProperty("fuel_used")]
        public double FuelUsed { get; set; }

        [JsonProperty("has_neutron")]
        public bool HasNeutron { get; set; }

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("is_scoopable")]
        public bool IsScoopable { get; set; }

        [JsonProperty("must_inject")]
        public int MustInject { get; set; }

        [JsonProperty("must_refuel")]
        public bool MustRefuel { get; set; }

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
