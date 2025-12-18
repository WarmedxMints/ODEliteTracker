using EliteJournalReader;
using EliteJournalReader.Events;
using NetTopologySuite.Geometries;
using ODEliteTracker.Database;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models.BGS;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Missions;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Helpers;
using System.Windows.Navigation;

namespace ODEliteTracker.Stores
{
    public sealed class BGSDataStore : LogProcessorBase
    {
        public BGSDataStore(IManageJournalEvents journalManager,
            IODDatabaseProvider databaseProvider,
            TickDataStore tickData,
            SharedDataStore sharedData)
        {
            this.journalManager = journalManager;
            this.tickDataStore = tickData;
            this.sharedData = sharedData;
            this.tickDataStore.NewTick += OnNewTick;
            this.tickContainer = new(tickDataStore.BGSTickData);
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
            lastThursday = EliteHelpers.PreviousThursday();

            this.journalManager.RegisterLogProcessor(this);
        }

        private IManageJournalEvents journalManager;
        private readonly TickDataStore tickDataStore;
        private readonly SharedDataStore sharedData;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private long CurrentSystemAddress;
        private string CurrentSystemName = "Unknown";
        private ulong CurrentMarketID;
        private string CurrentStationName = "Unknown";
        private string? currentSuperCruiseDestination;
        private bool odyssey;
        private DateTime lastThursday;

        private TickContainer tickContainer;
        private readonly List<BGSMission> missions = [];
        private readonly Dictionary<long, BGSStarSystem> systems = [];
        private readonly List<MegaShipScan> megaShipScans = [];
        private readonly Dictionary<string, string> shipTargets = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<BGSIgnoredSystems> bGSIgnoredSystems = [];
        private TickData? selectedTick;
        private BGSStarSystem? currentSystem;
        private Station? currentStation;
        private SystemWarManager czManager = new();
        private SettlementActivity settlementActivity = new();
        public BGSStarSystem? CurrentSystem => currentSystem;
        public List<BGSIgnoredSystems> BGSIgnoredSystems => bGSIgnoredSystems;
        public TickData? SelectedTick => selectedTick;
        public List<BGSTickData> TickData => tickContainer.TickData;
        public IEnumerable<BGSStarSystem> Systems => systems.Values;
        public IEnumerable<MegaShipScan> MegaShipScans => megaShipScans;
        public IEnumerable<BGSMission> Missions => missions;
        internal SettlementActivity SettlementActivity => settlementActivity;
        #region Events
        public EventHandler<BGSMission>? MissionAddedEvent;
        public EventHandler<BGSMission>? MissionUpdatedEvent;
        public EventHandler<BGSMission>? CargoDepotEvent;
        public EventHandler? MissionsUpdatedEvent;
        public EventHandler<BGSStarSystem>? VouchersClaimedEvent;
        public EventHandler<Station?>? CurrentStationUpdated;
        public EventHandler<BGSStarSystem>? SystemUpdated;
        public EventHandler<BGSStarSystem>? SystemAdded;
        public EventHandler? MegaShipScansUpdated;
        public EventHandler? OnNewTickDetected;
        internal event EventHandler<SettlementActivity>? SettlementActivityUpdated
        {
            add { settlementActivity.ActivityUpdated += value; }
            remove { settlementActivity.ActivityUpdated -= value; }
        }
        #endregion

