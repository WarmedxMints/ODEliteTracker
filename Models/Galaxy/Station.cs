using EliteJournalReader;
using EliteJournalReader.Events;

namespace ODEliteTracker.Models.Galaxy
{
    public sealed class Station
    {
        public Station(CarrierJumpEvent.CarrierJumpEventArgs e, FactionData data, StarSystem system)
        {
            StarSystem = system;
            StationName = e.StationName;
            StationFaction = data;
            MarketID = e.MarketID;
            StationType = e.StationType;
        }

        public Station(LocationEvent.LocationEventArgs e, FactionData data, StarSystem system)
        {
            StarSystem = system;
            StationNameFDEV = e.StationName;
            StationName = e.StationName.Replace("$EXT_PANEL_ColonisationShip;", "Colonisation Ship -");
            StationFaction = data;
            MarketID = e.MarketID;
            StationType = e.StationType;
            DistanceToArrival = e.DistFromStarLS;
        }

        public Station(DockedEvent.DockedEventArgs e, FactionData data, StarSystem system)
        {
            StarSystem = system;
            StationNameFDEV = e.StationName;
            StationName = e.StationName.Replace("$EXT_PANEL_ColonisationShip;", "Colonisation Ship -");
            StationFaction = data;
            MarketID = e.MarketID;
            StationType = e.StationType;
            DistanceToArrival = e.DistFromStarLS;
            PadSize = GetPadSize(e.LandingPads);

        }

        public StarSystem StarSystem { get; }
        public string StationName { get; }
        public string StationNameFDEV { get; }
        public FactionData StationFaction { get; }
        public ulong MarketID { get;  }
        public string StationType { get; }
        public double DistanceToArrival { get;  }
        public LandingPadSize PadSize { get;  } = LandingPadSize.Small;

        private static LandingPadSize GetPadSize(LandingPads landingPads)
        {
            if (landingPads.Large > 0)
                return LandingPadSize.Large;
            if (landingPads.Medium > 0)
                return LandingPadSize.Medium;
            return LandingPadSize.Small;
        }
    }
}
