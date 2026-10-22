using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Database;
using ODEliteTracker.Helpers;
using ODEliteTracker.Managers;
using ODEliteTracker.Models.BGS;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Market;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Notifications;
using ODEliteTracker.Notifications.ScanNotification;
using ODEliteTracker.Services;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Helpers;

namespace ODEliteTracker.Stores
{
    public sealed class SharedDataStore : LogProcessorBase, IWatchStatus
    {
        public SharedDataStore(IManageJournalEvents journalManager,
                               NotificationService notificationService,
                               IODDatabaseProvider databaseProvider,
                               SettingsStore settings,
                               StationMarketController marketController)
        {
            this.journalManager = journalManager;
            this.notificationService = notificationService;
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
            this.settings = settings;
            this.marketController = marketController;
            this.journalManager.RegisterLogProcessor(this);

            bountiesManager = new(databaseProvider);
            materialTraderService = new();
            this.journalManager.OnStatusFileUpdate += JournalManager_OnStatusFileUpdate;
        }

        #region Private fields
        private readonly IManageJournalEvents journalManager;
        private readonly NotificationService notificationService;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private readonly SettingsStore settings;
        private readonly StationMarketController marketController;
        private readonly Dictionary<string, FactionData> factions = [];
        private readonly Dictionary<ulong, StarSystem> systems = [];
        private string? commanderPower;
        private readonly BountiesManager bountiesManager;
        private readonly MaterialTraderService materialTraderService;
        private readonly Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> marketPurchases = [];
        private Station? currentStation;
        private readonly Dictionary<ColonisationBuild, int> colonisationWishList = [];
        private int currentCommanderId = -1;
        private readonly Dictionary<string, string> carrierNames = [];
        private readonly Dictionary<ulong, string> carrierNamesByMarketID = [];
        private readonly Dictionary<int, ShipInfo> commandersShips = [];
        private List<SystemInRoute> currentRoute = [];
        #endregion

