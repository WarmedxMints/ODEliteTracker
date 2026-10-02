using EliteJournalReader.Events;
using ODEliteTracker.Models.Galaxy;

namespace ODEliteTracker.Models.Colonisation
{
    public sealed class ConstructionDepot(ColonisationConstructionDepotEvent.ColonisationConstructionDepotEventArgs args, StarSystem system, string stationName, bool inactive)
    {
        public StarSystem StarSystem { get; private set; } = system;
        public bool Inactive { get; set; } = inactive;
        public ulong SystemAddress => StarSystem.Address;
        public string SystemName => StarSystem.Name;
        public ulong MarketID { get; set; } = args.MarketID;
        public string StationName { get; set; } = stationName;
        public double Progress { get; set; } = args.ConstructionProgress;
        public bool Complete { get; set; } = args.ConstructionComplete;
        public bool Failed { get; set; } = args.ConstructionFailed;
        public List<ConstructionResource> Resources { get; set; } = [.. args.ResourcesRequired.Select(resource => new ConstructionResource(resource))];

        internal bool Update(ColonisationConstructionDepotEvent.ColonisationConstructionDepotEventArgs args,
                             StarSystem system,
                             string currentStationName)
        {
            bool updated = system.Address != SystemAddress || Progress != args.ConstructionProgress || Complete != args.ConstructionComplete || Failed != args.ConstructionFailed;

            //If this happens then previous construction must be complete and it's on a new one maybe?
            if (SystemAddress != system.Address)
            {
                Resources.Clear();
            }

            if (updated)
            {
                this.StarSystem = system;
                StationName = currentStationName;
                Progress = args.ConstructionProgress;
                Complete = args.ConstructionComplete;
                Failed = args.ConstructionFailed;
            }

            foreach (var resource in args.ResourcesRequired)
            {
                var known = Resources.FirstOrDefault(res => string.Equals(res.FDEVName, resource.Name));

                if (known != null && known.Update(resource))
                {
                    updated = true;
                    continue;
                }
                if (known is null)
                {
                    Resources.Add(new(resource));
                    updated = true;
                }
            }

            return updated;
        }
    }
}
