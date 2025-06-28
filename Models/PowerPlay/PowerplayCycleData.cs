using EliteJournalReader;
using EliteJournalReader.Events;

namespace ODEliteTracker.Models.PowerPlay
{
    public sealed class PowerplayCycleData
    {
        public List<PowerPlayMerit> MeritList { get; set; } = [];
        public int MeritsEarned => MeritList.Sum(x => x.Value);
        public Dictionary<string, int> GoodsCollected { get; set; } = [];
        public Dictionary<string, int> GoodsDelivered { get; set; } = [];
        public string? ControllingPower { get; set; }
        public PowerplayState PowerState { get; set; }
        public double PowerplayStateControlProgress { get; set; }
        public int PowerplayStateReinforcement { get; set; }
        public int PowerplayStateUndermining { get; set; }
        public List<PowerConflict>? PowerConflict { get; set; }
        public List<string>? Powers { get; set; }
        internal void Update(PowerplayCycleData evt)
        {
            PowerplayStateControlProgress = evt.PowerplayStateControlProgress;
            PowerplayStateReinforcement = evt.PowerplayStateReinforcement;
            PowerplayStateUndermining = evt.PowerplayStateUndermining;
            PowerConflict = evt.PowerConflict?.Select(x => x.Copy()).ToList();
            PowerState = evt.PowerState;
            ControllingPower = evt.ControllingPower;
            Powers = evt.Powers;
        }
    }
}
