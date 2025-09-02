using EliteJournalReader.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ODEliteTracker.Models.BGS
{
    public enum OnFootActivity
    {
        None = 0,
        [Description("Settlement Raid")]
        Raid,
        [Description("Low CZ")]
        LowCZ,
        [Description("Medium CZ")]
        MediumCZ,
        [Description("High CZ")]
        HighCZ
    }

    public sealed class SettlementActivity
    {
        internal OnFootActivity Activity { get; set; }
        internal int KillCount {  get; set; }
        internal int LowestBond { get; set; }
        internal int HighestBond { get; set; }
        internal int TotalBonds { get; set; }
        internal string SupportingFaction { get; set; } = string.Empty;
        internal string VictimFaction {  get; set; } = string.Empty;
        internal string SettlementName { get; set; } = string.Empty;
        internal EventHandler<SettlementActivity>? ActivityUpdated;
        #region Events
        internal void OnApproachSettlement(ApproachSettlementEvent.ApproachSettlementEventArgs e)
        {
            SettlementName = e.Name;
            ActivityUpdated?.Invoke(this, this);
        }

        internal void OnCommitCrime(CommitCrimeEvent.CommitCrimeEventArgs e)
        {
            if (string.Equals(e.CrimeType, "onFoot_murder", StringComparison.OrdinalIgnoreCase))
            {
                Activity = OnFootActivity.Raid;
                VictimFaction = e.Faction;
                KillCount++;
                ActivityUpdated?.Invoke(this, this);
            }
        }

        internal void OnFactionKillBond(FactionKillBondEvent.FactionKillBondEventArgs e)
        {
            if (e != null && string.IsNullOrEmpty(SettlementName) == false)
            {
                var type = SystemWarManager.GroundBondToConflictType(e.Reward);

                switch (type)
                {
                    case ConflictType.LowGroundCZ:
                        Activity = OnFootActivity.LowCZ;
                        break;
                    case ConflictType.MediumGroundCZ:
                        Activity = OnFootActivity.MediumCZ;
                        break;
                    case ConflictType.HighGroundCZ:
                        Activity = OnFootActivity.HighCZ;
                        break;
                }
                KillCount++;
                SupportingFaction = e.AwardingFaction;
                VictimFaction = e.VictimFaction;

                if (e.Reward < LowestBond || LowestBond == 0)
                {
                    LowestBond = e.Reward;
                }
                if (e.Reward > HighestBond || HighestBond == 0)
                {
                    HighestBond = e.Reward;
                }
                TotalBonds += e.Reward;
                ActivityUpdated?.Invoke(this, this);
            }
        }
        #endregion

        internal void Reset()
        {
            Activity = OnFootActivity.None;
            KillCount = LowestBond = HighestBond = TotalBonds = 0;
            SupportingFaction = VictimFaction = SettlementName = string.Empty;
            ActivityUpdated?.Invoke(this, this);
        }
    }
}
