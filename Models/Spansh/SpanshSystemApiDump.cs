using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed class Position
    {
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
    }

    public sealed class SpanshSystemApiDump
    {
        public SpanshSystem? System { get; set; }
    }
    public sealed class SpanshSystem
    {
        [JsonProperty("id64")]
        public long Id64 { get; set; }
        public long population { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;
        public Position coords { get; set; } = new();
        [JsonProperty("bodies")]
        public IReadOnlyList<SpanshBody> Bodies { get; set; } = [];
        public int bodyCount { get; set; }
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

        //[JsonProperty("signals")]
        //public RingSignals Signals { get; set; }
    }

    public partial class RingSignals
    {
        [JsonProperty("signals")]
        public PurpleSignals Signals { get; set; } = new();

        [JsonProperty("updateTime")]
        public string UpdateTime { get; set; } = string.Empty;
    }

    public partial class PurpleSignals
    {
        [JsonProperty("Monazite", NullValueHandling = NullValueHandling.Ignore)]
        public long? Monazite { get; set; }

        [JsonProperty("Platinum", NullValueHandling = NullValueHandling.Ignore)]
        public long? Platinum { get; set; }

        [JsonProperty("LowTemperatureDiamond", NullValueHandling = NullValueHandling.Ignore)]
        public long? LowTemperatureDiamond { get; set; }

        [JsonProperty("Rhodplumsite", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rhodplumsite { get; set; }

        [JsonProperty("Serendibite", NullValueHandling = NullValueHandling.Ignore)]
        public long? Serendibite { get; set; }

        [JsonProperty("Painite", NullValueHandling = NullValueHandling.Ignore)]
        public long? Painite { get; set; }
    }

    public sealed class SpanshBody
    {
        public long id64 { get; set; }
        public long population { get; set; }
        public int bodyId { get; set; }
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string subType { get; set; } = string.Empty;
        public bool? mainStar { get; set; }
        public bool? isLandable { get; set; }
        public bool? rotationalPeriodTidallyLocked { get; set; }
        public double? gravity { get; set; }
        public double? earthMasses { get; set; }
        public double? radius { get; set; }
        public double? surfaceTemperature { get; set; }
        public double? surfacePressure { get; set; }
        public string? atmosphereType { get; set; }
        public string? volcanismType { get; set; }
        public string? terraformingState { get; set; }
        public double distanceToArrival { get; set; }
        public string updateTime { get; set; } = string.Empty;
        [JsonProperty("rings", NullValueHandling = NullValueHandling.Ignore)]
        public Ring[]? Rings { get; set; }
    }
}
