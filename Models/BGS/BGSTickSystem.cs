
using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Models.Galaxy;
using System.Windows.Forms;

namespace ODEliteTracker.Models.BGS
{
    public sealed class BGSTickSystem
    {
        public BGSTickSystem(BGSStarSystem starSystem, SystemTickData tickData, IEnumerable<VoucherClaim> claims, IEnumerable<TradeTransaction> transactions, IEnumerable<SystemCrime> crimes, IEnumerable<ExplorationData> carto, IEnumerable<SearchAndRescue> s_r, IEnumerable<SystemConflict> conflicts, IEnumerable<SystemWarZone> wars) 
        { 
            Name = starSystem.Name;
            Address = starSystem.Address;
            Population = tickData.Population;
            Position = starSystem.Position;
            ControllingFaction = tickData.ControllingFaction;
            Power = string.IsNullOrEmpty(starSystem.ControllingPower) ? "No Power" : starSystem.ControllingPower;
            SystemAllegiance = tickData.SystemAllegiance;
            VoucherClaims = [.. claims];
            Factions = tickData.Factions;
            Transactions = [.. transactions];
            Crimes = [.. crimes];
            Carto = [.. carto];
            SearchAndRescueData = [.. s_r];
            Conflicts = [.. conflicts];
            Wars = [.. wars];
            Security = starSystem.Security;

            //Powerplay
            ControllingPower = tickData.ControllingPower;
            PowerState = tickData.PowerState;
            PowerplayStateControlProgress = tickData.PowerplayStateControlProgress;
            PowerplayStateReinforcement = tickData.PowerplayStateReinforcement;
            PowerplayStateUndermining = tickData.PowerplayStateUndermining;
            PowerConflict = tickData.PowerConflict?.Select(x => x.Copy()).ToList();
            Powers = tickData.Powers?.ToList();
        }

        public string Name { get; }
        public long Address { get;  }
        public long Population { get; }
        public Position Position { get; }
        public string ControllingFaction { get;  }
        public string Power { get;  }
        public string SystemAllegiance { get; }
        public string? Security { get; }
        public List<VoucherClaim> VoucherClaims { get; }
        public List<Faction> Factions { get; } = [];
        public List<TradeTransaction> Transactions { get;  } 
        public List<SystemCrime> Crimes { get;  }
        public List<ExplorationData> Carto { get; }
        public List<SearchAndRescue> SearchAndRescueData { get; }
        public List<SystemConflict> Conflicts { get; }
        public List<SystemWarZone> Wars { get; }

        public string? ControllingPower { get; set; }
        public PowerplayState PowerState { get; set; }
        public double PowerplayStateControlProgress { get; set; }
        public int PowerplayStateReinforcement { get; set; }
        public int PowerplayStateUndermining { get; set; }
        public List<PowerConflict>? PowerConflict { get; set; }
        public List<string>? Powers { get; set; }

    }
}
