using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json.Linq;
using ODEliteTracker.Models.PowerPlay;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Helpers;

namespace ODEliteTracker.Stores
{
    public sealed class PowerPlayDataStore : LogProcessorBase
    {
        public PowerPlayDataStore(IManageJournalEvents journalManager)
        {
            this.journalManager = journalManager;
            this.journalManager.RegisterLogProcessor(this);

            previousCycle = EliteHelpers.PreviousThursday(1);
            currentCycle = EliteHelpers.PreviousThursday();
            nextCycle = EliteHelpers.NextThursday();
        }


        private readonly IManageJournalEvents journalManager;
        private DateTime previousCycle;
        private DateTime currentCycle;
        private DateTime nextCycle;
        private DateTime? lastActivity;
        private Dictionary<string, int> activities = [];
        private bool inPowerplayCZ = false;
        private long currentSystemAddress;
        private bool odyssey;
        private int storedMerits;
        private Dictionary<long, PowerPlaySystem> systems = [];
        public override string StoreName => "PowerPlay";
        public PowerPlaySystem? CurrentSystem => systems.Values.FirstOrDefault(x => x.Address == currentSystemAddress);
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.LoadGame,true},
                { JournalTypeEnum.Location,true},
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.CarrierJump, true},
                { JournalTypeEnum.Powerplay,true},
                { JournalTypeEnum.PowerplayMerits,true },
                { JournalTypeEnum.PowerplayCollect,true },
                { JournalTypeEnum.PowerplayDeliver,true },
                { JournalTypeEnum.PowerplayRank,true },
                { JournalTypeEnum.ShipTargeted,true },
                { JournalTypeEnum.SupercruiseDestinationDrop, true },
                { JournalTypeEnum.SupercruiseEntry, true },
                { JournalTypeEnum.Bounty, true },
                { JournalTypeEnum.FactionKillBond, true },
                { JournalTypeEnum.MissionCompleted,true },
                { JournalTypeEnum.DatalinkScan,true },
                { JournalTypeEnum.SearchAndRescue, true },
                { JournalTypeEnum.MarketSell, true },
                { JournalTypeEnum.SellExplorationData, true },
                { JournalTypeEnum.MultiSellExplorationData, true },
                { JournalTypeEnum.SellOrganicData, true },
                { JournalTypeEnum.HoloscreenHacked, true },
                { JournalTypeEnum.Died, true },
            };
        }
        public DateTime PreviousCycle => previousCycle;
        public DateTime CurrentCycle => currentCycle;

        public TimeSpan CycleRemaining => nextCycle - DateTime.UtcNow;

        public IEnumerable<PowerPlaySystem> Systems => systems.Values;

        private PledgeData? pledgeData;
        public PledgeData? PledgeData
        {
            get { return pledgeData; }
            set
            {
                pledgeData = value;
                if (IsLive)
                    PledgeDataUpdated?.Invoke(this, pledgeData);
            }
        }

        public EventHandler<PledgeData?>? PledgeDataUpdated;
        public EventHandler<PowerPlaySystem>? SystemUpdated;
        public EventHandler<PowerPlaySystem>? SystemAdded;
        public EventHandler<PowerPlaySystem>? SystemCycleUpdated;
        public EventHandler<int>? MeritsEarned;
        public EventHandler? CyclesUpdated;
        public override DateTime GetJournalAge(DateTime defaultAge)
        {
            return previousCycle;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            //If it has been more than 5 seconds since an activity, reset the current
            if (lastActivity != null && evt.TimeStamp - lastActivity >= TimeSpan.FromSeconds(5))
            {
                activities.Clear();
            }

            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            if (GetVisitedCycle(evt.TimeStamp, out DateTime cycle) == false)
                return;

            switch (evt.EventData)
            {
                case LoadGameEvent.LoadGameEventArgs load:
                    odyssey = load.Odyssey;
                    ClearActivities(cycle);
                    if (IsLive)
                    {
                        var currentCycle = EliteHelpers.PreviousThursday();
                        if (currentCycle == this.currentCycle)
                            break;

                        var systemsToRemove = new List<long>();
                        foreach (var powerSystem in systems)
                        {
                            if (powerSystem.Value.CycleData.ContainsKey(this.currentCycle) == false)
                            {
                                systemsToRemove.Add(powerSystem.Key);
                            }
                        }

                        foreach (var sys in systemsToRemove)
                        {
                            systems.Remove(sys);
                        }

                        previousCycle = EliteHelpers.PreviousThursday(1);
                        this.currentCycle = currentCycle;
                        nextCycle = EliteHelpers.NextThursday();

                        CyclesUpdated?.Invoke(this, EventArgs.Empty);
                    }
                    break;
                case LocationEvent.LocationEventArgs location:
                    ClearActivities(cycle);
                    if (location.Powers is null || location.Powers.Count == 0)
                    {
                        break;
                    }
                    var system = new PowerPlaySystem(location, cycle);
                    Add_UpdateSystem(system, cycle);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    ClearActivities(cycle);
                    if (fsdJump.Powers is null || fsdJump.Powers.Count == 0)
                    {
                        break;
                    }
                    var ppSystem = new PowerPlaySystem(fsdJump, cycle);
                    Add_UpdateSystem(ppSystem, cycle);
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs cJump:
                    ClearActivities(cycle);
                    if (cJump.Powers is null || cJump.Powers.Count == 0)
                    {
                        break;
                    }
                    var pSystem = new PowerPlaySystem(cJump, cycle);
                    Add_UpdateSystem(pSystem, cycle);
                    break;
                case PowerplayEvent.PowerplayEventArgs powerplay:
                    pledgeData ??= new(powerplay);
                    pledgeData.Update(powerplay);
                    
                    if (IsLive)
                        PledgeDataUpdated?.Invoke(this, pledgeData);
                    break;
                case PowerplayRankEvent.PowerplayRankEventArgs rank:
                    if (PledgeData == null)
                        break;
                    PledgeData.Rank = rank.Rank;
                    if (IsLive)
                        PledgeDataUpdated?.Invoke(this, PledgeData);
                    break;
                case PowerplayMeritsEvent.PowerplayMeritsEventArgs merits:
                    if (systems.TryGetValue(currentSystemAddress, out var powerplaySystem))
                    {
                        UpdatePledgeData(merits);

                        if (powerplaySystem.CycleData.TryGetValue(cycle, out var ppData))
                        {
                            AddMerits(ppData, merits.MeritsGained);
                            UpdateSystemIfLive(powerplaySystem);
                            break;
                        }

                        var data = new PowerplayCycleData();

                        AddMerits(data, merits.MeritsGained);

                        if (powerplaySystem.CycleData.TryAdd(cycle, data))
                        {
                            UpdateSystemIfLive(powerplaySystem);
                        }
                    }
                    break;
                case PowerplayCollectEvent.PowerplayCollectEventArgs powerplayCollect:
                    if (systems.TryGetValue(currentSystemAddress, out system))
                    {
                        var itemName = powerplayCollect.GetTypeCollected();

                        if (system.CycleData.TryGetValue(cycle, out var ppData))
                        {
                            if (ppData.GoodsCollected.ContainsKey(itemName))
                            {
                                ppData.GoodsCollected[itemName] += powerplayCollect.Count;
                                UpdateSystemIfLive(system);
                                break;
                            }

                            if (ppData.GoodsCollected.TryAdd(itemName, powerplayCollect.Count))
                            {
                                UpdateSystemIfLive(system);
                            }
                            break;
                        }
                    }
                    break;
                case PowerplayDeliverEvent.PowerplayDeliverEventArgs powerplayDeliver:
                    if (systems.TryGetValue(currentSystemAddress, out system))
                    {
                        SetActivity(cycle, "Powerplay Deliveries", evt.TimeStamp);
                        var itemName = powerplayDeliver.GetTypeCollected();

                        if (system.CycleData.TryGetValue(cycle, out var ppData))
                        {
                            if (ppData.GoodsDelivered.ContainsKey(itemName))
                            {
                                ppData.GoodsDelivered[itemName] += powerplayDeliver.Count;
                                UpdateSystemIfLive(system);
                                break;
                            }

                            if (ppData.GoodsDelivered.TryAdd(itemName, powerplayDeliver.Count))
                            {
                                UpdateSystemIfLive(system);
                            }
                            break;
                        }
                    }
                    break;
                case ShipTargetedEvent.ShipTargetedEventArgs shipTargeted:
                    if (string.IsNullOrEmpty(shipTargeted.LegalStatus) && shipTargeted.LegalStatus == "Enemy")
                    {
                        SetActivity(cycle, "Enemy Kill", evt.TimeStamp);
                        return;
                    }
                    if (shipTargeted.ScanStage > 2)
                        SetActivity(cycle, "Ship Scans", evt.TimeStamp);
                    break;
                case SupercruiseDestinationDropEvent.SupercruiseDestinationDropEventArgs destinationDropEvent:
                    if (destinationDropEvent.Type.StartsWith("$Warzone_Powerplay"))
                    {
                        inPowerplayCZ = true;
                        SetActivity(cycle, "Enemy CZ Kills", evt.TimeStamp);
                    }
                    break;
                case SupercruiseEntryEvent.SupercruiseEntryEventArgs scEntry:
                        ClearActivities(cycle);
                    break;
                case BountyEvent.BountyEventArgs bnty:
                        SetActivity(cycle, "Bounties", evt.TimeStamp, bnty.Rewards.Count);
                    break;
                case FactionKillBondEvent.FactionKillBondEventArgs factionKillBond:
                    if(string.Equals(factionKillBond.AwardingFaction, pledgeData?.Power))
                    {
                        SetActivity(cycle, "Enemy CZ Kills", evt.TimeStamp);
                    }
                    break;
                case MissionCompletedEvent.MissionCompletedEventArgs missionCompleted:
                    if (missionCompleted.Name.Contains("Mission_Altruism"))
                        SetActivity(cycle, "Donations", evt.TimeStamp);
                    break;
                case DatalinkScanEvent.DatalinkScanEventArgs datalink:
                    if(datalink.Message.Contains("$Datascan_ShipUplink;"))
                        SetActivity(cycle, "Data Link Scans", evt.TimeStamp);
                    break;
                case SearchAndRescueEvent.SearchAndRescueEventArgs:
                    SetActivity(cycle, "Search & Rescue", evt.TimeStamp);
                    break;
                case MarketSellEvent.MarketSellEventArgs sell:
                    if (sell.AvgPricePaid == 0)
                    {
                        SetActivity(cycle, "Mined Ore Sales", evt.TimeStamp);
                        break;
                    }

                    if (EliteCommodityHelpers.IsRare(sell.Type))
                    {
                        SetActivity(cycle, "Rare Good Sales", evt.TimeStamp);
                        break;
                    }
                    SetActivity(cycle, "Market Sales", evt.TimeStamp);
                    break;
                case SellExplorationDataEvent.SellExplorationDataEventArgs:
                case MultiSellExplorationDataEvent.MultiSellExplorationDataEventArgs:
                    SetActivity(cycle, "Cartographic Data Sales", evt.TimeStamp);
                    break;
                case SellOrganicDataEvent.SellOrganicDataEventArgs:
                    SetActivity(cycle, "Exobiology Sales", evt.TimeStamp);
                    break;
                case HoloscreenHackedEvent.HoloscreenHackedEventArgs:
                    SetActivity(cycle, "Holoscreen Hacks", evt.TimeStamp);
                    break;
                case DiedEvent.DiedEventArgs:
                    ClearActivities(cycle);
                    break;

            }
        }
        public override void RunAfterParsingHistory()
        {
            base.RunAfterParsingHistory();
        }
        private void UpdatePledgeData(PowerplayMeritsEvent.PowerplayMeritsEventArgs merits)
        {
            if (PledgeData != null)
            {
                PledgeData.Merits = merits.TotalMerits;

                if (merits.Timestamp >= currentCycle)
                    PledgeData.MeritsEarnedThisCycle += merits.MeritsGained;

                if (IsLive)
                    PledgeDataUpdated?.Invoke(this, PledgeData);
            }
        }

        private void UpdateSystemIfLive(PowerPlaySystem system)
        {
            if (IsLive)
                SystemUpdated?.Invoke(this, system);
        }

        private bool GetVisitedCycle(DateTime timeStamp, out DateTime cycle)
        {
            if (timeStamp > previousCycle && timeStamp < currentCycle)
            {
                cycle = previousCycle;
                return true;
            }
            if (timeStamp > currentCycle)
            {
                cycle = currentCycle;
                return true;
            }
            cycle = DateTime.MinValue;
            return false;
        }

        private void Add_UpdateSystem(PowerPlaySystem newSystem, DateTime cycle)
        {
            currentSystemAddress = newSystem.Address;
            if (systems.TryGetValue(newSystem.Address, out var nSystem))
            {
                nSystem.Add_UpdateCycle(cycle, newSystem);
                if (IsLive)
                    SystemCycleUpdated?.Invoke(this, newSystem);
                return;
            }
            if (systems.TryAdd(newSystem.Address, newSystem))
            {
                if (IsLive)
                    SystemAdded?.Invoke(this, newSystem);
                return;
            }
        }

        private void AddActivity(string activity, int count)
        {
            if (activities.TryGetValue(activity, out int value))
            {
                activities[activity]+= count;
                return;
            }

            activities.Add(activity, count);
        }
        private void SetActivity(DateTime cycle, string activity, DateTime eventTime, int count = 1)
        {

            AddActivity(activity, count);
            lastActivity = eventTime;
            if (storedMerits == 0 || activities.Count == 0)
            {
                return;
            }

            if (systems.TryGetValue(currentSystemAddress, out var system))
            {
                if (system.CycleData.TryGetValue(cycle, out var ppData))
                {
                    AddMerits(ppData, storedMerits);
                    storedMerits = 0;
                    UpdateSystemIfLive(system);
                    return;
                }

                var data = new PowerplayCycleData();

                AddMerits(data, storedMerits);
                system.CycleData.Add(cycle, data);
                storedMerits = 0;
                UpdateSystemIfLive(system);
            }
        }

        private void ClearActivities(DateTime cycle)
        {
            inPowerplayCZ = false;
            if (storedMerits <= 0)
            {
                activities.Clear();
                return;
            }

            if (CurrentSystem != null && CurrentSystem.CycleData.TryGetValue(cycle, out var ppData))
            {
                var value = storedMerits;
                storedMerits = 0;
                AddMerits(ppData, value);
                if (storedMerits > 0)
                {
                    ppData.MeritList.Add(new("Unknown", storedMerits));
                    storedMerits = 0;
                }
                UpdateSystemIfLive(CurrentSystem);
                activities.Clear();
            }
        }
        private void AddMerits(PowerplayCycleData data, int value)
        {
            //FDEV being the pain that they are will sometime write the merits event before the activity
            //So we store the value and apply it next time the activity is set
            if (activities.Count == 0)
            {
                storedMerits += value;
                return;
            }

            if (value < 10 && activities.TryGetValue("Ship Scans", out var count))
            {
                data.MeritList.Add(new("Ship Scans", value));
                activities["Ship Scans"]--;
                if (activities["Ship Scans"] == 0)
                {
                    activities.Remove("Ship Scans");
                }
                if (IsLive)
                    MeritsEarned?.Invoke(this, value);

                return;
            }

            if (inPowerplayCZ)
            {
                data.MeritList.Add(new("Enemy CZ Kills", value + storedMerits));
                storedMerits = 0;
                if (IsLive)
                    MeritsEarned?.Invoke(this, value);
                return;
            }

            var other = activities.FirstOrDefault(x => x.Key != "Ship Scans" && x.Value > 0);

            if (other.Value == 0)
            {
                storedMerits += value;
                return;
            }


            data.MeritList.Add(new(other.Key, value + storedMerits));
            storedMerits = 0;
            activities[other.Key]--;
            if (activities[other.Key] == 0)
            {
                activities.Remove(other.Key);
            }
            if (IsLive)
                MeritsEarned?.Invoke(this, value);

        }

        public override void ClearData()
        {
            IsLive = false;
            systems.Clear();
            PledgeData = null;
        }

        public override void Dispose()
        {
            this.journalManager.UnregisterLogProcessor(this);
        }
    }
}
