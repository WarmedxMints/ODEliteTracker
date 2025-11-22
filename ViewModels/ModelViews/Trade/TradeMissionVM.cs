using ODEliteTracker.Models;
using ODEliteTracker.Models.Missions;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Trade
{
    public sealed class TradeMissionVM : ODObservableObject
    {
        public TradeMissionVM(TradeMission mission)
        {
            OriginSystemAddress = mission.OriginSystemAddress;
            OriginSystemName = mission.OriginSystemName;
            OriginMarketID = mission.OriginMarketID;
            OriginStationName = mission.OriginStationName;
            CurrentState = mission.CurrentState;
            AcceptedTime = mission.AcceptedTime;
            MissionID = mission.MissionID;
            IssuingFaction = mission.IssuingFaction;
            Influence = mission.Influence;
            Reputation = mission.Reputation;
            Reward = mission.Reward;
            Wing = mission.Wing;
            Expiry = mission.Expiry;
            destinationSystem = mission.DestinationSystem;
            destinationStation = mission.DestinationStation ?? string.Empty;
            FdevCommodity = mission.FdevCommodity;
            Commodity_Localised = mission.Commodity_Localised;
            Count = mission.Count;
            MissionName = mission.LocalisedName ?? mission.Name;
            ItemsCollected = mission.ItemsCollected;
            ItemsDelivered = mission.ItemsDelivered;
            CompletionTime = mission.CompletionTime.ToLocalTime();

            switch (mission.Name)
            {
                case string a when a.StartsWith("Mission_Mining", StringComparison.OrdinalIgnoreCase):
                    MissionType = TradeMissionType.Mining;
                    break;
                case string b when b.StartsWith("Mission_Delivery", StringComparison.OrdinalIgnoreCase):
                    MissionType = TradeMissionType.Delivery;
                    break;
                case string c when c.StartsWith("Mission_Collect", StringComparison.OrdinalIgnoreCase):
                    MissionType = TradeMissionType.SourceAndReturn;
                    break;
            }
        }

        public TradeMissionType MissionType { get; }
        public bool DeliveryMission => MissionType == TradeMissionType.Delivery;
        public long OriginSystemAddress { get; private set; }
        public string OriginSystemName { get; private set; }
        public ulong OriginMarketID { get; private set; }
        public string OriginStationName { get; private set; }
        public string MissionName { get; private set; }
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

        private string destinationSystem;
        public string DestinationSystem 
        {
            get
            {
                return destinationSystem;
            }
            set => destinationSystem = value; 
        }

        private string destinationStation;
        public string DestinationStation 
        {
            get
            {
                return destinationStation;
            }
            set => destinationStation = value; 
        }
        public string FdevCommodity { get; set; }
        public string Commodity_Localised { get; set; } 
        public int Count { get; set; }
        public int ItemsCollected { get; set; }
        public int ItemsToCollectRemaining => Count - ItemsCollected;
        public int ItemsDelivered { get; set; }
        public int ItemsToDeliverRemaining => Count - ItemsDelivered;
        public bool ReadyToTurnIn
        {
            get
            {
                if (DeliveryMission)
                    return ItemsToCollectRemaining > 0 == false;

                return ItemsToDeliverRemaining > 0 == false;
            }
        }
        internal void Update(TradeMission mission)
        {
            CurrentState = mission.CurrentState;
            Influence = mission.Influence;
            Reputation = mission.Reputation;
            DestinationSystem = mission.DestinationSystem;
            DestinationStation = mission.DestinationStation ?? string.Empty;
            Count = mission.Count;
            ItemsDelivered = mission.ItemsDelivered;
            ItemsCollected = mission.ItemsCollected;

            if (mission.CurrentState == MissionState.Completed)
            {
                CompletionTime = mission.CompletionTime.ToLocalTime();
            }

            OnPropertyChanged(nameof(CurrentState));
            OnPropertyChanged(nameof(Influence));
            OnPropertyChanged(nameof(Reputation));
            OnPropertyChanged(nameof(ExpiryRelativeTime));
            OnPropertyChanged(nameof(DestinationSystem));
            OnPropertyChanged(nameof(DestinationStation));
            OnPropertyChanged(nameof(Count));
            OnPropertyChanged(nameof(ItemsDelivered));
            OnPropertyChanged(nameof(ItemsCollected));
        }

        internal void UpdateExpiryTime()
        {
            OnPropertyChanged(nameof(ExpiryRelativeTime));
        }
    }
}
