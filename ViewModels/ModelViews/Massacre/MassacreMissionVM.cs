using ODEliteTracker.Models.Missions;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Massacre
{
    public sealed class MassacreMissionVM : ODObservableObject
    {
        public MassacreMissionVM(MassacreMission mission)
        {
            OriginSystemAddress = mission.OriginSystemAddress;
            OriginSystemName = mission.OriginSystemName;
            OriginMarketID = mission.OriginMarketID;
            OriginStationName = mission.OriginStationName;
            TargetSystem = mission.TargetSystem;
            CurrentState = mission.CurrentState;
            AcceptedTime = mission.AcceptedTime;
            MissionID = mission.MissionID;
            IssuingFaction = mission.IssuingFaction;
            Influence = mission.Influence;
            Reputation = mission.Reputation;
            Reward = mission.Reward;
            Wing = mission.Wing;
            Expiry = mission.Expiry;
            DestinationSystem = mission.DestinationSystem;
            Target = mission.Target;
            TargetType = mission.TargetType;
            TargetFaction = mission.TargetFaction;
            KillCount = mission.KillCount;
            Kills = mission.Kills;
            CompletionTime = mission.CompletionTime.ToLocalTime();
        }

        public long OriginSystemAddress { get; private set; }
        public string OriginSystemName { get; private set; }
        public ulong OriginMarketID { get; private set; }
        public string OriginStationName { get; private set; }
        public string TargetSystem { get; private set; }
        public MissionState CurrentState { get; set; }
        public DateTime AcceptedTime { get; private set; }
        public ulong MissionID { get; set; }
        public string IssuingFaction { get; set; }
        public int Influence { get; set; }
        public int Reputation { get; set; }
        public int Reward { get; set; }
        public bool Wing { get; set; }
        public DateTime Expiry { get; set; }
        public DateTime CompletionTime { get; set; }
        public string ExpiryRelativeTime => Expiry.RelativeTime(DateTime.UtcNow);
        public string DestinationSystem { get; set; }
        public string Target { get; set; }
        public string TargetType { get; set; }
        public string TargetFaction { get; set; }
        public int KillCount { get; set; }
        public int Kills { get; set; }
        public string KillsString => $"{Kills} / {KillCount}";
        public bool AtStation { get; private set; }
        internal void Update(MassacreMission mission)
        {
            CurrentState = mission.CurrentState;
            Influence = mission.Influence;
            Reputation = mission.Reputation;
            DestinationSystem = mission.DestinationSystem;
            Kills = mission.Kills;

            if (mission.CurrentState == MissionState.Completed)
            {
                CompletionTime = mission.CompletionTime.ToLocalTime();
            }
            OnPropertyChanged(nameof(CurrentState));
            OnPropertyChanged(nameof(Influence));
            OnPropertyChanged(nameof(Reputation));
            OnPropertyChanged(nameof(ExpiryRelativeTime));
            OnPropertyChanged(nameof(DestinationSystem));
            OnPropertyChanged(nameof(Kills));
        }

        internal void UpdateStation(ulong marketId)
        {
            if(CurrentState != MissionState.Redirected)
            {
                return;
            }

            AtStation = marketId == OriginMarketID;
            OnPropertyChanged(nameof(AtStation));
        }
        internal void UpdateExpiryTime()
        {
            OnPropertyChanged(nameof(ExpiryRelativeTime));
        }
    }
}