        #region Public Properties
        public override string StoreName => "Shared Data";
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.LoadGame, true  },
                { JournalTypeEnum.ApproachBody, true},
                { JournalTypeEnum.Bounty, true },
                { JournalTypeEnum.Cargo, false },
                { JournalTypeEnum.CarrierJump, true},
                { JournalTypeEnum.Commander, true },
                { JournalTypeEnum.CommitCrime, false },
                { JournalTypeEnum.Died, true },
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.DockingDenied, false },
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.FSSSignalDiscovered, true },
                { JournalTypeEnum.FSSSignalsDiscoveredEvent, true },
                { JournalTypeEnum.JetConeBoost, true },
                { JournalTypeEnum.LeaveBody, true},
                { JournalTypeEnum.Loadout, true },
                { JournalTypeEnum.Location, true },
                { JournalTypeEnum.Market, false },
                { JournalTypeEnum.MarketBuy, true},
                { JournalTypeEnum.MarketSell, true},
                { JournalTypeEnum.NavRoute, false},
                { JournalTypeEnum.NavRouteClear, false},
                { JournalTypeEnum.Powerplay, true },
                { JournalTypeEnum.RedeemVoucher, true},
                { JournalTypeEnum.Scan, true},
                { JournalTypeEnum.ShipTargeted, false },
                { JournalTypeEnum.SupercruiseEntry, true},
                { JournalTypeEnum.SellShipOnRebuy, true},
                { JournalTypeEnum.ShipyardSell, true},
                { JournalTypeEnum.Synthesis, true},
                { JournalTypeEnum.Undocked, true},
                { JournalTypeEnum.FSSDiscoveryScan, true },
            };
        }
        public Dictionary<string, FactionData> Factions => factions;
        public StarSystem? CurrentSystem { get; private set; }
        public SystemBody? CurrentBody { get; private set; }
        public StationMarket? CurrentMarket { get { return marketController.StationMarket; } }
        public List<WatchedMarket> WatchedMarkets { get; private set; } = [];
        public string? CurrentBody_Station { get; private set; }
        public ulong CurrentMarketID { get; private set; } = 0;
        public ShipInfo? CurrentShipInfo { get; private set; }
        public IEnumerable<ShipCargo>? CurrentShipCargo => CurrentShipInfo?.Cargo;
        public Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> MarketPurchases => marketPurchases;
        public DateTime LastLoginTime { get; private set; }
        public Dictionary<ColonisationBuild, int> ColonisationWishList => colonisationWishList;
        public Dictionary<ulong, StarSystem> Systems => systems;
        public List<SystemInRoute> CurrentRoute => currentRoute;
        public IEnumerable<ShipInfo> Ships => commandersShips.Values;
        public Station? CurrentStation => currentStation;
        #endregion

        #region Events
        public EventHandler<StarSystem?>? CurrentSystemChanged;
        public EventHandler<string?>? CurrentBody_StationChanged;
        public EventHandler<StationMarket?>? MarketEvent;
        public EventHandler<ShipInfo?>? ShipChangedEvent;
        public EventHandler<ShipInfo?>? ShipCargoUpdatedEvent;
        public EventHandler<ShipInfo?>? ShipStatsUpdated;
        public EventHandler? BountiesUpdated;
        public EventHandler<CommodityPurchase>? PurchasesUpdated;
        public EventHandler<WatchedMarket>? WatchedMarketUpdated;
        public EventHandler<WatchedMarketsUpdatedEventArgs>? WatchedMarketsUpdated;
        public EventHandler<StarSystem>? SystemBodiesUpdated;
        public EventHandler<StarSystem>? SystemDSSScaned;
        public EventHandler<Station?>? Docked;
        public EventHandler<List<SystemInRoute>>? CurrentRouteChanged;
        #endregion

        public override void ClearData()
        {
            CurrentSystem = null;          
            CurrentShipInfo = null;
            CurrentBody_Station = null;
            CurrentSystemChanged?.Invoke(this, null);
            MarketEvent?.Invoke(this, null);
            ShipChangedEvent?.Invoke(this, null);
            ShipCargoUpdatedEvent?.Invoke(this, null);
            MarketPurchases.Clear();
            LastLoginTime = DateTime.MinValue;
            marketController.ClearData();
            commandersShips.Clear();
            WatchedMarkets.Clear();
            IsLive = false;
            journalManager.StopStatusWatcher(this);
        }

        public override void Dispose()
        {
            journalManager.UnregisterLogProcessor(this);
        }

        public override DateTime GetJournalAge(DateTime defaultAge)
        {
            return defaultAge;
        }

        public override Task ParseHistoryStream(JournalEntry entry)
        {
            ParseJournalEvent(entry);
            return Task.CompletedTask;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case LoadGameEvent.LoadGameEventArgs loadGame:
                    CurrentBody = null;
                    LastLoginTime = loadGame.Timestamp;
                    UpdateCurrentSystem(null);
                    UpdateCurrentBody_Station(string.Empty);
                    CurrentMarketID = 0;
                    break;
                case ApproachBodyEvent.ApproachBodyEventArgs approachBody:
                    CurrentBody = CurrentSystem?.Bodies.FirstOrDefault(x => x.Key == approachBody.BodyID).Value;

                    if (CurrentBody != null)
                        CurrentBody.Landable = true;
                    UpdateCurrentBody_Station(approachBody.Body);
                    break;

                case BountyEvent.BountyEventArgs bounty:
                    if (bounty.Rewards is null || bounty.Rewards.Count == 0)
                        break;

                    foreach (var reward in bounty.Rewards)
                    {
                        bountiesManager.AddBounty(new VoucherClaim(Models.VoucherType.Bounty, reward.Faction, reward.Reward, bounty.Timestamp));
                    }

                    FireBountiesChangedIfLive();
                    break;

                case CargoEvent.CargoEventArgs:
                    GetShipCargo();
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
                    carrierJump.StationName = CheckForCarrierNames(carrierJump.StationType, carrierJump.StationName, carrierJump.MarketID);
                    UpdateCurrentSystem(new(carrierJump));
                    string? bodyStn = null;
                    if (string.IsNullOrEmpty(carrierJump.Body) == false)
                    {
                        bodyStn = carrierJump.Body;
                    }
                    if (string.IsNullOrEmpty(carrierJump.StationName) == false)
                    {
                        bodyStn = carrierJump.StationName;
                    }
                    UpdateCurrentBody_Station(bodyStn);
                    AddFactions(carrierJump.Factions);

                    if (carrierJump.Population == 0 || carrierJump.SystemFaction is null || carrierJump.Factions is null || carrierJump.Factions.Count == 0)
                    {
                        SystemNotification(carrierJump.StarSystem,
                        [
                            "Unpopulated",
                            carrierJump.SystemSecurity_Localised,
                        ]);
                        break;
                    }

                    SystemNotification(carrierJump.StarSystem,
                        [
                            carrierJump.SystemFaction.Name,
                            $"Population : {EliteHelpers.FormatNumber(carrierJump.Population ?? 0)}",
                            carrierJump.SystemAllegiance,
                            carrierJump.SystemFaction.FactionState,
                            carrierJump.SystemSecurity_Localised,
                            EliteHelpers.FactionReputationToString(carrierJump.Factions.FirstOrDefault(x => string.Equals(x.Name, carrierJump.SystemFaction.Name))?.MyReputation)
                        ]);
                    break;

                case CommanderEvent.CommanderEventArgs commander:
                    LastLoginTime = commander.Timestamp;
                    break;

                case CommitCrimeEvent.CommitCrimeEventArgs commitCrimeEvent:
                    if (!IsLive)
                        break;

                    notificationService.ShowCrimeNotification(commitCrimeEvent);
                    break;

                case DiedEvent.DiedEventArgs died:
                    bountiesManager.Reset();
                    break;

                case DockedEvent.DockedEventArgs docked:

                    CurrentMarketID = docked.MarketID;

                    docked.StationName = docked.StationName_Localised ?? docked.StationName;
                    docked.StationName = docked.StationName.Replace("$EXT_PANEL_ColonisationShip;", "Colonisation Ship -");

                    if (docked.StationType == "FleetCarrier" && carrierNames.TryGetValue(docked.StationName, out var name))
                    {
                        docked.StationName = name;
                    }

                    UpdateCurrentBody_Station(docked.StationName);

                    if (CurrentSystem != null)
                    {
                        currentStation = new(docked, new(CurrentSystem), CurrentSystem);
                    }

                    if (IsLive == false)
                        break;

                    Docked?.Invoke(this, currentStation);

                    Task.Run(async () =>
                    {
                        var market = await marketController.GetMarket(docked);

                        UpdateMarkets(market);
                    });

                    var stationFaction = docked.StationFaction.Name;

                    if (stationFaction.Equals("FleetCarrier", StringComparison.OrdinalIgnoreCase))
                    {
                        stationFaction = docked.StationServices.Contains("squadronBank") ? "Squadron Carrier" : "Personal Carrier";
                    }
                    var args = new NotificationArgs(docked.StationName,
                        [
                            $"{EliteJournalReaderHelpers.StationTypeText(docked.StationType)}",
                            $"{stationFaction}",
                            $"{docked.StationGovernment_Localised ?? docked.StationGovernment}",
                            $"{docked.StationEconomy_Localised ?? docked.StationEconomy}",
                            $"{docked.LandingPads.LandPadText()}"
                        ],
                        NotificationOptions.Station);

                    notificationService.ShowBasicNotification(args);
                    break;

                case DockingDeniedEvent.DockingDeniedEventArgs dockingDenied:
                    notificationService.ShowDockingNotification("Docking Denied", dockingDenied.Reason);
                    break;

                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    if (CurrentShipInfo != null)
                    {
                        CurrentShipInfo.CurrentBoostValue = 1;
                        if (IsLive)
                            ShipStatsUpdated?.Invoke(this, CurrentShipInfo);
                    }
                    CurrentBody = null;
                    UpdateCurrentSystem(new(fsdJump));
                    UpdateCurrentBody_Station(null);
                    AddFactions(fsdJump.Factions);
                    if (fsdJump.SystemFaction is null || fsdJump.Factions is null || fsdJump.Factions.Count == 0)
                    {
                        SystemNotification(fsdJump.StarSystem,
                        [
                            "Unpopulated",
                            fsdJump.SystemSecurity_Localised,
                        ]);
                        break;
                    }
                    SystemNotification(fsdJump.StarSystem,
                    [
                            fsdJump.SystemFaction.Name,
                            $"Population : {EliteHelpers.FormatNumber(fsdJump.Population)}",
                            fsdJump.SystemAllegiance,
                            //fsdJump.SystemFaction.FactionState,
                            fsdJump.ControllingFactionState,
                            fsdJump.SystemSecurity_Localised,
                            EliteHelpers.FactionReputationToString(fsdJump.Factions.FirstOrDefault(x => string.Equals(x.Name, fsdJump.SystemFaction.Name))?.MyReputation)
                        ]);
                    break;

                case FSSSignalDiscoveredEvent.FSSSignalDiscoveredEventArgs fss:
                    ProcessFSSSignals(fss);
                    break;
                case FSSSignalsDiscoveredEvent.FSSSignalsDiscoveredEventArgs fssSignals:
                    foreach(var signal in fssSignals.Signals)
                    {
                        ProcessFSSSignals(signal);
                    }
                    break;
                case JetConeBoostEvent.JetConeBoostEventArgs jetConeBoost:
                    CurrentShipInfo?.UpdateCurrentBoost(jetConeBoost);
                    if (IsLive)
                    {
                        ShipStatsUpdated?.Invoke(this, CurrentShipInfo);                        
                    }
                    break;
                case LeaveBodyEvent.LeaveBodyEventArgs leaveBody:
                    CurrentBody = null;
                    UpdateCurrentBody_Station(null);
                    break;
                case LoadoutEvent.LoadoutEventArgs loadout:
                    UpdateShips(loadout);
                    if (IsLive)
                        ShipChangedEvent?.Invoke(this, CurrentShipInfo);
                    break;

                case LocationEvent.LocationEventArgs location:
                    location.StationName = CheckForCarrierNames(location.StationType, location.StationName, location.MarketID);
                    UpdateCurrentSystem(new(location));
                    string? bodyStation = null;
                    if (string.IsNullOrEmpty(location.Body) == false)
                    {
                        bodyStation = location.Body;
                    }
                    if (string.IsNullOrEmpty(location.StationName) == false)
                    {
                        bodyStation = CheckForCarrierNames(location.StationType, location.StationName, location.MarketID);
                    }
                    UpdateCurrentBody_Station(bodyStation);
                    AddFactions(location.Factions);

                    if (location.BodyType == BodyType.Planet && CurrentSystem != null)
                    {
                        var body = new SystemBody(location, CurrentSystem);

                        CurrentSystem.AddBody(body);
                    }

                    if (location.SystemFaction is null || location.Factions is null || location.Factions.Count == 0)
                    {
                        SystemNotification(location.StarSystem,
                        [
                            "Unpopulated",
                            location.SystemSecurity_Localised,
                        ]);
                        break;
                    }

                    SystemNotification(location.StarSystem,
                        [
                            location.SystemFaction.Name,
                            $"Population : {EliteHelpers.FormatNumber(location.Population ?? 0)}",
                            location.SystemAllegiance,
                            location.ControllingFactionState,
                            location.SystemSecurity_Localised,
                            EliteHelpers.FactionReputationToString(location.Factions.FirstOrDefault(x => string.Equals(x.Name, location.SystemFaction.Name))?.MyReputation)
                        ]);
                    break;

                case MarketEvent.MarketEventArgs marketArgs:
                    Task.Factory.StartNew(async () =>
                    {
                        marketArgs.StationName = CheckForCarrierNames(marketArgs.StationType, marketArgs.StationName, marketArgs.MarketID);
                        var market = await marketController.GetMarket(marketArgs);
                        UpdateMarkets(market);
                    });
                    break;

                case MarketBuyEvent.MarketBuyEventArgs marketBuy:
                    if (currentStation == null
                        || CurrentSystem == null)
                    {
                        break;
                    }
                    var commodity = EliteCommodityHelpers.GetCommodityFromPartial(marketBuy.Type, marketBuy.Type_Localised);

                    var purcashedItem = marketController.GetMarketItemToBuy(commodity.FdevName, marketBuy.Type, commodity.Id);

                    if (purcashedItem != null)
                    {
                        purcashedItem.Stock -= marketBuy.Count;

                        if (purcashedItem.Stock < 0)
                            purcashedItem.Stock = 0;

                        MarketEvent?.Invoke(this, CurrentMarket);
                    }

                    //We don't care about fleet carrier purchases for the history
                    if (currentStation.StationType.Equals("FleetCarrier", StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }

                    var purchase = new CommodityPurchase(commodity, currentStation, marketBuy.BuyPrice, marketBuy.Timestamp);

                    if (marketPurchases.TryGetValue(commodity, out var list))
                    {
                        var knownCommodity = list.FirstOrDefault(x => x.Commodity == commodity && x.Station.MarketID == currentStation.MarketID);

                        if (knownCommodity != null)
                        {
                            //Remove the older entry
                            list.Remove(knownCommodity);
                        }
                        list.Add(purchase);
                        if (IsLive)
                            PurchasesUpdated?.Invoke(this, purchase);
                        break;
                    }

                    marketPurchases.TryAdd(commodity, [purchase]);
                    if (IsLive)
                        PurchasesUpdated?.Invoke(this, purchase);
                    break;

                case MarketSellEvent.MarketSellEventArgs sell:
                    if (IsLive == false)
                        break;

                    var commod = EliteCommodityHelpers.GetCommodityFromPartial(sell.Type, string.IsNullOrEmpty(sell.Type_Localised) ? sell.Type : sell.Type_Localised);

                    var marketItem = marketController.GetMarketItemForSale(commod.FdevName, sell.Type, commod.Id);

                    if (marketItem != null)
                    {
                        marketItem.Demand -= sell.Count;

                        if (marketItem.Demand < 0)
                            marketItem.Demand = 0;

                        MarketEvent?.Invoke(this, CurrentMarket);
                    }

                    var known = WatchedMarkets.FirstOrDefault(x => x.MarketID == sell.MarketID);

                    if (known == null)
                        break;

                    var item = known.ItemsForPurchase.FirstOrDefault(x => string.Equals(x.Name, commod.FdevName));

                    if (item == null)
                    {
                        break;
                    }
                    item.Stock += sell.Count;
                    item.Demand -= sell.Count;

                    if (item.Demand <= 0)
                    {
                        known.ItemsForPurchase.Remove(item);
                    }

                    if (IsLive)
                    {
                        WatchedMarketUpdated?.Invoke(this, known);
                    }
                    break;
                case NavRouteEvent.NavRouteEventArgs:
                    var route = journalManager.ReadNavRouteJson(currentCommanderId);

                    if (route == null || route.Route.Any() == false)
                    {
                        SetNavRoute(null);
                        return;
                    }
                    SetNavRoute(route);
                    break;
                case NavRouteClearEvent.NavRoutClearEventArgs:
                    SetNavRoute(null);
                    break;
                case PowerplayEvent.PowerplayEventArgs powerPlay:
                    commanderPower = powerPlay.Power;
                    break;

                case RedeemVoucherEvent.RedeemVoucherEventArgs redeemVoucher:

                    if (!string.Equals(redeemVoucher.Type, "Bounty", StringComparison.OrdinalIgnoreCase))
                        break;

                    var boutniesRemoved = false;

                    if (redeemVoucher.Factions == null || redeemVoucher.Factions.Count == 0)
                    {
                        if (string.IsNullOrEmpty(redeemVoucher.Faction))
                            break;


                        boutniesRemoved = bountiesManager.FactionBountiesClaimed(redeemVoucher.Faction, redeemVoucher.Amount, redeemVoucher.BrokerPercentage);
                        if (boutniesRemoved)
                            FireBountiesChangedIfLive();
                        break;
                    }

                    foreach (var faction in redeemVoucher.Factions)
                    {
                        boutniesRemoved = bountiesManager.FactionBountiesClaimed(faction, redeemVoucher.BrokerPercentage) || boutniesRemoved;
                    }

                    if (boutniesRemoved)
                        FireBountiesChangedIfLive();
                    break;

                case ScanEvent.ScanEventArgs scan:
                    if (CurrentSystem is null || CurrentSystem.Address != scan.SystemAddress)
                        break;
                    if (CurrentSystem.AddBody(scan) && IsLive)
                    {
                        SystemBodiesUpdated?.Invoke(this, CurrentSystem);
                    }
                    break;

                case ShipTargetedEvent.ShipTargetedEventArgs shipTargeted:

                    if (shipTargeted.ScanStage != 3)
                        break;

                    var pilotName = shipTargeted.PilotName_Localised ?? shipTargeted.PilotName;

                    if (string.IsNullOrEmpty(pilotName))
                    {
                        return;
                    }

                    if (string.IsNullOrEmpty(shipTargeted.Power) && shipTargeted.Bounty <= 0)
                    {
                        break;
                    }

                    var targetType = TargetType.None;

                    if (string.IsNullOrEmpty(shipTargeted.Power) == false && string.Equals(shipTargeted.Power, commanderPower) == false)
                    {
                        targetType |= TargetType.Enemy;
                    }

                    if (shipTargeted.Bounty > 0)
                    {
                        targetType |= TargetType.Wanted;
                    }

                    if (targetType == TargetType.None)
                    {
                        break;
                    }

                    if (IsLive)
                        notificationService.ShowShipTargetedNotification(pilotName, EliteHelpers.ConvertShipName(shipTargeted.Ship), targetType, shipTargeted.Bounty, shipTargeted.Faction, shipTargeted.Power);
                    break;
                case SellShipOnRebuyEvent.SellShipOnRebuyEventArgs sellOnRebuy:
                    var soldrebuyShip = commandersShips.FirstOrDefault(x => x.Value.ShipId == sellOnRebuy.SellShipID);
                    var removed = commandersShips.Remove(sellOnRebuy.SellShipID);
                    if (IsLive && removed)
                    {
                        ShipChangedEvent?.Invoke(this, soldrebuyShip.Value);
                    }
                    break;
                case ShipyardSellEvent.ShipyardSellEventArgs shipyardSell:
                    var soldShip = commandersShips.FirstOrDefault(x => x.Value.ShipId == shipyardSell.SellShipID);
                    var shipRemoved = commandersShips.Remove(shipyardSell.SellShipID);
                    if (IsLive && shipRemoved)
                    {
                        ShipChangedEvent?.Invoke(this, soldShip.Value);
                    }
                    break;
                case SupercruiseEntryEvent.SupercruiseEntryEventArgs:
                    CurrentBody = null;
                    break;
                    case SynthesisEvent.SynthesisEventArgs synthesisEventArgs:
                    if (CurrentShipInfo == null)
                        break;
                    if(CurrentShipInfo.ProcessSynth(synthesisEventArgs) && IsLive)
                    {
                        ShipStatsUpdated?.Invoke(this, CurrentShipInfo);
                    }
                    break;
                case UndockedEvent.UndockedEventArgs:
                    CurrentMarketID = 0;
                    currentStation = null;
                    UpdateCurrentBody_Station(CurrentBody?.BodyName);
                    break;

                case FSSDiscoveryScanEvent.FSSDiscoveryScanEventArgs discoveryScan:
                    if (systems.TryGetValue(discoveryScan.SystemAddress, out var knownSystem))
                    {
                        knownSystem.BodyCount = discoveryScan.BodyCount;
                        SystemDSSScaned?.Invoke(this, knownSystem);
                    }
                    break;
            }
        }

        private void GetShipCargo()
        {
            var cargo = journalManager.GetCargo();

            if (cargo != null && cargo.Vessel.Equals("Ship", StringComparison.OrdinalIgnoreCase) && CurrentShipInfo != null)
            {
                CurrentShipInfo.Cargo = [.. cargo.Inventory.Select(x =>
                        {
                            return new ShipCargo((string.IsNullOrEmpty(x.Name_Localised) ? x.Name : x.Name_Localised).ToTitleCase(), x.Count);
                        })];

                if (CurrentShipCargo != null)
                {
                    ShipCargoUpdatedEvent?.Invoke(this, CurrentShipInfo);
                }
            }
        }

        private void ProcessFSSSignals(FSSSignalDiscoveredEvent.FSSSignalDiscoveredEventArgs fss)
        {
            if (fss.IsStation == false)
                return;

            if (string.Equals(fss.SignalType, "FleetCarrier", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(fss.SignalName) == false)
            {
                var id = fss.SignalName.Split(' ').Last();

                if (carrierNames.ContainsKey(id))
                {
                    carrierNames[id] = fss.SignalName;
                    return;
                }

                _ = carrierNames.TryAdd(id, fss.SignalName);
                return;
            }

            if (string.Equals(fss.SignalType, "SquadronCarrier", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty(fss.SignalName) == false)
            {
                var id = fss.SignalName.Split('|').Last().Trim();

                if (carrierNames.ContainsKey(id))
                {
                    carrierNames[id] = fss.SignalName;
                    return;
                }

                _ = carrierNames.TryAdd(id, fss.SignalName);
            }

            return;
        }

        private void SetNavRoute(NavigationRoute? route)
        {
            if (route == null)
            {
                currentRoute.Clear();
                CurrentRouteChanged?.Invoke(this, currentRoute);
                return;
            }

            currentRoute = [.. route.Route];
            CurrentRouteChanged?.Invoke(this, currentRoute);
        }

        private void UpdateShips(LoadoutEvent.LoadoutEventArgs loadout)
        {    
            if (commandersShips.TryGetValue(loadout.ShipID, out ShipInfo? value))
            {
                value.Update(loadout);
                CurrentShipInfo = value;
                return;
            }

            var ship = new ShipInfo(loadout);
            CurrentShipInfo = ship;
            commandersShips.TryAdd(loadout.ShipID, ship);            
        }

        private void UpdateMarkets(StationMarket? market)
        {
            if (market != null)
            {
                MarketEvent?.Invoke(this, CurrentMarket);

                var watched = WatchedMarkets.FirstOrDefault(x => x.MarketID == market.MarketID);

                if (watched != null)
                {
                    watched.UpdatePurchaseOrders(market);
                    WatchedMarketUpdated?.Invoke(this, watched);
                }
            }
        }

        private string CheckForCarrierNames(string stationType, string stationName, ulong marketId)
        {
            if (stationType == "FleetCarrier" && carrierNames.TryGetValue(stationName, out var carrierName))
            {
                if (carrierNamesByMarketID.ContainsKey(marketId))
                {
                    carrierNamesByMarketID[marketId] = carrierName;
                    return carrierName;
                }

                carrierNamesByMarketID.TryAdd(marketId, stationName);
            }
            return stationName;
        }

        #region Watched Markets
        public void AddRemoveWatchedMarket(ulong marketId)
        {
            var known = WatchedMarkets.FirstOrDefault(x => x.MarketID == marketId);

            if (known != null)
            {
                WatchedMarkets.Remove(known);
                WatchedMarketsUpdated?.Invoke(this, new(MarketStatus.Removed, known));
                return;
            }

            if (CurrentMarket != null && marketId == CurrentMarket.MarketID)
            {
                known = new WatchedMarket(CurrentMarket);
                WatchedMarkets.Add(known);
                WatchedMarketsUpdated?.Invoke(this, new(MarketStatus.Added, known));
            }
        }
        #endregion

        private void SystemNotification(string name, string[] fields)
        {
            if (IsLive == false)
                return;

            var args = new NotificationArgs(name, fields, NotificationOptions.System);
            notificationService.ShowBasicNotification(args);
        }

        private void UpdateCurrentSystem(StarSystem? starSystem)
        {
            if (CurrentSystem != null && starSystem != null && starSystem.Address == CurrentSystem.Address && !CurrentSystem.DataChanged(starSystem))
            {
                //Nothing to do here
                return;
            }

            CurrentSystem = starSystem == null ? null : AddSystem(starSystem);

            if (IsLive == false)
            {
                //No need to trigger events if parsing history
                return;
            }

            CurrentSystemChanged?.Invoke(this, CurrentSystem);
        }

        private StarSystem AddSystem(StarSystem starSystem)
        {
            if (systems.TryGetValue(starSystem.Address, out var system))
            {
                system.UpdateSystem(starSystem);
                return system;
            }

            systems.TryAdd(starSystem.Address, starSystem);
            return starSystem;
        }

        private void UpdateCurrentBody_Station(string? text)
        {
            if (string.IsNullOrEmpty(text) == false)
            {
                text = text.Replace("$EXT_PANEL_ColonisationShip;", "Colonisation Ship -");
            }
            CurrentBody_Station = text;
            if (IsLive)
                CurrentBody_StationChanged?.Invoke(this, CurrentBody_Station);
        }

        private void AddFactions(IEnumerable<Faction>? factions)
        {
            if (factions != null && factions.Any())
            {
                foreach (var faction in factions)
                {
                    this.factions.TryAdd(faction.Name, new(faction.Name, faction.Government, faction.Allegiance));
                }
            }
        }

        public override void RunBeforeParsingHistory(int currentCmdrId)
        {
            currentCommanderId = currentCmdrId;
            bountiesManager.Initialise(currentCmdrId);
            marketController.ClearData();
            WatchedMarkets.Clear();
            colonisationWishList.Clear();
            var wishListItem = databaseProvider.GetColonisationWishList(currentCmdrId);
            foreach (var wish in wishListItem)
            {
                colonisationWishList.TryAdd(wish.Key, wish.Value);
            }
        }

        public void IgnoreAllBounties()
        {
            foreach (var bounty in bountiesManager.GetBounties())
            {
                bountiesManager.AddIgnoredBounty(settings.SelectedCommanderID, bounty.FactionName);
            }
            FireBountiesChangedIfLive();
        }

        public void AddIgnoredBountyFaction(string factionName)
        {
            bountiesManager.AddIgnoredBounty(settings.SelectedCommanderID, factionName);
            FireBountiesChangedIfLive();
        }

        public void RemoveIgnoreBountyFaction(string factionName)
        {
            bountiesManager.RemovedIgnoreBounty(settings.SelectedCommanderID, factionName);
            FireBountiesChangedIfLive();
        }

        public List<BountyClaims> GetBounties()
        {
            return bountiesManager.GetBounties();
        }

        private void FireBountiesChangedIfLive()
        {
            if (IsLive)
            {
                BountiesUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void RunAfterParsingHistory()
        {
            journalManager.StartStatusWatcher(this);

            GetShipCargo();

            var market = marketController.GetMarket();

            if (market != null)
            {
                if (carrierNames.TryGetValue(market.StationName, out var name))
                {
                    market.StationName = name;
                }
                MarketEvent?.Invoke(this, CurrentMarket);
            }

            var watchedMarkets = databaseProvider.GetWatchedMarkets(currentCommanderId);

            if (watchedMarkets != null)
            {
                WatchedMarkets = watchedMarkets;
            }

            base.RunAfterParsingHistory();
        }

        public override void SaveData()
        {
            Save();
        }

        public void Save()
        {
            if (currentCommanderId <= 0)
                return;
            databaseProvider.UpdateWatchedMarkets(WatchedMarkets, currentCommanderId);
            databaseProvider.UpdateColonisationWishList(currentCommanderId, colonisationWishList);
        }

        public Tuple<IEnumerable<MaterialTrader>, IEnumerable<MaterialTrader>, IEnumerable<MaterialTrader>> GetNearestTraders()
        {
            //This should never happen if we have any logs
            if (CurrentSystem == null)
                return Tuple.Create(Enumerable.Empty<MaterialTrader>(), Enumerable.Empty<MaterialTrader>(), Enumerable.Empty<MaterialTrader>());

            return materialTraderService.GetNearestTraders(CurrentSystem.Position);
        }

        public void UpdateColonisationWishListItem(ColonisationBuildWishListItem item)
        {
            if (colonisationWishList.ContainsKey(item.Build))
            {
                colonisationWishList[item.Build] = item.Count;

                if (item.Count <= 0)
                    colonisationWishList.Remove(item.Build);
                return;
            }

            colonisationWishList.TryAdd(item.Build, item.Count);
        }

        public NavigationRoute? GetNavRoute() => journalManager.ReadNavRouteJson(currentCommanderId);

        private void JournalManager_OnStatusFileUpdate(object? sender, StatusFileEvent e)
        {
            if (!IsLive)
                return;

            if (!e.Flags.HasFlag(StatusFlags.InMainShip))
                return;

            if (CurrentShipInfo == null)
                return;

            if (CurrentShipInfo.UpdateFromStatus(e))
                ShipStatsUpdated?.Invoke(this, CurrentShipInfo);
        }
    }
}
