using EliteJournalReader.Events;
using ODMVVM.Helpers;

namespace ODEliteTracker.Models.Missions
{
    public enum MissionState
    {
        Active,
        Redirected,
        Completed,
        Failed,
        Abandoned
    }

    public class MissionBase
    {
        public MissionBase(MissionAcceptedEvent.MissionAcceptedEventArgs args,
                           long originAddress,
                           string originSystemName,
                           ulong originMarketID,
                           string originStationName,
                           bool odyssey) 
        { 
            Odyssey = odyssey;
            OriginSystemAddress = originAddress;
            OriginSystemName = originSystemName;
            OriginMarketID = originMarketID;
            OriginStationName = originStationName;
            MissionID = args.MissionID;
            Name = args.Name;
            LocalisedName = args.LocalisedName;
            IssuingFaction = args.Faction;
            TargetFaction = args.TargetFaction;
            Influence = args.Influence.MissionPlusCount();
            Reputation = args.Reputation.MissionPlusCount();
            Reward = args.Reward;
            Wing = args.Wing;
            Expiry = args.Expiry ?? args.Timestamp.AddMinutes(15);
            CurrentState = MissionState.Active;
            AcceptedTime = args.Timestamp;
            DestinationSystem = args.DestinationSystem;
            DestinationSettlement = args.DestinationSettlement;
            DestinationSystem = args.DestinationSystem;
        }
        public bool Odyssey { get; protected set; }
        public long OriginSystemAddress { get; protected set; }
        public string OriginSystemName { get; protected set; }
        public ulong OriginMarketID { get; protected set; }
        public string OriginStationName { get; protected set; }
        public MissionState CurrentState { get; set; }
        public DateTime AcceptedTime { get; protected set;}
        public ulong MissionID { get; protected set; }
        public string Name { get; protected set; }
        public string LocalisedName { get; protected set; }
        public string IssuingFaction { get; protected set; }
        public string TargetFaction { get; protected set; }
        public int Influence { get; protected set; }
        public int Reputation { get; protected set; }
        public int Reward { get; set; }
        public bool Wing { get; protected set; }
        public DateTime Expiry { get; protected set; }
        public DateTime CompletionTime { get; set; }
        public string DestinationSystem { get; protected set; }
        public string? DestinationStation { get; protected set; }
        public string? DestinationSettlement { get; protected set; }

        public void UpdateState(MissionState state, DateTime updateTime)
        {
            CurrentState = state;
        }
    }
}
