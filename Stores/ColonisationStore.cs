using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Database;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class ColonisationStore : LogProcessorBase
    {
        public ColonisationStore(IManageJournalEvents journalManager, IODDatabaseProvider databaseProvider)
        {
            this.journalManager = journalManager;
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
            this.journalManager.RegisterLogProcessor(this);
        }

        private readonly IManageJournalEvents journalManager;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private readonly Dictionary<Tuple<long, long>, ConstructionDepot> depots = [];
        private readonly Dictionary<long, CommanderSystem> commanderSystems = [];
        private HashSet<Tuple<long, long>> inactiveDepots = [];
        private HashSet<Tuple<long, long>> shoppingListDepots = [];
        private readonly Dictionary<string, long> constructionTotals = [];
        //When the colonisation events were added to the game
        private readonly DateTime colonisationEventUpdate = new(2025, 4, 7);

        private StarSystem? currentSystem;
        private long CurrentSystemAddress;
        private string CurrentSystemName = "Unknown";
        private string CurrentStationName = "Unknown";

        public override string StoreName => "Colonisation";
        public IEnumerable<ConstructionDepot> Depots => depots.Values.OrderBy(x => x.SystemName);
        public HashSet<Tuple<long, long>> ShoppingList => shoppingListDepots;
        public IEnumerable<CommanderSystem> CommanderSystems => commanderSystems.Values.OrderBy(x => x.SystemName);
        public Dictionary<string, long> ConstructionTotals => constructionTotals;
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.Location,true},
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.CarrierJump, true},
                { JournalTypeEnum.ColonisationBeaconDeployed, true},
                { JournalTypeEnum.ColonisationConstructionDepot, true},
                { JournalTypeEnum.ColonisationContribution, true},
                { JournalTypeEnum.ColonisationSystemClaim, true},
                { JournalTypeEnum.ColonisationSystemClaimRelease, true},
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.Undocked, true},
                { JournalTypeEnum.Cargo, false},
            };
        }

        public EventHandler<ConstructionDepot>? NewDepot;
        public EventHandler<ConstructionDepot>? DepotUpdated;
        public EventHandler<CommanderSystem>? NewCommanderSystem;
        public EventHandler<CommanderSystem>? ReleaseCommanderSystem;
        public EventHandler<bool>? ShoppingListUpdated;

        public override void ClearData()
        {
            depots.Clear();
            commanderSystems.Clear();
            inactiveDepots.Clear();
            shoppingListDepots.Clear();
            CurrentSystemAddress = 0;
            CurrentSystemName = "Unknown";
            CurrentStationName = "Unknown";
            IsLive = false;
        }

        public override void Dispose()
        {
            this.journalManager.UnregisterLogProcessor(this);
        }

        public override void RunBeforeParsingHistory(int currentCmdrId)
        {
            inactiveDepots = databaseProvider.GetInactiveDepots();
            shoppingListDepots = databaseProvider.GetDepotShoppingList();
            constructionTotals.Clear();
        }

        public override DateTime GetJournalAge(DateTime defaultAge)
        {
            var ret = defaultAge > colonisationEventUpdate ? defaultAge : colonisationEventUpdate;

            return ret;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case LocationEvent.LocationEventArgs location:
                    CurrentSystemAddress = location.SystemAddress;
                    CurrentSystemName = location.StarSystem;
                    currentSystem = new(location);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    CurrentSystemAddress = fsdJump.SystemAddress;
                    CurrentSystemName = fsdJump.StarSystem;
                    currentSystem = new(fsdJump);
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
                    CurrentSystemAddress = carrierJump.SystemAddress;
                    CurrentSystemName = carrierJump.StarSystem;
                    currentSystem = new(carrierJump);
                    break;
                case ColonisationBeaconDeployedEvent.ColonisationBeaconDeployedEventArgs:
                    if (commanderSystems.ContainsKey(CurrentSystemAddress) == false)
                    {
                        var newClaim = new CommanderSystem(CurrentSystemAddress, CurrentSystemName);
                        commanderSystems.TryAdd(CurrentSystemAddress, newClaim);
                        TriggerNewCommanderSystemIfLive(newClaim);
                    }
                    break;
                case ColonisationSystemClaimEvent.ColonisationSystemClaimEventArgs colonisationSystemClaim:
                    if (commanderSystems.ContainsKey(colonisationSystemClaim.SystemAddress) == false)
                    {
                        var newClaim = new CommanderSystem(colonisationSystemClaim.SystemAddress, colonisationSystemClaim.StarSystem);
                        commanderSystems.TryAdd(colonisationSystemClaim.SystemAddress, newClaim);
                        TriggerNewCommanderSystemIfLive(newClaim);
                    }
                    break;
                case ColonisationSystemClaimReleaseEvent.ColonisationSystemClaimReleaseEventArgs colonisationSystemClaimRelease:
                    if (commanderSystems.TryGetValue(colonisationSystemClaimRelease.SystemAddress, out var system))
                    {
                        commanderSystems.Remove(colonisationSystemClaimRelease.SystemAddress);
                        if (IsLive)
                            ReleaseCommanderSystem?.Invoke(this, system);
                    }
                    break;
                case ColonisationConstructionDepotEvent.ColonisationConstructionDepotEventArgs depot:
                    if (currentSystem == null)
                        break;
                    var key = Tuple.Create(depot.MarketID, CurrentSystemAddress);
                    if (depots.TryGetValue(key, out ConstructionDepot? value) && value.Update(depot, currentSystem, CurrentStationName))
                    {
                        TriggerDepotUpdateIfLive(value);
                        break;
                    }
                    var newDepot = new ConstructionDepot(depot, currentSystem, CurrentStationName, inactiveDepots.Contains(key));
                    if (depots.TryAdd(key, newDepot))
                    {
                        TriggerNewDepotIfLive(newDepot);
                    }
                    break;
                case ColonisationContributionEvent.ColonisationContributionEventArgs contribution:
                    if (currentSystem == null)
                        break;

                    long delivered = 0;

                    foreach (var item in contribution.Contributions)
                    {
                        delivered += item.Amount;
                    }
                    if (constructionTotals.ContainsKey(currentSystem.Name))
                    {
                        constructionTotals[currentSystem.Name] += delivered;
                        break;
                    }
                    constructionTotals.TryAdd(currentSystem.Name, delivered);
                    break;
                case DockedEvent.DockedEventArgs docked:
                    CurrentStationName = string.IsNullOrEmpty(docked.StationName_Localised) ? docked.StationName : docked.StationName_Localised;
                    break;
            }
        }

        private void TriggerNewCommanderSystemIfLive(CommanderSystem newClaim)
        {
            if (IsLive)
            {
                NewCommanderSystem?.Invoke(this, newClaim);
            }
        }

        private void TriggerDepotUpdateIfLive(ConstructionDepot value)
        {
            if (IsLive)
            {
                DepotUpdated?.Invoke(this, value);
            }
        }

        private void TriggerNewDepotIfLive(ConstructionDepot newDepot)
        {
            if (IsLive)
            {
                NewDepot?.Invoke(this, newDepot);
            }
        }

        internal void SetDepotActiveState(ConstructionDepotVM vM)
        {
            SetDepot(vM);
            inactiveDepots = databaseProvider.GetInactiveDepots();

            if (depots.TryGetValue(Tuple.Create(vM.MarketID, vM.SystemAddress), out var depot))
            {
                depot.Inactive = vM.Inactive;
            }
        }

        private void SetDepot(ConstructionDepotVM vM)
        {
            if (vM.Inactive)
            {
                databaseProvider.AddInactiveDepot(vM.MarketID, vM.SystemAddress, vM.StationName);
                return;
            }

            databaseProvider.RemoveInactiveDepot(vM.MarketID, vM.SystemAddress, vM.StationName);
        }

        internal bool SetDepotShoppingState(ConstructionDepotVM vM)
        {
            var tuple = Tuple.Create(vM.MarketID, vM.SystemAddress);
            var add = !shoppingListDepots.Contains(tuple);
            SetDepotShopping(vM, add);
            shoppingListDepots = databaseProvider.GetDepotShoppingList();
            ShoppingListUpdated?.Invoke(this, add);
            return add;
        }

        private void SetDepotShopping(ConstructionDepotVM vM, bool add)
        {
            if (add)
            {
                databaseProvider.AddShoppingListDepot(vM.MarketID, vM.SystemAddress, vM.StationName);
                return;
            }

            databaseProvider.RemoveShoppingListDepot(vM.MarketID, vM.SystemAddress, vM.StationName);
        }
    }
}
