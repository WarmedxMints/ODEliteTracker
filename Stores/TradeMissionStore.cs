using EliteJournalReader.Events;
using EliteJournalReader;
using ODEliteTracker.Models.Missions;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class TradeMissionStore : LogProcessorBase
    {
        public TradeMissionStore(IManageJournalEvents journalManager)
        {
            this.journalManager = journalManager;
            this.journalManager.RegisterLogProcessor(this);
        }

        private IManageJournalEvents journalManager;
        private readonly List<TradeMission> missions = [];

        private readonly string[] validMissionNames = ["Mission_Collect", "Mission_Delivery", "Mission_Mining", "Mission_Altruism"];
        private long CurrentSystemAddress;
        private string CurrentSystemName = "Unknown";
        private ulong CurrentMarketID;
        private string CurrentStationName = "Unknown";
        private bool odyssey;
        public override string StoreName => "Trade Mission";
        public List<TradeMission> Missions { get { return missions; } }
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.LoadGame,true},
                { JournalTypeEnum.Location,true},
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.MissionAbandoned, true},
                { JournalTypeEnum.MissionAccepted, true},
                { JournalTypeEnum.MissionCompleted, true},
                { JournalTypeEnum.MissionFailed, true},
                { JournalTypeEnum.MissionRedirected, true},
                { JournalTypeEnum.Missions, true},
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.Undocked, true},
                { JournalTypeEnum.CargoDepot, true},
            };
        }

        public EventHandler<TradeMission>? OnMissionAddedEvent;
        public EventHandler<TradeMission>? OnMissionUpdatedEvent;
        public EventHandler<TradeMission>? OnCargoDepot;
        public EventHandler? OnMissionsUpdatedEvent;

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case LoadGameEvent.LoadGameEventArgs load:
                    odyssey = load.Odyssey;
                    break;
                case LocationEvent.LocationEventArgs location:
                    CurrentSystemAddress = location.SystemAddress;
                    CurrentSystemName = location.StarSystem;

                    if (string.IsNullOrEmpty(location.StationName) == false)
                    {
                        CurrentStationName = location.StationName;
                        CurrentMarketID = location.MarketID;
                    }
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    CurrentSystemAddress = fsdJump.SystemAddress;
                    CurrentSystemName = fsdJump.StarSystem;
                    break;
                case MissionAcceptedEvent.MissionAcceptedEventArgs accepted:
                    if (string.IsNullOrEmpty(CurrentStationName)
                       || ValidMission(accepted.Count, accepted.Name) == false)
                    {
                        break;
                    }

                    var mission = new TradeMission(accepted,
                                                      CurrentSystemAddress,
                                                      CurrentSystemName,
                                                      CurrentMarketID,
                                                      CurrentStationName,
                                                      odyssey);

                    missions.Add(mission);
                    if (IsLive)
                        OnMissionAddedEvent?.Invoke(this, mission);
                    break;
                case MissionRedirectedEvent.MissionRedirectedEventArgs redirected:
                    var rMission = missions.FirstOrDefault(x => x.MissionID == redirected.MissionID);
                    if (rMission != null)
                    {
                        rMission.CurrentState = MissionState.Redirected;
                        if (IsLive)
                            OnMissionUpdatedEvent?.Invoke(this, rMission);
                    }
                    break;
                case MissionCompletedEvent.MissionCompletedEventArgs completed:
                    var cMission = missions.FirstOrDefault(x => x.MissionID == completed.MissionID);
                    if (cMission != null)
                    {
                        cMission.CurrentState = MissionState.Completed;
                        cMission.CompletionTime = completed.Timestamp;
                        cMission.Reward = completed.Reward == 0 ? -completed.Donated : completed.Reward;
                        if (IsLive)
                            OnMissionUpdatedEvent?.Invoke(this, cMission);
                    }
                    break;
                case MissionFailedEvent.MissionFailedEventArgs missionFailed:
                    var fMission = missions.FirstOrDefault(x => x.MissionID == missionFailed.MissionID);
                    if (fMission != null)
                    {
                        fMission.CurrentState = MissionState.Failed;
                        fMission.Reward = 0;
                        if (IsLive)
                            OnMissionUpdatedEvent?.Invoke(this, fMission);
                    }
                    break;
                case MissionAbandonedEvent.MissionAbandonedEventArgs abandoned:
                    var aMission = missions.FirstOrDefault(x => x.MissionID == abandoned.MissionID);
                    if (aMission != null)
                    {
                        aMission.CurrentState = MissionState.Abandoned;
                        aMission.Reward = 0;
                        if (IsLive)
                            OnMissionUpdatedEvent?.Invoke(this, aMission);
                    }
                    break;
                case MissionsEvent.MissionsEventArgs missionsEvt:
                    //Check if we've started an operation and shouldn't check for missions to be removed
                    if (missionsEvt.Active.Count == 1 && missionsEvt.Failed.Count == 0 && missionsEvt.Complete.Count == 0
                        && missionsEvt.Active[0].Expires == 0)
                    {
                        break;
                    }
                    var evtMissions = new List<Mission>();

                    evtMissions.AddRange(missionsEvt.Active);
                    evtMissions.AddRange(missionsEvt.Complete);
                    evtMissions.AddRange(missionsEvt.Failed);

                    if (evtMissions.Count == 0)
                        break;
                    var missionsToRemove = new List<TradeMission>();

                    foreach (var m in missions)
                    {
                        if (m.CurrentState == MissionState.Completed || m.Odyssey != odyssey)
                            continue;

                        var mis = evtMissions.FirstOrDefault(x => x.MissionID == m.MissionID);

                        if (mis is null)
                        {
                            missionsToRemove.Add(m);
                        }
                    }

                    if (missionsToRemove.Count == 0)
                        break;

                    foreach (var ms in missionsToRemove)
                    {
                        missions.Remove(ms);
                    }
                    if (IsLive)
                        OnMissionsUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    break;
                case DockedEvent.DockedEventArgs docked:
                    CurrentStationName = string.IsNullOrEmpty(docked.StationName_Localised) ? docked.StationName : docked.StationName_Localised;
                    CurrentSystemAddress = docked.SystemAddress;
                    CurrentSystemName = docked.StarSystem;
                    CurrentMarketID = docked.MarketID;
                    break;
                case UndockedEvent.UndockedEventArgs undocked:
                    CurrentStationName = string.Empty;
                    CurrentMarketID = 0;
                    break;
                case CargoDepotEvent.CargoDepotEventArgs cargoDepot:
                    var kMission = missions.FirstOrDefault(x => x.MissionID == cargoDepot.MissionID);

                    if (kMission is null)
                        break;

                    kMission.ItemsCollected = cargoDepot.ItemsCollected;
                    kMission.ItemsDelivered = cargoDepot.ItemsDelivered;

                    if(IsLive)
                        OnCargoDepot?.Invoke(this, kMission);
                    break;

            }
        }
        public override void ClearData()
        {
            missions.Clear();
            IsLive = false;
        }

        public override void Dispose()
        {
            this.journalManager.UnregisterLogProcessor(this);
        }

        private bool ValidMission(int? count, string missionName)
        {
            if (count is null || count == 0)
            {
                return false;
            }

            for (int i = 0; i < validMissionNames.Length; i++)
            {
                if (missionName.StartsWith(validMissionNames[i]))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
