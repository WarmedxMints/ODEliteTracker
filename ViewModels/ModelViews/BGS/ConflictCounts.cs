using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public record GroundCounflictCounts(string SettlementName, string Wins);

    public sealed class ConflictCounts : ODObservableObject
    {
        public ConflictCounts(FactionVM faction)
        {
            if (faction == null || faction.Wars == null)
            {
                return;
            }

            Name = faction.Name;
            LSCZCount = faction.Wars.LowSpaceCZ == 0 ? string.Empty : $"{faction.Wars.LowSpaceCZ:N0}";
            MSCZCount = faction.Wars.MediumSpaceCZ == 0 ? string.Empty: $"{faction.Wars.MediumSpaceCZ:N0}";
            HSCZCount = faction.Wars.HighSpaceCZ == 0 ? string.Empty : $"{faction.Wars.HighSpaceCZ:N0}";

            LGCZCount = faction.Wars.LowGroundCZ == 0 ? string.Empty : $"{faction.Wars.LowGroundCZ:N0}";
            MGCZCount = faction.Wars.MediumGroundCZ == 0 ? string.Empty : $"{faction.Wars.MediumGroundCZ:N0}";
            HGCZCount = faction.Wars.HighGroundCZ == 0 ? string.Empty : $"{faction.Wars.HighGroundCZ:N0}";

            if (faction.Wars != null)
            {
                foreach (var conflict in faction.Wars.Settlements.OrderBy(x => x.Key))
                {
                    GroundCounflictCounts.AddItem(new GroundCounflictCounts(conflict.Key, $"{conflict.Value:N0}"));
                }
            }
        }

        public string Name { get; } = string.Empty;
        public string LSCZCount { get; } = string.Empty;
        public string MSCZCount { get; } = string.Empty;
        public string HSCZCount { get; } = string.Empty;
        public string LGCZCount { get; } = string.Empty;
        public string MGCZCount { get; } = string.Empty;
        public string HGCZCount { get; } = string.Empty;
        public ObservableCollection<GroundCounflictCounts> GroundCounflictCounts { get; } = [];
    }
}
