using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Models.PowerPlay;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.PowerPlay
{
    public sealed class PowerPlayCycleDataVM : ODObservableObject
    {
        private readonly PowerplayCycleData data;

        public PowerPlayCycleDataVM(PowerplayCycleData data) 
        {          
            this.data = data;

            foreach (var item in data.GoodsCollected)
            {
                GoodsCollected.Add(new(item.Key, item.Value));
            }

            foreach (var item in data.GoodsDelivered)
            {
                GoodsDelivered.Add(new(item.Key, item.Value));
            }

            if (data.PowerConflict != null && data.PowerConflict.Count > 0)
            {
                SetConflict(data.PowerConflict);                
            }

            if (data.MeritList.Count > 0)
            {
                var dict = data.MeritList.GroupBy(x => x.Activity).ToDictionary(x => x.Key, x => x.ToList());

                foreach (var kvp in dict)
                {
                    var merits = kvp.Value.Sum(x => x.Value);
                    var count = kvp.Value.Count;

                    Merits.Add(new PPMeritsVM(kvp.Key, merits, count));
                }

                Merits.Sort((x, y) => x.Activity.CompareTo(y.Activity));
            }
        }

        private void SetConflict(List<PowerConflict> data)
        {
            var ordered = data.OrderByDescending(x => x.ConflictProgress).ToList();

            var winning = ordered.First();
            ordered.Remove(winning);

            PowerPlayConflictData = new PowerPlayConflictDataVM()
            {
                WinningPower = new(winning)
            };

            if (ordered.Count == 0)
            {
                PowerPlayConflictData.LosingPowers = [new(new("No Opposing Power", -1))];
                PowerPlayConflictData.ConflictState = "Expansion";
                return;
            }

            PowerPlayConflictData.LosingPowers = [.. ordered.Select(x => new PowerPlayConflictVM(x))];
            PowerPlayConflictData.ConflictState = "Contested";
        }

        public List<PPMeritsVM> Merits { get; } = [];
        public int MeritsEarnedValue => data.MeritsEarned;
        public string MeritsEarned => $"{data.MeritsEarned:N0}";
        public string ControllingPower => data.ControllingPower ?? "";
        public PowerplayState PowerState => data.PowerState;
        public string PowerStateString => data.PowerState.GetEnumDescription();
        public string PowerplayStateControlProgress => $"{data.PowerplayStateControlProgress * 100:N2} %";
        public string PowerplayStateReinforcement => $"{data.PowerplayStateReinforcement:N0}";
        public string PowerplayStateUndermining => $"{data.PowerplayStateUndermining:N0}";
        public List<PowerPlayItemVM> GoodsCollected { get; } = [];
        public List<PowerPlayItemVM> GoodsDelivered { get; } = [];
        public List<string>? Powers => data.Powers;
        public List<string> OpposingPowers
        {
            get
            {
                var opposing = Powers?.Where(x => string.Equals(ControllingPower, x) == false);

                if (opposing is null || !opposing.Any())
                {
                    return ["No Opposing Power"];
                }
                var ret = opposing.ToList();
                ret.Sort();
                return ret;
            }
        }
        public PowerPlayConflictDataVM? PowerPlayConflictData { get; set; }
    }
}
