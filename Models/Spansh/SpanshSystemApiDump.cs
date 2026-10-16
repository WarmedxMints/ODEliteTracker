using EliteJournalReader;
using Newtonsoft.Json;
namespace ODEliteTracker.Models.Spansh
{
    public sealed class Position
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public double Z { get; set; }
    }

    public partial class ControllingFaction
    {
        [JsonProperty("allegiance")]
        public string? Allegiance { get; set; }

        [JsonProperty("government")]
        public string? Government { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }
    }

    public sealed class SpanshSystemApiDump
    {
        public SpanshSystem? System { get; set; }
    }
    public sealed class SpanshSystem
    {
        [JsonProperty("allegiance")]
        public string? Allegiance { get; set; }

        [JsonProperty("bodies")]
        public IReadOnlyList<SpanshBody> Bodies { get; set; } = [];

        [JsonProperty("bodyCount")]
        public int BodyCount { get; set; }

        [JsonProperty("controllingFaction")]
        public ControllingFaction? ControllingFaction { get; set; }

        [JsonProperty("coords")]
        public Position Coords { get; set; } = new();

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("factions")]
        public IReadOnlyList<Faction>? Factions { get; set; }

        [JsonProperty("government")]
        public string? Government { get; set; }

        [JsonProperty("id64")]
        public long Id64 { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("population")]
        public long Population { get; set; }

        [JsonProperty("primaryEconomy")]
        public string? PrimaryEconomy { get; set; }

        [JsonProperty("secondaryEconomy")]
        public string? SecondaryEconomy { get; set; }

        [JsonProperty("security")]
        public string? Security { get; set; }

        //[JsonProperty("stations")]
        //public List<SystemStation> Stations { get; set; }

        //[JsonProperty("timestamps")]
        //public SystemTimestamps Timestamps { get; set; }
    }

    public partial class Ring
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("mass")]
        public double Mass { get; set; }

        [JsonProperty("innerRadius")]
        public double InnerRadius { get; set; }

        [JsonProperty("outerRadius")]
        public double OuterRadius { get; set; }

        [JsonProperty("id64")]
        public double Id64 { get; set; }

        [JsonProperty("signals")]
        public RingSignals? Signals { get; set; }
    }

    public partial class RingSignals
    {
        [JsonProperty("signals")]
        public IReadOnlyDictionary<string,double>? Signals { get; set; }

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; } = string.Empty;
    }

    public sealed class SpanshBody
    {
        [JsonProperty("argOfPeriapsis")]
        public double ArgOfPeriapsis { get; set; }

        [JsonProperty("ascendingNode")]
        public double AscendingNode { get; set; }

        [JsonProperty("bodyId")]
        public long BodyId { get; set; }

        [JsonProperty("id64")]
        public long Id64 { get; set; }

        [JsonProperty("meanAnomaly")]
        public double MeanAnomaly { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("orbitalEccentricity")]
        public double OrbitalEccentricity { get; set; }

        [JsonProperty("orbitalInclination")]
        public double OrbitalInclination { get; set; }

        [JsonProperty("orbitalPeriod")]
        public double OrbitalPeriod { get; set; }

        [JsonProperty("semiMajorAxis")]
        public double SemiMajorAxis { get; set; }

        [JsonProperty("stations")]
        public List<BodyStation>? Stations { get; set; }

        [JsonProperty("timestamps")]
        public BodyTimestamps? Timestamps { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }

        [JsonProperty("absoluteMagnitude", NullValueHandling = NullValueHandling.Ignore)]
        public double? AbsoluteMagnitude { get; set; }

        [JsonProperty("age", NullValueHandling = NullValueHandling.Ignore)]
        public long? Age { get; set; }

        [JsonProperty("axialTilt", NullValueHandling = NullValueHandling.Ignore)]
        public double? AxialTilt { get; set; }

        [JsonProperty("distanceToArrival", NullValueHandling = NullValueHandling.Ignore)]
        public double? DistanceToArrival { get; set; }

        [JsonProperty("luminosity", NullValueHandling = NullValueHandling.Ignore)]
        public string? Luminosity { get; set; }

        [JsonProperty("mainStar", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MainStar { get; set; }

        [JsonProperty("parents", NullValueHandling = NullValueHandling.Ignore)]
        public List<Parent>? Parents { get; set; }

        [JsonProperty("rotationalPeriod", NullValueHandling = NullValueHandling.Ignore)]
        public double? RotationalPeriod { get; set; }

        [JsonProperty("solarMasses", NullValueHandling = NullValueHandling.Ignore)]
        public double? SolarMasses { get; set; }

        [JsonProperty("solarRadius", NullValueHandling = NullValueHandling.Ignore)]
        public double? SolarRadius { get; set; }

        [JsonProperty("spectralClass", NullValueHandling = NullValueHandling.Ignore)]
        public string? SpectralClass { get; set; }

        [JsonProperty("subType", NullValueHandling = NullValueHandling.Ignore)]
        public string? SubType { get; set; }

        [JsonProperty("surfaceTemperature", NullValueHandling = NullValueHandling.Ignore)]
        public double? SurfaceTemperature { get; set; }

        [JsonProperty("belts", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<Ring>? Belts { get; set; }

        [JsonProperty("atmosphereComposition", NullValueHandling = NullValueHandling.Ignore)]
        public AtmosphereComposition? AtmosphereComposition { get; set; }

        [JsonProperty("atmosphereType")]
        public string? AtmosphereType { get; set; }

        [JsonProperty("earthMasses", NullValueHandling = NullValueHandling.Ignore)]
        public double? EarthMasses { get; set; }

        [JsonProperty("gravity", NullValueHandling = NullValueHandling.Ignore)]
        public double? Gravity { get; set; }

        [JsonProperty("isLandable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsLandable { get; set; }

        [JsonProperty("radius", NullValueHandling = NullValueHandling.Ignore)]
        public double? Radius { get; set; }

        [JsonProperty("rotationalPeriodTidallyLocked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? RotationalPeriodTidallyLocked { get; set; }

        [JsonProperty("solidComposition", NullValueHandling = NullValueHandling.Ignore)]
        public SolidComposition? SolidComposition { get; set; }

        [JsonProperty("surfacePressure", NullValueHandling = NullValueHandling.Ignore)]
        public double? SurfacePressure { get; set; }

        [JsonProperty("terraformingState", NullValueHandling = NullValueHandling.Ignore)]
        public string? TerraformingState { get; set; }

        [JsonProperty("volcanismType", NullValueHandling = NullValueHandling.Ignore)]
        public string? VolcanismType { get; set; }

        [JsonProperty("materials", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, double>? Materials { get; set; }

        [JsonProperty("signals", NullValueHandling = NullValueHandling.Ignore)]
        public BodySignals? Signals { get; set; }

        [JsonProperty("reserveLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string? ReserveLevel { get; set; }

        [JsonProperty("rings", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<Ring>? Rings { get; set; }
    }

    public partial class AtmosphereComposition
    {
        [JsonProperty("Carbon dioxide", NullValueHandling = NullValueHandling.Ignore)]
        public double? CarbonDioxide { get; set; }

        [JsonProperty("Sulphur dioxide", NullValueHandling = NullValueHandling.Ignore)]
        public double? SulphurDioxide { get; set; }

        [JsonProperty("Nitrogen", NullValueHandling = NullValueHandling.Ignore)]
        public double? Nitrogen { get; set; }

        [JsonProperty("Water", NullValueHandling = NullValueHandling.Ignore)]
        public double? Water { get; set; }

        [JsonProperty("Oxygen", NullValueHandling = NullValueHandling.Ignore)]
        public double? Oxygen { get; set; }

        [JsonProperty("Ammonia", NullValueHandling = NullValueHandling.Ignore)]
        public double? Ammonia { get; set; }

        [JsonProperty("Argon", NullValueHandling = NullValueHandling.Ignore)]
        public double? Argon { get; set; }

        [JsonProperty("Helium", NullValueHandling = NullValueHandling.Ignore)]
        public double? Helium { get; set; }
    }

    public partial class Parent
    {
        [JsonProperty("Null", NullValueHandling = NullValueHandling.Ignore)]
        public long? Null { get; set; }

        [JsonProperty("Planet", NullValueHandling = NullValueHandling.Ignore)]
        public long? Planet { get; set; }

        [JsonProperty("Star", NullValueHandling = NullValueHandling.Ignore)]
        public long? Star { get; set; }
    }

    public partial class BodySignals
    {
        [JsonProperty("genuses", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<string>? Genuses { get; set; }

        [JsonProperty("signals")]
        public SignalsSignals? Signals { get; set; }

        [JsonProperty("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }
    }

    public partial class SignalsSignals
    {
        [JsonProperty("$PlanetaryMiningLocation_Name;")]
        public long PlanetaryMiningLocationName { get; set; }

        [JsonProperty("$SAA_SignalType_Biological;", NullValueHandling = NullValueHandling.Ignore)]
        public long? SaaSignalTypeBiological { get; set; }

        [JsonProperty("$SAA_SignalType_Human;", NullValueHandling = NullValueHandling.Ignore)]
        public long? SaaSignalTypeHuman { get; set; }
    }

    public partial class SolidComposition
    {
        [JsonProperty("Ice")]
        public double Ice { get; set; }

        [JsonProperty("Metal")]
        public double Metal { get; set; }

        [JsonProperty("Rock")]
        public double Rock { get; set; }
    }

    public partial class BodyTimestamps
    {
        [JsonProperty("meanAnomaly")]
        public DateTimeOffset MeanAnomaly { get; set; }

        [JsonProperty("distanceToArrival", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? DistanceToArrival { get; set; }
    }

    public partial class BodyStation
    {
        [JsonProperty("controllingFaction")]
        public string? ControllingFaction { get; set; }

        [JsonProperty("controllingFactionState")]
        public string? ControllingFactionState { get; set; }

        [JsonProperty("distanceToArrival", NullValueHandling = NullValueHandling.Ignore)]
        public double? DistanceToArrival { get; set; }

        [JsonProperty("economies")]
        public Dictionary<string, double>? Economies { get; set; }

        [JsonProperty("government")]
        public string? Government { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("landingPads", NullValueHandling = NullValueHandling.Ignore)]
        public LandingPads? LandingPads { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("market")]
        public Market? Market { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("primaryEconomy")]
        public string? PrimaryEconomy { get; set; }

        [JsonProperty("services", NullValueHandling = NullValueHandling.Ignore)]
        public List<string>? Services { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }

        [JsonProperty("outfitting", NullValueHandling = NullValueHandling.Ignore)]
        public Outfitting? Outfitting { get; set; }
    }

    public partial class PurpleEconomies
    {
        [JsonProperty("Colony", NullValueHandling = NullValueHandling.Ignore)]
        public long? Colony { get; set; }

        [JsonProperty("Agriculture", NullValueHandling = NullValueHandling.Ignore)]
        public long? Agriculture { get; set; }

        [JsonProperty("Industrial", NullValueHandling = NullValueHandling.Ignore)]
        public long? Industrial { get; set; }

        [JsonProperty("Refinery", NullValueHandling = NullValueHandling.Ignore)]
        public long? Refinery { get; set; }
    }

    public partial class LandingPads
    {
        [JsonProperty("large")]
        public long Large { get; set; }

        [JsonProperty("medium")]
        public long Medium { get; set; }

        [JsonProperty("small")]
        public long Small { get; set; }
    }

    public partial class Market
    {
        [JsonProperty("commodities")]
        public List<Commodity>? Commodities { get; set; }

        [JsonProperty("prohibitedCommodities")]
        public IReadOnlyList<string>? ProhibitedCommodities { get; set; }

        [JsonProperty("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }
    }

    public partial class Commodity
    {
        [JsonProperty("buyPrice")]
        public long BuyPrice { get; set; }

        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("commodityId")]
        public long CommodityId { get; set; }

        [JsonProperty("demand")]
        public long Demand { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("sellPrice")]
        public long SellPrice { get; set; }

        [JsonProperty("supply")]
        public long Supply { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }
    }

    public partial class Outfitting
    {
        [JsonProperty("modules")]
        public IReadOnlyList<Module> Modules { get; set; } = [];

        [JsonProperty("updateTime")]
        public DateTimeOffset UpdateTime { get; set; }
    }

    public partial class Module
    {
        [JsonProperty("category")]
        public string? Category { get; set; }

        [JsonProperty("class")]
        public long Class { get; set; }

        [JsonProperty("moduleId")]
        public long ModuleId { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("rating")]
        public string? Rating { get; set; }

        [JsonProperty("symbol")]
        public string? Symbol { get; set; }

        [JsonProperty("ship", NullValueHandling = NullValueHandling.Ignore)]
        public string? Ship { get; set; }
    }
}
