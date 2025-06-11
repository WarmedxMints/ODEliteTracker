using EliteJournalReader.Events;

namespace ODEliteTracker.Models.Missions
{
    public sealed class MassacreMission : MissionBase
    {
        public MassacreMission(MissionAcceptedEvent.MissionAcceptedEventArgs args,
                               long originAddress,
                               string originSystemName,
                               ulong originMarketID,
                               string originStationName,
                               bool odyssey) : base(args,
                                                                originAddress,
                                                                originSystemName,
                                                                originMarketID,
                                                                originStationName,
                                                                odyssey)
        {
            Target = args.Target;
            TargetType = args.TargetType;
            TargetFaction = args.TargetFaction;
            TargetSystem = args.DestinationSystem;
            KillCount = args.KillCount ?? 0;
        }

        public string Target { get; set; }
        public string TargetType { get; set; }
        public string TargetSystem { get; set; }
        public int KillCount { get; set; }
        public int Kills { get; set; }  
    }
}
