using EliteJournalReader.Events;
using ODEliteTracker.Database;
using ODEliteTracker.Models.BGS;
using ODJournalDatabase.Database.Interfaces;

namespace ODEliteTracker.Managers
{
    public sealed class BountiesManager
    {
        public BountiesManager(IODDatabaseProvider databaseProvider)
        {
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
        }

        private readonly ODEliteTrackerDatabaseProvider databaseProvider;

        private readonly Dictionary<string, List<VoucherClaim>> bounties = new(StringComparer.InvariantCultureIgnoreCase);
        private Dictionary<string, DateTime> ignoreBounties = [];

        public void Initialise(int commanderID)
        {
            bounties.Clear();
            UpdateIgnored(commanderID);
        }

        public void UpdateIgnored(int commanderID)
        {
            ignoreBounties = databaseProvider.GetIgnoredBounties(commanderID);
        }

        public void AddBounty(VoucherClaim voucherClaim)
        {
            if (string.IsNullOrEmpty(voucherClaim.Faction))
                return;

            if (bounties.TryGetValue(voucherClaim.Faction, out var claims))
            {
                claims.Add(voucherClaim);
                return;
            }

            bounties.TryAdd(voucherClaim.Faction, [voucherClaim]);
        }

        public bool FactionBountiesClaimed(RedeemVoucherEvent.RedeemVoucherEventArgs.FactionAmount faction, double? brokerPercentage)
        { 
            if (string.IsNullOrEmpty(faction.Faction))
            {
                var known = bounties.FirstOrDefault(x => IsWithinPercentageRange(faction.Amount, x.Value.Sum(claim => claim.Value), brokerPercentage ?? 0));

                if (string.IsNullOrEmpty(known.Key) == false)
                    return bounties.Remove(known.Key);
                return false;
            }
            if (!bounties.ContainsKey(faction.Faction))
            {
                return false;
            }

            bounties.Remove(faction.Faction);
            return true;
        }

        public static bool IsWithinPercentageRange(double value, double baseValue, double percentage)
        {
            double lowerBound = baseValue - (baseValue * percentage / 100);
            double upperBound = baseValue + (baseValue * percentage / 100);
            return value >= lowerBound && value <= upperBound;
        }

        public void AddIgnoredBounty(int commanderID, string factionName)
        {
            databaseProvider.AddIgnoredBounty(commanderID, factionName, DateTime.UtcNow);
            UpdateIgnored(commanderID);
        }

        public void RemovedIgnoreBounty(int commanderID, string factionName)
        {
            databaseProvider.DeleteIgnoredBounty(commanderID, factionName);
            UpdateIgnored(commanderID);
        }

        public List<BountyClaims> GetBounties()
        {
            var ret = new List<BountyClaims>();

            foreach (var voucherClaim in bounties)
            {
                if (ignoreBounties.TryGetValue(voucherClaim.Key, out var time))
                {
                    var vouchers = voucherClaim.Value.Where(x => x.Timestamp > time);

                    if (vouchers.Any())
                    {
                        ret.Add(new(voucherClaim.Key, vouchers.Sum(x => x.Value), vouchers.Count()));
                    }
                    continue;
                }

                ret.Add(new(voucherClaim.Key, voucherClaim.Value.Sum(x => x.Value), voucherClaim.Value.Count));
            }

            return ret;
        }

        internal void Reset()
        {
            bounties.Clear();
        }
    }

}
