using Newtonsoft.Json;
using ODEliteTracker.Helpers.JsonCoverters;

namespace ODEliteTracker.Models.Spansh
{
    public sealed record ApiFleetCarrierRoute : SpanshResponseBase
    {
        [JsonProperty("parameters")]
        public required ApiFleetCarrierRouteParameters Parameters { get; set; }

        [JsonProperty("result")]
        public required ApiFleetCarrierRouteResult Result { get; set; }
    }

    public sealed record ApiFleetCarrierRouteParameters : SpanshApiParameters
    {
        [JsonProperty("calculate_starting_fuel"), JsonConverter(typeof(BoolConvertor))]
        public bool CalculateStartingFuel { get; set; }

        [JsonProperty("capacity")]
        public int Capacity { get; set; } = 25000;

        [JsonProperty("capacity_used")]
        public long CapacityUsed { get; set; }

        [JsonProperty("current_fuel")]
        public int CurrentFuel { get; set; }

        [JsonProperty("destination_systems")]
        public IReadOnlyList<string> DestinationSystems { get; set; } = [];

        [JsonProperty("mass")]
        public int Mass { get; set; } 

        [JsonProperty("refuel_destinations")]
        public IReadOnlyList<string> RefuelDestinations { get; set; } = [];

        [JsonProperty("source_system")]
        public string SourceSystem { get; set; } = string.Empty;

        [JsonProperty("tritium_amount")]
        public long TritiumAmount { get; set; }
        public override SpanshRouteType GetRouteType() => SpanshRouteType.FleetCarrier;
    }

    public sealed record class ApiFleetCarrierRouteResult
    {
        [JsonProperty("calculate_starting_fuel")]
        public bool CalculateStartingFuel { get; set; }

        [JsonProperty("capacity")]
        public int Capacity { get; set; }

        [JsonProperty("capacity_used")]
        public int CapacityUsed { get; set; }

        [JsonProperty("destinations")]
        public IReadOnlyList<string>? Destinations { get; set; }

        [JsonProperty("fuel_loaded")]
        public int FuelLoaded { get; set; }

        [JsonProperty("jumps")]
        public required IReadOnlyList<ApiFleetCarrierRouteJump> Jumps { get; set; }

        [JsonProperty("mass")]
        public int Mass { get; set; }

        [JsonProperty("refuel_destinations")]
        public IReadOnlyList<string>? RefuelDestinations { get; set; }

        [JsonProperty("source")]
        public required string Source { get; set; }

        [JsonProperty("tritium_stored")]
        public int TritiumStored { get; set; }
    }

    public partial class ApiFleetCarrierRouteJump : ISpanshApiSystem
    {
        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("distance_to_destination")]
        public double DistanceToDestination { get; set; }

        [JsonProperty("fuel_in_tank")]
        public int FuelInTank { get; set; }

        [JsonProperty("fuel_used")]
        public int FuelUsed { get; set; }

        [JsonProperty("has_icy_ring")]
        public bool HasIcyRing { get; set; }

        [JsonProperty("id64")]
        public ulong Id64 { get; set; }

        [JsonProperty("is_desired_destination")]
        public int IsDesiredDestination { get; set; }

        [JsonProperty("is_system_pristine")]
        public bool IsSystemPristine { get; set; }

        [JsonProperty("must_restock")]
        public int MustRestock { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("restock_amount")]
        public int RestockAmount { get; set; }

        [JsonProperty("tritium_in_market")]
        public int TritiumInMarket { get; set; }

        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }
}
