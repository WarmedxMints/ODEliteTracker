using ODEliteTracker.Models.BGS;
using ODEliteTracker.Models.Galaxy;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class BGSTickSystemVM : ODObservableObject
    {
        public BGSTickSystemVM(BGSTickSystem system) 
        { 
   
            this.system = system;
            Factions = [.. system.Factions.OrderByDescending(x => x.Influence).Select(x => new FactionVM(x))];

            foreach (var voucher in system.VoucherClaims)
            {
                switch (voucher.VoucherType)
                {
                    case Models.VoucherType.Bounty:
                        var faction = Factions.FirstOrDefault(x => string.Equals(x.Name,voucher.Faction,StringComparison.OrdinalIgnoreCase));

                        if (faction != null)
                            faction.Bounties += voucher.Value;
                        continue;
                    case Models.VoucherType.CombatBond:
                        var fction = Factions.FirstOrDefault(x => string.Equals(x.Name, voucher.Faction, StringComparison.OrdinalIgnoreCase));

                        if (fction != null)
                            fction.Bonds += voucher.Value;
                        continue;
                    default:
                        continue;
                }
            }

            if(system.Transactions.Count > 0)
            {
                foreach (var transaction in system.Transactions)
                {
                    var faction = Factions.FirstOrDefault(x => string.Equals(x.Name, transaction.Faction.Name, StringComparison.OrdinalIgnoreCase));

                    faction?.AddTraction(transaction);
                }
            }

            if(system.Crimes.Count > 0)
            {
                var crimes = system.Crimes.GroupBy(x => x.TargetFaction.Name).ToDictionary(x => x.Key, x => x);

                foreach(var kvp in crimes)
                {
                    var faction = Factions.FirstOrDefault(x => string.Equals(x.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                    if(faction == null)
                        continue;

                    faction.AddMurders(kvp.Value);
                }
            }

            if(system.Carto.Count > 0)
            {
                var crimes = system.Carto.GroupBy(x => x.Faction.Name).ToDictionary(x => x.Key, x => x);

                foreach (var kvp in crimes)
                {
                    var faction = Factions.FirstOrDefault(x => string.Equals(x.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                    if (faction == null)
                        continue;

                    faction.AddCartoData(kvp.Value);
                }
            }

            if (system.SearchAndRescueData.Count > 0)
            {
                foreach (var item in system.SearchAndRescueData)
                {
                    var faction = Factions.FirstOrDefault(x => string.Equals(x.Name, item.Faction.Name, StringComparison.OrdinalIgnoreCase));

                    if (faction == null)
                        continue;

                    faction.AddSearchAndRescue(item);
                }
            }

            if (system.Conflicts.Count > 0)
            {
                Conflicts = [.. system.Conflicts.Select(x => new SystemConflictVM(x)).OrderBy(x => x.Status)];
            }

            if(system.Wars.Count > 0)
            {
                foreach (var item in system.Wars)
                {
                    var faction = Factions.FirstOrDefault(x => string.Equals(item.SupportingFaction, x.Name, StringComparison.OrdinalIgnoreCase));

                    if(faction == null)
                        continue;

                    faction.Wars ??= new();

                    switch (item.Type)
                    {
                        case Models.ConflictType.None:
                            continue;
                        case Models.ConflictType.LowSpaceCZ:
                            faction.Wars.LowSpaceCZ++;
                            continue;
                        case Models.ConflictType.MediumSpaceCZ:
                            faction.Wars.MediumSpaceCZ++;
                            continue;
                        case Models.ConflictType.HighSpaceCZ:
                            faction.Wars.HighSpaceCZ++;
                            continue;
                        case Models.ConflictType.LowGroundCZ:
                            faction.Wars.LowGroundCZ++;
                            faction.Wars.AddSettlement($"{item.SettlementName} [L]");
                            continue;
                        case Models.ConflictType.MediumGroundCZ:
                            faction.Wars.MediumGroundCZ++;
                            faction.Wars.AddSettlement($"{item.SettlementName} [M]");
                            continue;
                        case Models.ConflictType.HighGroundCZ:
                            faction.Wars.HighGroundCZ++;
                            faction.Wars.AddSettlement($"{item.SettlementName} [H]");
                            continue;
                        default:
                            continue;
                    }
                }
            }
        }

        private readonly BGSTickSystem system;
        public string NonUpperName => system.Name;
        public string Name => system.Name.ToUpper();
        public long Address => system.Address;
        public string Population => $"{system.Population:N0}";
        public Position Position => system.Position;
        public string ControllingFaction => system.ControllingFaction;
        public string? Security => system.Security;
        public string? ControllingFactionState => Factions.FirstOrDefault(x => string.Equals(x.Name, ControllingFaction))?.FactionState;
        public string SystemAllegiance => system.SystemAllegiance;
        public List<FactionVM> Factions { get; }
        public List<SystemConflictVM> Conflicts { get; } = [];    
        public bool HasData => Factions.Any(x => x.HasData());
        public bool HasConflict => Conflicts.Count > 0;
    }
}
