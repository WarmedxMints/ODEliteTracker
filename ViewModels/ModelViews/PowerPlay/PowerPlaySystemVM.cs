using ODEliteTracker.Models.PowerPlay;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.PowerPlay
{
    public sealed class PowerPlaySystemVM : ODObservableObject
    {
        private readonly PowerPlaySystem system;


        public PowerPlaySystemVM(PowerPlaySystem system)
        {
            this.system = system;

            foreach (var kvp in system.CycleData)
            {
                _ = Data.TryAdd(kvp.Key, new(kvp.Value));
            }
        }
        public long Address => system.Address;
        public string NonUpperName => system.Name;
        public string Name => system.Name.ToUpper();
        public string Power => system.ControllingPower ?? string.Empty;
        public Dictionary<DateTime, PowerPlayCycleDataVM> Data { get; set; } = [];
        public string? ControllingPower => system.ControllingPower;
        public string PowerState => system.PowerState.ToString();
        public string? ControllingFaction => system.ControllingFaction;
        public string? SystemAllegiance => system.SystemAllegiance;

        public bool MeritsEarned(DateTime cycle)
        {
            if (Data.TryGetValue(cycle, out var data))
            {
                return data.MeritsEarnedValue > 0;
            }
            return false;
        }
    }
}