        #region LogProcessor Implementation
        public override string StoreName => "BGS Data";
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.LoadGame,true},
                { JournalTypeEnum.Location,true},
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.CarrierJump, true},
                { JournalTypeEnum.MissionAbandoned, true},
                { JournalTypeEnum.MissionAccepted, true},
                { JournalTypeEnum.MissionCompleted, true},
                { JournalTypeEnum.MissionFailed, true},
                { JournalTypeEnum.MissionRedirected, true},
                { JournalTypeEnum.Missions, true},
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.Undocked, true},
                { JournalTypeEnum.FactionKillBond, true},
                { JournalTypeEnum.RedeemVoucher, true},
                { JournalTypeEnum.MarketBuy, true},
                { JournalTypeEnum.MarketSell, true},
                { JournalTypeEnum.CommitCrime, true},
                { JournalTypeEnum.SellExplorationData, true},
                { JournalTypeEnum.MultiSellExplorationData, true},
                { JournalTypeEnum.SearchAndRescue, true},
                { JournalTypeEnum.SupercruiseDestinationDrop, true},
                { JournalTypeEnum.SupercruiseEntry, true},
                { JournalTypeEnum.ShipTargeted, true},
                { JournalTypeEnum.DataScanned, true},
                { JournalTypeEnum.Shutdown, true},
                { JournalTypeEnum.Music, true},
                { JournalTypeEnum.Fileheader, true},
                { JournalTypeEnum.ApproachSettlement, true},
            };
        }
        public override void ParseJournalEvent(ODJournalDatabase.JournalManagement.JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case FileheaderEvent.FileheaderEventArgs file:
                    CheckForCZ(file.Timestamp);
                    break;
                case LoadGameEvent.LoadGameEventArgs load:
                    odyssey = load.Odyssey;
                    settlementActivity.Reset();
                    CheckForCZ(load.Timestamp);
                    if (IsLive)
                        UpdatePreviousThursday();
                    break;
                case LocationEvent.LocationEventArgs location:
                    CurrentSystemAddress = location.SystemAddress;
                    CurrentSystemName = location.StarSystem;

                    if (string.IsNullOrEmpty(location.StationName) == false)
                    {
                        CurrentStationName = location.StationName;
                        CurrentMarketID = location.MarketID;
                    }

                    //Ignore systems with less than 2 factions
                    if (location.Factions is null || location.Factions.Count < 2)
                    {
                        currentSystem = null;
                        break;
                    }

                    var system = new BGSStarSystem(location);
                    TryAddSystem(system, location.Timestamp);

                    if (string.IsNullOrEmpty(location.StationName)
                        || location.StationFaction is null
                        || !sharedData.Factions.TryGetValue(location.StationFaction.Name, out var faction))
                    {
                        break;
                    }

                    currentStation = new Station(location, faction, system);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    shipTargets.Clear();
                    settlementActivity.Reset();
                    CurrentSystemAddress = fsdJump.SystemAddress;
                    CurrentSystemName = fsdJump.StarSystem;
                    CheckForCZ(fsdJump.Timestamp);
                    //Ignore systems with less than 2 factions
                    if (fsdJump.Factions is null || fsdJump.Factions.Count < 2)
                    {
                        currentSystem = null;
                        break;
                    }
                    var nSystem = new BGSStarSystem(fsdJump);

                    TryAddSystem(nSystem, fsdJump.Timestamp);
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
                    CurrentSystemAddress = carrierJump.SystemAddress;
                    CurrentSystemName = carrierJump.StarSystem;

                    if (string.IsNullOrEmpty(carrierJump.StationName) == false)
                    {
                        CurrentStationName = carrierJump.StationName;
                        CurrentMarketID = carrierJump.MarketID;
                    }

                    //Ignore systems with less than 2 factions
                    if (carrierJump.Factions is null || carrierJump.Factions.Count < 2)
                    {
                        currentSystem = null;
                        break;
                    }

                    var cSystem = new BGSStarSystem(carrierJump);
                    TryAddSystem(cSystem, carrierJump.Timestamp);

                    if (string.IsNullOrEmpty(carrierJump.StationName)
                        || carrierJump.StationFaction is null
                        || !sharedData.Factions.TryGetValue(carrierJump.StationFaction.Name, out var fctn))
                    {
                        break;
                    }

                    currentStation = new Station(carrierJump, fctn, cSystem);
                    break;
                case MissionAcceptedEvent.MissionAcceptedEventArgs accepted:
                    if (string.IsNullOrEmpty(CurrentStationName))
                    {
                        break;
                    }

                    var mission = new BGSMission(accepted,
                                                      CurrentSystemAddress,
                                                      CurrentSystemName,
                                                      CurrentMarketID,
                                                      CurrentStationName,
                                                      odyssey);

                    missions.Add(mission);
                    UpdateMissionIfLive(mission);
                    break;
                case MissionRedirectedEvent.MissionRedirectedEventArgs redirected:
                    var rMission = missions.FirstOrDefault(x => x.MissionID == redirected.MissionID);
                    if (rMission != null)
                    {
                        rMission.CurrentState = MissionState.Redirected;
                        UpdateMissionIfLive(rMission);
                    }
                    break;
                case MissionCompletedEvent.MissionCompletedEventArgs completed:
                    var cMission = missions.FirstOrDefault(x => x.MissionID == completed.MissionID);
                    if (cMission != null)
                    {
                        cMission.CurrentState = MissionState.Completed;
                        cMission.CompletionTime = completed.Timestamp;
                        cMission.Reward = completed.Reward == 0 ? -completed.Donated : completed.Reward;
                        cMission.ApplyFactionEffects(completed.FactionEffects);

                        UpdateMissionIfLive(cMission);
                    }
                    break;
                case MissionFailedEvent.MissionFailedEventArgs missionFailed:
                    var fMission = missions.FirstOrDefault(x => x.MissionID == missionFailed.MissionID);
                    if (fMission != null)
                    {
                        fMission.CurrentState = MissionState.Failed;
                        fMission.CompletionTime = missionFailed.Timestamp;
                        fMission.Reward = 0;
                        UpdateMissionIfLive(fMission);
                    }
                    break;
                case MissionAbandonedEvent.MissionAbandonedEventArgs abandoned:
                    var aMission = missions.FirstOrDefault(x => x.MissionID == abandoned.MissionID);
                    if (aMission != null)
                    {
                        aMission.CurrentState = MissionState.Abandoned;
                        aMission.CompletionTime = abandoned.Timestamp;
                        aMission.Reward = 0;
                        UpdateMissionIfLive(aMission);
                    }
                    break;
                case MissionsEvent.MissionsEventArgs missionsEvt:
                    var evtMissions = new List<Mission>();

                    evtMissions.AddRange(missionsEvt.Active);
                    evtMissions.AddRange(missionsEvt.Complete);
                    evtMissions.AddRange(missionsEvt.Failed);

                    var missionsToRemove = missions.Where(x => x.CurrentState <= MissionState.Redirected).ToList();

                    if (missionsToRemove.Count == 0)
                        break;

                    foreach (var m in missionsToRemove)
                    {
                        var mis = evtMissions.Count == 0 ? null : evtMissions.FirstOrDefault(x => x.MissionID == m.MissionID);

                        if (mis == null)
                        {
                            missions.Remove(m);
                        }
                    }

                    if (IsLive)
                        MissionsUpdatedEvent?.Invoke(this, EventArgs.Empty);
                    break;
                case DockedEvent.DockedEventArgs docked:
                    CurrentStationName = string.IsNullOrEmpty(docked.StationName_Localised) ? docked.StationName : docked.StationName_Localised;
                    CurrentSystemAddress = docked.SystemAddress;
                    CurrentSystemName = docked.StarSystem;
                    CurrentMarketID = docked.MarketID;
                    CheckForNewTick();

                    if (string.IsNullOrEmpty(docked.StationName)
                        || docked.StationFaction is null
                        || !sharedData.Factions.TryGetValue(docked.StationFaction.Name, out var statinFaction))
                    {
                        break;
                    }
                    if (currentSystem != null)
                    {
                        currentStation = new Station(docked, statinFaction, currentSystem);
                        UpdateStationIfLive(currentStation);
                    }
                    break;
                case UndockedEvent.UndockedEventArgs undocked:
                    CurrentStationName = string.Empty;
                    CurrentMarketID = 0;
                    currentStation = null;
                    UpdateStationIfLive(currentStation);
                    break;
                case FactionKillBondEvent.FactionKillBondEventArgs factionKillBond:
                    czManager.OnEarnedBonds(factionKillBond.Reward, factionKillBond.AwardingFaction);
                    settlementActivity.OnFactionKillBond(factionKillBond);
                    break;
                case RedeemVoucherEvent.RedeemVoucherEventArgs redeemVoucher:
                    if (currentSystem == null)
                        break;
                    if (redeemVoucher.Factions == null || redeemVoucher.Factions.Count == 0)
                    {
                        if (string.IsNullOrEmpty(redeemVoucher.Faction))
                            break;

                        currentSystem
                            .VoucherClaims
                            .Add(new VoucherClaim(redeemVoucher.Type, redeemVoucher.Faction, redeemVoucher.Amount, redeemVoucher.Timestamp));

                        if (IsLive)
                            VouchersClaimedEvent?.Invoke(this, currentSystem);
                        break;
                    }

                    foreach (var fct1 in redeemVoucher.Factions)
                    {
                        if (string.IsNullOrEmpty(fct1.Faction))
                            continue;
                        currentSystem
                            .VoucherClaims
                            .Add(new VoucherClaim(redeemVoucher.Type, fct1, redeemVoucher.Timestamp));
                    }

                    if (IsLive)
                        VouchersClaimedEvent?.Invoke(this, currentSystem);
                    break;
                case MarketBuyEvent.MarketBuyEventArgs marketBuy:
                    if (currentSystem == null || currentStation == null || currentStation.MarketID != marketBuy.MarketID)
                        break;

                    currentSystem.Transactions.Add(new(marketBuy, currentStation.StationFaction));
                    UpdateSystemIfLive(currentSystem);
                    break;
                case MarketSellEvent.MarketSellEventArgs marketSell:
                    if (currentSystem == null || currentStation == null || currentStation.MarketID != marketSell.MarketID)
                        break;

                    currentSystem.Transactions.Add(new(marketSell, currentStation.StationFaction));
                    UpdateSystemIfLive(currentSystem);
                    break;
                case CommitCrimeEvent.CommitCrimeEventArgs commitCrime:
                    if (currentSystem == null || commitCrime.CrimeType.Contains("murder") == false)
                        return;

                    var victimFaction = GetVictimFaction(commitCrime);
                    if (sharedData.Factions.TryGetValue(victimFaction, out var value))
                    {
                        var crime = new SystemCrime(commitCrime, value);

                        currentSystem.Crimes.Add(crime);
                        UpdateSystemIfLive(currentSystem);
                    }
                    settlementActivity.OnCommitCrime(commitCrime);
                    break;
                case SellExplorationDataEvent.SellExplorationDataEventArgs sellData:
                    if (currentSystem == null || currentStation == null)
                        return;

                    if (sharedData.Factions.TryGetValue(currentStation.StationFaction.Name, out var fct))
                    {

                        currentSystem.CartoData.Add(new(sellData, fct));
                        UpdateSystemIfLive(currentSystem);
                    }
                    break;
                case MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs m_sellData:
                    if (currentSystem == null || currentStation == null)
                        return;

                    if (sharedData.Factions.TryGetValue(currentStation.StationFaction.Name, out var factn))
                    {

                        currentSystem.CartoData.Add(new(m_sellData, factn));
                        UpdateSystemIfLive(currentSystem);
                    }
                    break;
                case SearchAndRescueEvent.SearchAndRescueEventArgs searchAndRescueData:
                    if (currentSystem == null || currentStation == null || currentStation.MarketID != searchAndRescueData.MarketID)
                        return;

                    if (sharedData.Factions.TryGetValue(currentStation.StationFaction.Name, out var _factn))
                    {
                        currentSystem.SearchAndRescueData.Add(new(searchAndRescueData, _factn));
                        UpdateSystemIfLive(currentSystem);
                    }
                    break;
                case SupercruiseDestinationDropEvent.SupercruiseDestinationDropEventArgs scDestDrop:
                    currentSuperCruiseDestination = scDestDrop.Type;
                    shipTargets.Clear();
                    czManager.OnDestinationDrop(scDestDrop.Type);
                    break;
                case SupercruiseEntryEvent.SupercruiseEntryEventArgs scEntry:
                    currentSuperCruiseDestination = null;
                    shipTargets.Clear();
                    settlementActivity.Reset();
                    CheckForCZ(scEntry.Timestamp);
                    break;
                case ShipTargetedEvent.ShipTargetedEventArgs shipTargetedData:
                    if (string.IsNullOrEmpty(shipTargetedData.Faction))
                        break;

                    var name = shipTargetedData.PilotName_Localised ?? shipTargetedData.PilotName;

                    if (shipTargets.ContainsKey(name))
                    {
                        shipTargets[name] = shipTargetedData.Faction;
                        return;
                    }
                    shipTargets.TryAdd(shipTargetedData.PilotName_Localised ?? shipTargetedData.PilotName, shipTargetedData.Faction);
                    break;
                case MusicEvent.MusicEventArgs music:
                    if (string.Equals(music.MusicTrack, "MainMenu") == false)
                    {
                        break;
                    }
                    CheckForCZ(music.Timestamp);
                    break;
                case ShutdownEvent.ShutdownEventArgs shutdown:
                    CheckForCZ(shutdown.Timestamp);
                    break;
                case ApproachSettlementEvent.ApproachSettlementEventArgs approachSettlement:
                    czManager.OnApproachSettlement(approachSettlement.Name);
                    settlementActivity.OnApproachSettlement(approachSettlement);
                    break;
                case DataScannedEvent.DataScannedEventArgs scannedData:
                    if (currentSystem is null || string.IsNullOrEmpty(currentSuperCruiseDestination))
                        break;

                    if (string.Equals(scannedData.Type, "$Datascan_ShipUplink;") && scannedData.Timestamp >= lastThursday)
                    {
                        var known = megaShipScans.FirstOrDefault(x => string.Equals(x.MegaShipName, currentSuperCruiseDestination)
                                                    && x.SystemAddress == currentSystem.Address);

                        if (known != null)
                        {
                            break;
                        }

                        var megaship = new MegaShipScan(scannedData.Timestamp, currentSystem.Name, currentSystem.Address, currentSuperCruiseDestination);
                        megaShipScans.Add(megaship);

                        if (IsLive)
                            MegaShipScansUpdated?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case DiedEvent.DiedEventArgs died:
                    czManager.Reset();
                    settlementActivity.Reset();
                    break;

            }
        }

        private string GetVictimFaction(CommitCrimeEvent.CommitCrimeEventArgs e)
        {
            if (string.Equals(e.CrimeType, "onFoot_murder"))
            {
                return e.Faction;
            }

            if (shipTargets.TryGetValue(e.Victim, out var value))
            {
                return value;
            }

            if (string.IsNullOrEmpty(e.Faction) == false)
                return e.Faction;
            //Commander didn't get a level 3 scan so we don't know the faction
            return string.Empty;
        }


        private void CheckForCZ(DateTime timestamp)
        {
            if (currentSystem == null)
                return;

            if (czManager.HasConflict())
            {
                currentSystem.AddWar(czManager.GetConflictZone(timestamp));
                UpdateSystemIfLive(currentSystem);
                return;
            }
            czManager.Reset();
        }

        private void UpdatePreviousThursday()
        {
            var previous = EliteHelpers.PreviousThursday();

            if (previous == lastThursday)
                return;

            lastThursday = previous;

            var outdatedMegaships = megaShipScans.Where(x => x.ScanDate < lastThursday);

            if (outdatedMegaships.Any() == false)
                return;

            foreach (var ms in outdatedMegaships)
            {
                megaShipScans.Remove(ms);
            }

            MegaShipScansUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateMissionIfLive(BGSMission mission)
        {
            if (IsLive)
                MissionAddedEvent?.Invoke(this, mission);
        }

        private void TryAddSystem(BGSStarSystem system, DateTime eventTime)
        {

            if (systems.TryGetValue(system.Address, out var bgsSystem))
            {
                bgsSystem.AddTickData(system, eventTime);
                currentSystem = bgsSystem;
                UpdateSystemIfLive(currentSystem);
                return;
            }

            if (systems.TryAdd(system.Address, system))
            {
                currentSystem = system;
                if (IsLive)
                {
                    SystemAdded?.Invoke(this, system);
                }
            }
        }

        private void UpdateSystemIfLive(BGSStarSystem system)
        {
            if (IsLive)
            {
                SystemUpdated?.Invoke(this, system);
            }
        }

        private void UpdateStationIfLive(Station? currentStation)
        {
            if (IsLive)
            {
                CurrentStationUpdated?.Invoke(this, currentStation);
            }
        }

        public override void RunBeforeParsingHistory(int currentCmdrId)
        {
            var task = Task.Run(async () => { await tickDataStore.UpdateTickFromDatabase(); });
            task.Wait();
            var systems = databaseProvider.GetIgnoredSystems();
            bGSIgnoredSystems.Clear();
            if (systems.Count > 0)
            {

                foreach (var system in systems)
                {
                    bGSIgnoredSystems.Add(system);
                }
            }
            this.tickContainer.UpdateTickData(tickDataStore.BGSTickData);
        }

        public override void RunAfterParsingHistory()
        {
            base.RunAfterParsingHistory();
            CheckForNewTick();
        }

        public override void ClearData()
        {
            missions.Clear();
            systems.Clear();
            IsLive = false;
            megaShipScans.Clear();
            shipTargets.Clear();
            bGSIgnoredSystems.Clear();
        }
        public override void Dispose()
        {
            this.journalManager.UnregisterLogProcessor(this);
        }

        public override void SaveData()
        {
            databaseProvider.SetIgnoredBGSSystems(bGSIgnoredSystems);
        }
        #endregion

        private void OnNewTick(object? sender, EventArgs e)
        {
            tickContainer = new(tickDataStore.BGSTickData);
            OnNewTickDetected?.Invoke(this, EventArgs.Empty);
        }

        public Tuple<IEnumerable<BGSTickSystem>, IEnumerable<BGSMission>> GetTickInfo(string? id)
        {
            if (id == null)
                return Tuple.Create<IEnumerable<BGSTickSystem>, IEnumerable<BGSMission>>([], []);

            selectedTick = tickContainer.GetTickFromTo(id);

            var missions = this.missions.Where(x => selectedTick.TimeWithinTick(x.CompletionTime)
                                            && (x.CurrentState == MissionState.Completed || x.CurrentState == MissionState.Failed)).ToList();
            var systems = new List<BGSTickSystem>();

            foreach (var system in this.systems.Values)
            {
                var validSystem = system.GetBGSTickSystem(selectedTick);

                if (validSystem != null)
                {
                    systems.Add(validSystem);
                }
            }

            return Tuple.Create<IEnumerable<BGSTickSystem>, IEnumerable<BGSMission>>(systems, missions);
        }

        internal async Task<BGSTickData> AddTick(DateTime dateTime)
        {
            var tick = await tickDataStore.AddTick(dateTime);
            tickContainer.UpdateTickData(tickDataStore.BGSTickData);
            return tick;
        }

        internal async Task DeleteTick(string iD)
        {
            await tickDataStore.DeleteTick(iD);
            tickContainer.UpdateTickData(tickDataStore.BGSTickData);
        }

        public void CheckForNewTick()
        {
            if (IsLive == false)
            {
                return;
            }
            _ = Task.Factory.StartNew(() => tickDataStore.CheckForNewTick());
        }

        internal void AddIgnoredSystem(long address, string name, bool add)
        {
            var known = bGSIgnoredSystems.FirstOrDefault(x => x.Address == address);
            if (add)
            {
                if (known != null)
                    return;

                bGSIgnoredSystems.Add(new(address, name));
                return;
            }

            if (known != null)
            {
                bGSIgnoredSystems.Remove(known);
            }
        }
    }
}
