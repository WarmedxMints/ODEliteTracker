using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Models.Missions;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class MassacreMissionStore : LogProcessorBase
    {
        public MassacreMissionStore(IManageJournalEvents journalManager)
        {
            this.journalManager = journalManager;
            this.journalManager.RegisterLogProcessor(this);
        }

        private IManageJournalEvents journalManager;
        private readonly List<MassacreMission> missions = [];

        private long CurrentSystemAddress;
        private string CurrentSystemName = "Unknown";
        private ulong CurrentMarketID;
        private string CurrentStationName = "Unknown";
        private bool odyssey;
        public override string StoreName => "Massacre Mission";
        public List<MassacreMission> Missions { get { return missions; } }
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.LoadGame,true},
                { JournalTypeEnum.Location,true},
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.MissionAbandoned, true},
                { JournalTypeEnum.MissionAccepted, true},
                { JournalTypeEnum.MissionCompleted, false},
                { JournalTypeEnum.MissionFailed, false},
                { JournalTypeEnum.MissionRedirected, true},
                { JournalTypeEnum.Missions, false},
                { JournalTypeEnum.Bounty, true},
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.Undocked, true},
            };
        }

        public EventHandler<MassacreMission>? MissionAddedEvent;
        public EventHandler<MassacreMission>? MissionUpdatedEvent;
        public EventHandler? MissionsUpdatedEvent;

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
                       || accepted.KillCount is null
                       || accepted.KillCount == 0
                       || string.IsNullOrEmpty(accepted.TargetFaction)
                       || accepted.TargetType == null
                       || !accepted.TargetType.Contains("MissionUtil_FactionTag_Pirate", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    var mission = new MassacreMission(accepted,
                                                      CurrentSystemAddress,
                                                      CurrentSystemName,
                                                      CurrentMarketID,
                                                      CurrentStationName,
                                                      odyssey);

                    missions.Add(mission);
                    if (IsLive)
                        MissionAddedEvent?.Invoke(this, mission);
                    break;
                case MissionRedirectedEvent.MissionRedirectedEventArgs redirected:
                    var rMission = missions.FirstOrDefault(x => x.MissionID == redirected.MissionID);
                    if(rMission != null)
                    {
                        rMission.CurrentState = MissionState.Redirected;
                        rMission.Kills = rMission.KillCount;
                        if (IsLive)
                            MissionUpdatedEvent?.Invoke(this, rMission);
                    }
                    break;
                case MissionCompletedEvent.MissionCompletedEventArgs completed:
                    var cMission = missions.FirstOrDefault(x => x.MissionID == completed.MissionID);
                    if (cMission != null)
                    {
                        cMission.CurrentState = MissionState.Completed;
                        cMission.CompletionTime = completed.Timestamp;
                        cMission.Reward = completed.Reward;
                        cMission.Kills = cMission.KillCount;
                        if (IsLive)
                            MissionUpdatedEvent?.Invoke(this, cMission);
                    }
                    break;
                case MissionFailedEvent.MissionFailedEventArgs missionFailed:
                    var fMission = missions.FirstOrDefault(x => x.MissionID == missionFailed.MissionID);
                    if (fMission != null)
                    {
                        fMission.CurrentState = MissionState.Failed;
                        fMission.Reward = 0;
                        if (IsLive)
                            MissionUpdatedEvent?.Invoke(this, fMission);
                    }
                    break;
                case MissionAbandonedEvent.MissionAbandonedEventArgs abandoned:
                    var aMission = missions.FirstOrDefault(x => x.MissionID == abandoned.MissionID);
                    if (aMission != null)
                    {
                        aMission.CurrentState = MissionState.Abandoned;
                        aMission.Reward = 0;
                        if(IsLive)
                            MissionUpdatedEvent?.Invoke(this, aMission);
                    }
                    break;
                case MissionsEvent.MissionsEventArgs missionsEvt:
                    var evtMissions = new List<Mission>();

                    evtMissions.AddRange(missionsEvt.Active);
                    evtMissions.AddRange(missionsEvt.Complete);
                    evtMissions.AddRange(missionsEvt.Failed);

                    if (evtMissions.Count == 0)
                        break;
                    var missionsToRemove = new List<MassacreMission>();

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
                        MissionsUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    break;
                case BountyEvent.BountyEventArgs bountyEvt:
                    if(bountyEvt.VictimFaction.Contains("faction_none")
                       || bountyEvt.VictimFaction.Contains("faction_Pirate")
                       || bountyEvt.Target.Contains("suit")
                       || bountyEvt.TotalReward <= 0)
                    {
                        break;
                    }
                    var factionMissions = missions.Where(x => string.Equals(x.TargetFaction, bountyEvt.VictimFaction)).GroupBy(x => x.IssuingFaction);

                    foreach (var factionMission in factionMissions)
                    {
                        var activeMission = factionMission.FirstOrDefault(x => x.CurrentState == MissionState.Active);

                        if (activeMission != default)
                        {
                            activeMission.Kills++;
                            if (IsLive)
                                MissionUpdatedEvent?.Invoke(this, activeMission);
                        }
                    }
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
    }
}
