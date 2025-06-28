using EliteJournalReader;
using EliteJournalReader.Events;
using System.Windows.Forms;

namespace ODEliteTracker.Models.BGS
{
    public sealed class SystemTickData
    {
        public SystemTickData(LocationEvent.LocationEventArgs evt)
        {
            VisitedTimes = [evt.Timestamp];
            ControllingFaction = evt.SystemFaction?.Name ?? "Unknown";
            SystemAllegiance = evt.SystemAllegiance;
            Population = evt.Population ?? 0;

            if (evt.Factions != null && evt.Factions.Count != 0)
            {
                Factions = [.. evt.Factions.Select(x => x.Clone())];
            }

            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            PowerplayStateControlProgress = evt.PowerplayStateControlProgress;
            PowerplayStateReinforcement = evt.PowerplayStateReinforcement;
            PowerplayStateUndermining = evt.PowerplayStateUndermining;
            PowerConflict = evt.PowerplayConflictProgress?.Select(x => x.Copy()).ToList();
            Powers = evt.Powers?.ToList();
        }

        public SystemTickData(FSDJumpEvent.FSDJumpEventArgs evt)
        {
            VisitedTimes = [evt.Timestamp];
            ControllingFaction = evt.SystemFaction?.Name ?? "Unknown";
            SystemAllegiance = evt.SystemAllegiance;
            Population = evt.Population;

            if (evt.Factions != null && evt.Factions.Count != 0)
            {
                Factions = [.. evt.Factions.Select(x => x.Clone())];
            }

            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            PowerplayStateControlProgress = evt.PowerplayStateControlProgress;
            PowerplayStateReinforcement = evt.PowerplayStateReinforcement;
            PowerplayStateUndermining = evt.PowerplayStateUndermining;
            PowerConflict = evt.PowerplayConflictProgress?.Select(x => x.Copy()).ToList();
            Powers = evt.Powers?.ToList();
        }

        public SystemTickData(CarrierJumpEvent.CarrierJumpEventArgs evt)
        {
            VisitedTimes = [evt.Timestamp];
            ControllingFaction = evt.SystemFaction?.Name ?? "Unknown";
            SystemAllegiance = evt.SystemAllegiance;
            Population = evt.Population ?? 0;

            if (evt.Factions != null && evt.Factions.Count != 0)
            {
                Factions = [.. evt.Factions.Select(x => x.Clone())];
            }

            ControllingPower = evt.ControllingPower;
            PowerState = evt.PowerplayState;
            PowerplayStateControlProgress = evt.PowerplayStateControlProgress;
            PowerplayStateReinforcement = evt.PowerplayStateReinforcement;
            PowerplayStateUndermining = evt.PowerplayStateUndermining;
            PowerConflict = evt.PowerplayConflictProgress?.Select(x => x.Copy()).ToList();
            Powers = evt.Powers?.ToList();
        }

        public List<DateTime> VisitedTimes { get; }

        public string ControllingFaction { get;  }
        public string SystemAllegiance { get;  }
        public long Population { get; }
        public List<Faction> Factions { get; } = [];
        public string? ControllingPower { get; set; }
        public PowerplayState PowerState { get; set; }
        public double PowerplayStateControlProgress { get; set; }
        public int PowerplayStateReinforcement { get; set; }
        public int PowerplayStateUndermining { get; set; }
        public List<PowerConflict>? PowerConflict { get; set; }
        public List<string>? Powers { get; set; }

        public bool NewData(SystemTickData other)
        {
            var ret = !string.Equals(ControllingFaction, other.ControllingFaction);

            if (ret == true)
                return true;

            if(Factions.Count != other.Factions.Count) 
                return true;

            
            foreach (var faction in other.Factions)
            {
                var known = Factions.FirstOrDefault(x => x.Name == faction.Name);

                if (known == null)
                {
                    return true;
                }

                if(known.BGSDataUpdated(faction) == false)
                {
                    return true;
                }
            }

            //Update Powerplay data
            ControllingPower = other.ControllingPower;
            PowerState = other.PowerState;
            PowerplayStateControlProgress = other.PowerplayStateControlProgress;
            PowerplayStateReinforcement = other.PowerplayStateReinforcement;
            PowerplayStateUndermining = other.PowerplayStateUndermining;
            PowerConflict = other.PowerConflict?.Select(x => x.Copy()).ToList();
            Powers = other.Powers?.ToList();
            //Data is unchanged so add another visit to this
            VisitedTimes.AddRange(other.VisitedTimes);
            return false;
        }

        public bool VisitedDuringPeriod(DateTime from, DateTime to)
        {
            return VisitedTimes.Any(x => x >= from && x < to);
        }
    }
}
