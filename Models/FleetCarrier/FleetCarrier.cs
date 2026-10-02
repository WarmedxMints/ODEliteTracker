using EliteJournalReader;
using EliteJournalReader.Events;
using ODCapi.Models;

namespace ODEliteTracker.Models.FleetCarrier
{
    public enum CrewStatus
    {
        Active,
        Inactive,
        Suspended
    }

    public sealed class FleetCarrier
    {
        public FleetCarrier(CAPIFleetCarrier args) 
        {
            CarrierID = args.Market.Id;
            Callsign = args.Name.Callsign;
            Name = string.Empty;
            DockingAccess = args.DockingAccess;
            AllowNotorious = args.NotoriousAccess;
            FuelLevel = args.Fuel;
            Balance = args.Balance;
        }

        public FleetCarrier(CarrierStatsEvent.CarrierStatsEventArgs args)
        {
            CarrierID = args.CarrierID;
            Callsign = args.Callsign;
            Name = args.Name;
            DockingAccess = args.DockingAccess;
            AllowNotorious = args.AllowNotorious;
            FuelLevel = args.FuelLevel;
            Balance = args.Finance.CarrierBalance;
        }

        public ulong CarrierID { get; set; }
        public string Callsign { get; set; }
        public string Name { get; set; }
        public string DockingAccess { get; set; }
        public bool AllowNotorious { get; set; }
        public long FuelLevel { get; set; }
        public long Balance { get; set; }
        public List<CarrierCommodity> Stock { get; set; } = [];
        //public List<CarrierCommodity> BuyOrders { get; set; } = [];
        public string StarSystem { get; internal set; } = string.Empty;
        public ulong SystemAddress { get; internal set; }
        public int BodyID { get; internal set; }

        public FleetCarrierDestination Destination { get; set; } = new();
        public Dictionary<CarrierCrewRole, CrewStatus> Crew { get; set; } = [];
        internal void AssignCrew(IReadOnlyList<CarrierStatsEvent.CarrierCrew> crew)
        {
            Crew.Clear();

            foreach(var member in crew)
            {
                var status = member.Activated ?
                    member.Enabled ? CrewStatus.Active : CrewStatus.Suspended : CrewStatus.Inactive;

                Crew.TryAdd(member.CrewRole, status);
            }
        }

        internal void UpdateCrew(CarrierCrewServicesEvent.CarrierCrewServicesEventArgs e)
        {
            var state = CrewStatus.Inactive;

            switch (e.Operation)
            {
                case CarrierCrewOperation.Activate:
                case CarrierCrewOperation.Resume:
                    state = CrewStatus.Active;
                    break;
                case CarrierCrewOperation.Pause:
                    state = CrewStatus.Suspended;
                    break;
                case CarrierCrewOperation.Deactivate:
                    state = CrewStatus.Inactive;
                    break;
                case CarrierCrewOperation.Replace:
                case CarrierCrewOperation.Unknown:
                    return; 
            }

            if (Crew.ContainsKey(e.CrewRole))
            {
                Crew[e.CrewRole] = state;
                return;
            }

            Crew.TryAdd(e.CrewRole, state);
        }
    }
}
