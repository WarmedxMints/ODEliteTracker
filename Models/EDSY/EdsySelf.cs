using Newtonsoft.Json;

namespace ODEliteTracker.Models.EDSY
{
    public sealed class EdsySelf
    {
        [JsonProperty("header")]
        public Header Header { get; set; } = new();

        [JsonProperty("data")]
        public Data? Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("event")]
        public string? Event { get; set; }

        [JsonProperty("Ship")]
        public string? Ship { get; set; }

        [JsonProperty("ShipName")]
        public string? ShipName { get; set; }

        [JsonProperty("ShipIdent")]
        public string? ShipIdent { get; set; }

        [JsonProperty("HullValue")]
        public long HullValue { get; set; }

        [JsonProperty("ModulesValue")]
        public long ModulesValue { get; set; }

        [JsonProperty("UnladenMass")]
        public double UnladenMass { get; set; }

        [JsonProperty("CargoCapacity")]
        public long CargoCapacity { get; set; }

        [JsonProperty("MaxJumpRange")]
        public double MaxJumpRange { get; set; }

        [JsonProperty("FuelCapacity")]
        public FuelCapacity? FuelCapacity { get; set; }

        [JsonProperty("Rebuy")]
        public long Rebuy { get; set; }

        [JsonProperty("Modules")]
        public List<Module>? Modules { get; set; }
    }

    public partial class FuelCapacity
    {
        [JsonProperty("Main")]
        public long Main { get; set; }

        [JsonProperty("Reserve")]
        public double Reserve { get; set; }
    }

    public partial class Module
    {
        [JsonProperty("Slot")]
        public string? Slot { get; set; }

        [JsonProperty("Item")]
        public string? Item { get; set; }

        [JsonProperty("On")]
        public bool On { get; set; }

        [JsonProperty("Priority")]
        public long Priority { get; set; }

        [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("Engineering", NullValueHandling = NullValueHandling.Ignore)]
        public Engineering? Engineering { get; set; }
    }

    public partial class Engineering
    {
        [JsonProperty("BlueprintName", NullValueHandling = NullValueHandling.Ignore)]
        public string? BlueprintName { get; set; }

        [JsonProperty("Level", NullValueHandling = NullValueHandling.Ignore)]
        public long? Level { get; set; }

        [JsonProperty("Quality", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quality { get; set; }

        [JsonProperty("Modifiers")]
        public List<Modifier>? Modifiers { get; set; }

        [JsonProperty("ExperimentalEffect", NullValueHandling = NullValueHandling.Ignore)]
        public string? ExperimentalEffect { get; set; }
    }

    public partial class Modifier
    {
        [JsonProperty("Label")]
        public string? Label { get; set; }

        [JsonProperty("Value")]
        public double Value { get; set; }

        [JsonProperty("OriginalValue")]
        public double OriginalValue { get; set; }
    }

    public partial class Header
    {
        [JsonProperty("appName")]
        public string? AppName { get; set; } = "OD Elite Tracker";

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; } = App.AppVersion.ToString();

        //[JsonProperty("appURL")]
        //public Uri AppUrl { get; set; } = new Uri("https://edsy.org/");
    }

}
