using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Models.Mining;
using ODEliteTracker.Models.Ship;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class MiningDataStore : LogProcessorBase
    {
        public MiningDataStore(IManageJournalEvents journalManager, SharedDataStore sharedData)
        {
            this.journalManager = journalManager;
            this.sharedData = sharedData;

            this.journalManager.RegisterLogProcessor(this);
            this.sharedData.ShipCargoUpdatedEvent += OnShipCargoUpdated;
        }

        private readonly IManageJournalEvents journalManager;
        private readonly SharedDataStore sharedData;

        private MiningSession? currentSession;
        private readonly List<MiningSession> sessions = [];
        private MiningProspector? latestProspector;
        internal MiningSession? CurrentSession => currentSession;
        internal List<MiningSession> Sessions => sessions;
        internal MiningProspector? LatestProspector => latestProspector;

        internal EventHandler? CurrentSessionUpdated;
        internal EventHandler? SessionsUpdated; 
        internal EventHandler? LatestProspectorUpdated; 

        public override string StoreName => "Mining";
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.Music, true },
                { JournalTypeEnum.Fileheader, true },
                { JournalTypeEnum.Location, true },
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.SupercruiseEntry, true},
                { JournalTypeEnum.SupercruiseExit, true},
                { JournalTypeEnum.Shutdown, true},

                { JournalTypeEnum.AsteroidCracked, true},
                { JournalTypeEnum.LaunchDrone, true},
                { JournalTypeEnum.MaterialCollected, true},
                { JournalTypeEnum.MiningRefined, true},
                { JournalTypeEnum.ProspectedAsteroid, true},
            };
        }

        public override void ClearData()
        {
            currentSession = null;
            sessions.Clear();
            latestProspector = null;
            IsLive = false;
        }

        public override void Dispose()
        {
            this.journalManager.UnregisterLogProcessor(this);
            this.sharedData.ShipCargoUpdatedEvent -= OnShipCargoUpdated;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case MusicEvent.MusicEventArgs music:
                    if (string.Equals(music.MusicTrack, "MainMenu"))
                    {
                        CheckSession(evt.TimeStamp);
                    }
                    break;
                case FileheaderEvent.FileheaderEventArgs:
                case LocationEvent.LocationEventArgs:
                case FSDJumpEvent.FSDJumpEventArgs:
                case SupercruiseEntryEvent.SupercruiseEntryEventArgs:
                case ShutdownEvent.ShutdownEventArgs:
                    CheckSession(evt.TimeStamp);
                    break;
                case SupercruiseExitEvent.SupercruiseExitEventArgs scExit:
                    if (scExit.BodyType == BodyType.PlanetaryRing || scExit.BodyType == BodyType.AsteroidCluster)
                    {

                        currentSession = new MiningSession(scExit.StarSystem, scExit.Body, scExit.SystemAddress, scExit.BodyID);
                        TriggerCurrentSessionEvent();
                    }
                    break;
                case AsteroidCrackedEvent.AsteroidCrackedEventArgs cracked:
                    if (currentSession == null)
                        break;

                    currentSession.CheckStartTime(cracked.Timestamp);
                    currentSession.AsteroidsCracked++;
                    TriggerCurrentSessionEvent();
                    break;
                case LaunchDroneEvent.LaunchDroneEventArgs launchDrone:
                    if (currentSession == null)
                        break;

                    switch (launchDrone.Type)
                    {
                        case DroneType.Collection:
                            currentSession.CollectorsDeployed++;
                            break;
                        case DroneType.Prospector:
                            currentSession.ProspectorsFired++;
                            break;
                        default:
                            break;
                    }
                    currentSession?.CheckStartTime(launchDrone.Timestamp);
                    TriggerCurrentSessionEvent();
                    break;
                case MaterialCollectedEvent.MaterialCollectedEventArgs materialCollected:                   
                    currentSession?.AddMaterial(materialCollected);
                    TriggerCurrentSessionEvent();
                    break;
                case MiningRefinedEvent.MiningRefinedEventArgs miningRefined:
                    currentSession?.CheckStartTime(miningRefined.Timestamp);
                    currentSession?.AddOre(miningRefined);
                    TriggerCurrentSessionEvent();
                    break;
                case ProspectedAsteroidEvent.ProspectedAsteroidEventArgs prospectedAsteroid:
                    currentSession?.CheckStartTime(prospectedAsteroid.Timestamp);
                    currentSession?.AddAsteroid(prospectedAsteroid);

                    if(latestProspector != null)
                    {
                        currentSession?.AddProspector(latestProspector);
                    }
                    latestProspector = new([.. prospectedAsteroid.Materials.Select(x => new MiningMaterial(x.Name, x.Proportion))]
                                    , StringToContent(prospectedAsteroid.Content)
                                    , prospectedAsteroid.MotherlodeMaterial
                                    , prospectedAsteroid.Remaining);

                    TriggerCurrentSessionEvent();
                    TiggerProspectorUpdateEvent();
                    break;

            }
        }

        private static MiningContent StringToContent(string content)
        {
            if (string.Equals(content, "$AsteroidMaterialContent_High;"))
            {
                return MiningContent.High;
            }

            if (string.Equals(content, "$AsteroidMaterialContent_Medium;"))
            {
                return MiningContent.Medium;
            }

            return MiningContent.Low;
        }

        private void TiggerProspectorUpdateEvent()
        {
            if (IsLive == false)
                return;

            LatestProspectorUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void TriggerCurrentSessionEvent()
        {
            if (IsLive == false || currentSession == null)
            {
                return;                
            }

            CurrentSessionUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void CheckSession(DateTime time)
        {
            if (currentSession == null)
            {
                return;
            }

            if (currentSession.HasData == false)
            {
                currentSession = null;
                latestProspector = null;
                if (IsLive)
                {
                    LatestProspectorUpdated?.Invoke(this, EventArgs.Empty);
                    CurrentSessionUpdated?.Invoke(this, EventArgs.Empty);
                }
                return;
            }
            currentSession.TimeFinished = time;
            if (latestProspector != null)
            {
                currentSession.AddProspector(latestProspector);
            }
            var clone = currentSession.Clone();

            if (clone == null)
            {
                return;
            }

            sessions.Add(clone);
            currentSession = null;
            latestProspector = null;
            if (IsLive)
            {
                LatestProspectorUpdated?.Invoke(this, EventArgs.Empty);
                CurrentSessionUpdated?.Invoke(this, EventArgs.Empty);
                SessionsUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Event Listeners
        private void OnShipCargoUpdated(object? sender, IEnumerable<ShipCargo>? e)
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
