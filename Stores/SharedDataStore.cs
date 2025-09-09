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
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Helpers;

namespace ODEliteTracker.Stores
{
    public sealed class SharedDataStore : LogProcessorBase
    {
        public SharedDataStore(IManageJournalEvents journalManager,
                               NotificationService notificationService,
                               IODDatabaseProvider databaseProvider,
                               SettingsStore settings)
        {
            this.journalManager = journalManager;
            this.notificationService = notificationService;
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
            this.settings = settings;
            this.journalManager.RegisterLogProcessor(this);

            bountiesManager = new(databaseProvider);
            materialTraderService = new();
        }

        #region Private fields
        private readonly IManageJournalEvents journalManager;
        private readonly NotificationService notificationService;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private readonly SettingsStore settings;
        private readonly Dictionary<string, FactionData> factions = [];
        private readonly Dictionary<long, StarSystem> Systems = [];
        private string? commanderPower;
        private readonly BountiesManager bountiesManager;
        private readonly MaterialTraderService materialTraderService;
        private readonly Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> marketPurchases = [];
        private Station? currentStation;
        #endregion

        #region Public Properties
        public override string StoreName => "Shared Data";
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.Powerplay, true },
                { JournalTypeEnum.Location, true },
                { JournalTypeEnum.FSDJump, true},
                { JournalTypeEnum.CarrierJump, true},
                { JournalTypeEnum.Docked, true},
                { JournalTypeEnum.Undocked, true},
                { JournalTypeEnum.ApproachBody, true},
                { JournalTypeEnum.Market, false },
                { JournalTypeEnum.Loadout, true },
                { JournalTypeEnum.Cargo, false },
                { JournalTypeEnum.ShipTargeted, false },
                { JournalTypeEnum.Bounty, true },
                { JournalTypeEnum.RedeemVoucher, true},
                { JournalTypeEnum.MarketBuy, true},
                { JournalTypeEnum.MarketSell, true},
                { JournalTypeEnum.Scan, true},
                { JournalTypeEnum.SupercruiseEntry, true},
                { JournalTypeEnum.Died, true }
            };
        }
        public Dictionary<string, FactionData> Factions => factions;
        public StarSystem? CurrentSystem { get; private set; }
        public SystemBody? CurrentBody { get; private set; }
        public StationMarket? CurrentMarket { get; private set; }
        public List<WatchedMarket> WatchedMarkets { get; private set; } = [];
        public string? CurrentBody_Station { get; private set; }
        public ulong CurrentMarketID { get; private set; } = 0;

        public ShipInfo? CurrentShipInfo { get; private set; }
        public IEnumerable<ShipCargo>? CurrentShipCargo { get; private set; }
        public Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> MarketPurchases => marketPurchases;
        #endregion

        #region Events
        public EventHandler<StarSystem?>? CurrentSystemChanged;
        public EventHandler<string?>? CurrentBody_StationChanged;
        public EventHandler<StationMarket?>? MarketEvent;
        public EventHandler<ShipInfo?>? ShipChangedEvent;
        public EventHandler<IEnumerable<ShipCargo>?>? ShipCargoUpdatedEvent;
        public EventHandler? BountiesUpdated;
        public EventHandler<CommodityPurchase>? PurchasesUpdated;
        public EventHandler<WatchedMarket>? WatchedMarketUpdated;
        public EventHandler<List<WatchedMarket>>? WatchedMarketsUpdated;
        #endregion

        public override void ClearData()
        {
            CurrentSystem = null;
            CurrentMarket = null;
            CurrentShipInfo = null;
            CurrentShipCargo = null;
            CurrentBody_Station = null;
            CurrentSystemChanged?.Invoke(this, null);
            MarketEvent?.Invoke(this, null);
            ShipChangedEvent?.Invoke(this, null);
            ShipCargoUpdatedEvent?.Invoke(this, null);
            IsLive = false;
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
                    var cargo = journalManager.GetCargo();

                    if (cargo != null && cargo.Vessel.Equals("Ship", StringComparison.OrdinalIgnoreCase))
                    {
                        CurrentShipCargo = cargo?.Inventory.Select(x =>
                        {
                            return new ShipCargo((string.IsNullOrEmpty(x.Name_Localised) ? x.Name : x.Name_Localised).ToTitleCase(), x.Count);
                        });

                        if (CurrentShipCargo != null)
                        {
                            ShipCargoUpdatedEvent?.Invoke(this, CurrentShipCargo);
                        }
                    }
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
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
                case DockedEvent.DockedEventArgs docked:
                    CurrentMarketID = docked.MarketID;
                    UpdateCurrentBody_Station(string.IsNullOrEmpty(docked.StationName_Localised) ? docked.StationName : docked.StationName_Localised);
                    if (CurrentSystem != null)
                        currentStation = new(docked, new(CurrentSystem), CurrentSystem);

                    if (IsLive == false)
                        break;

                    var stationName = docked.StationName_Localised ?? docked.StationName;
                    stationName = stationName.Replace("$EXT_PANEL_ColonisationShip;", "Colonisation Ship -");
                    var args = new NotificationArgs(stationName,
                        [
                            $"{EliteJournalReaderHelpers.StationTypeText(docked.StationType)}",
                            $"{docked.StationFaction.Name}",
                            $"{docked.StationGovernment_Localised ?? docked.StationGovernment}",
                            $"{docked.StationEconomy_Localised ?? docked.StationEconomy}",
                            $"{docked.LandingPads.LandPadText()}"
                        ],
                        NotificationOptions.Station);

                    notificationService.ShowBasicNotification(args);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    CurrentBody = null;
                    UpdateCurrentSystem(new(fsdJump));
                    UpdateCurrentBody_Station(null);
                    AddFactions(fsdJump.Factions);
                    if (fsdJump.Population == 0 || fsdJump.SystemFaction is null || fsdJump.Factions is null || fsdJump.Factions.Count == 0)
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
                            fsdJump.SystemFaction.FactionState,
                            fsdJump.SystemSecurity_Localised,
                            EliteHelpers.FactionReputationToString(fsdJump.Factions.FirstOrDefault(x => string.Equals(x.Name, fsdJump.SystemFaction.Name))?.MyReputation)
                        ]);
                    break;
                case LoadoutEvent.LoadoutEventArgs loadOut:
                    //Sometimes the ship name comes though as a string with a single space so we trim it
                    CurrentShipInfo = new ShipInfo(string.IsNullOrEmpty(loadOut.ShipName.Trim()) ? EliteHelpers.ConvertShipName(loadOut.Ship) : loadOut.ShipName, loadOut.ShipIdent, loadOut.CargoCapacity);

                    if (IsLive)
                        ShipChangedEvent?.Invoke(this, CurrentShipInfo);
                    break;
                case LocationEvent.LocationEventArgs location:
                    UpdateCurrentSystem(new(location));
                    string? bodyStation = null;
                    if (string.IsNullOrEmpty(location.Body) == false)
                    {
                        bodyStation = location.Body;
                    }
                    if (string.IsNullOrEmpty(location.StationName) == false)
                    {
                        bodyStation = location.StationName;
                    }
                    UpdateCurrentBody_Station(bodyStation);
                    AddFactions(location.Factions);

                    if (location.BodyType == BodyType.Planet && CurrentSystem != null)
                    {
                        var body = new SystemBody(location, CurrentSystem);

                        CurrentSystem.AddBody(body);
                    }

                    if (location.Population == 0 || location.SystemFaction is null || location.Factions is null || location.Factions.Count == 0)
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
                            location.SystemFaction.FactionState,
                            location.SystemSecurity_Localised,
                            EliteHelpers.FactionReputationToString(location.Factions.FirstOrDefault(x => string.Equals(x.Name, location.SystemFaction.Name))?.MyReputation)
                        ]);
                    break;
                case MarketBuyEvent.MarketBuyEventArgs marketBuy:
                    if (currentStation == null
                        || CurrentSystem == null)
                    {
                        break;
                    }
                    var commodity = EliteCommodityHelpers.GetCommodityFromPartial(marketBuy.Type, marketBuy.Type_Localised);

                    if (CurrentMarket is not null)
                    {
                        var marketItem = CurrentMarket.ItemsForSale.FirstOrDefault(x => string.Equals(x.Name, commodity.FdevName));

                        if (marketItem != null)
                        {
                            marketItem.Stock -= marketBuy.Count;

                            if (marketItem.Stock < 0)
                                marketItem.Stock = 0;

                            MarketEvent?.Invoke(this, CurrentMarket);
                        }
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

                    var known = WatchedMarkets.FirstOrDefault(x => x.MarketID == sell.MarketID);

                    if (known == null)
                        break;

                    var commod = EliteCommodityHelpers.GetCommodityFromPartial(sell.Type, string.IsNullOrEmpty(sell.Type_Localised) ? sell.Type : sell.Type_Localised);

                    var item = known.ItemsForPurchase.FirstOrDefault(x => string.Equals(x.Name, commod.FdevName));

                    if (item != null)
                    {
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
                    }
                    break;
                case EliteJournalReader.Events.MarketEvent.MarketEventArgs:
                    var market = journalManager.GetMarketInfo();

                    if (market != null)
                    {
                        CurrentMarket = new(market);
                        MarketEvent?.Invoke(this, CurrentMarket);

                        var watched = WatchedMarkets.FirstOrDefault(x => x.MarketID == market.MarketID);

                        if (watched != null)
                        {
                            watched.UpdatePurchaseOrders(market);
                            WatchedMarketUpdated?.Invoke(this, watched);
                        }
                    }
                    break;
                case PowerplayEvent.PowerplayEventArgs powerPlay:
                    commanderPower = powerPlay.Power;
                    break;
                case RedeemVoucherEvent.RedeemVoucherEventArgs redeemVoucher:

                    if (!string.Equals(redeemVoucher.Type, "Bounty", StringComparison.OrdinalIgnoreCase))
                        break;

                    var fireEvent = false;
                    foreach (var faction in redeemVoucher.Factions)
                    {
                        fireEvent = bountiesManager.FactionBountiesClaimed(faction, redeemVoucher.BrokerPercentage) | fireEvent;
                    }

                    if (fireEvent)
                        FireBountiesChangedIfLive();
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

                    notificationService.ShowShipTargetedNotification(pilotName, EliteHelpers.ConvertShipName(shipTargeted.Ship), targetType, shipTargeted.Bounty, shipTargeted.Faction, shipTargeted.Power);
                    break;
                case ScanEvent.ScanEventArgs scan:
                    if (CurrentSystem is null)
                        break;
                    CurrentSystem.AddBody(scan);
                    break;
                case SupercruiseEntryEvent.SupercruiseEntryEventArgs:
                    CurrentBody = null;
                    break;
                case UndockedEvent.UndockedEventArgs:
                    CurrentMarketID = 0;
                    currentStation = null;
                    UpdateCurrentBody_Station(null);
                    break;
                case DiedEvent.DiedEventArgs died:
                    bountiesManager.Reset();
                    break;
            }
        }

        #region Watched Markets
        public void AddRemoveWatchedMarket(ulong marketId)
        {
            var known = WatchedMarkets.FirstOrDefault(x => x.MarketID == marketId);

            if (known != null)
            {
                WatchedMarkets.Remove(known);
                WatchedMarketsUpdated?.Invoke(this,WatchedMarkets);
                return;
            }

            if (CurrentMarket != null && marketId == CurrentMarket.MarketID)
            {
                WatchedMarkets.Add(new WatchedMarket(CurrentMarket));
                WatchedMarketsUpdated?.Invoke(this, WatchedMarkets);
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

        private void UpdateCurrentSystem(StarSystem starSystem)
        {
            if (CurrentSystem != null && starSystem.Address == CurrentSystem.Address)
            {
                //Nothing to do here
                return;
            }

            CurrentSystem = AddSystem(starSystem);

            if (IsLive == false)
            {
                //No need to trigger events if parsing history
                return;
            }

            CurrentSystemChanged?.Invoke(this, CurrentSystem);
        }

        private StarSystem AddSystem(StarSystem starSystem)
        {
            if (Systems.TryGetValue(starSystem.Address, out var system))
            {
                system.UpdateSystem(starSystem);
                return system;
            }

            Systems.TryAdd(starSystem.Address, starSystem);
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
            bountiesManager.Initialise(currentCmdrId);
            CurrentMarket = null;
            WatchedMarkets.Clear();
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
            var cargo = journalManager.GetCargo();

            if (cargo != null && cargo.Vessel.Equals("Ship", StringComparison.OrdinalIgnoreCase))
            {
                CurrentShipCargo = cargo?.Inventory.Select(x => new ShipCargo((string.IsNullOrEmpty(x.Name_Localised) ? x.Name : x.Name_Localised).ToTitleCase(), x.Count));

                if (CurrentShipCargo != null)
                {
                    ShipCargoUpdatedEvent?.Invoke(this, CurrentShipCargo);
                }
            }

            var market = journalManager.GetMarketInfo();

            if (market != null)
            {
                CurrentMarket = new(market);
                MarketEvent?.Invoke(this, CurrentMarket);
            }

            var watchedMarkets = databaseProvider.GetWatchedMarkets();

            if (watchedMarkets != null)
            {
                WatchedMarkets = watchedMarkets;
            }

            IsLive = true;
        }

        public void Save()
        {
            databaseProvider.UpdateWatchedMarkets(WatchedMarkets);
        }

        public Tuple<IEnumerable<MaterialTrader>, IEnumerable<MaterialTrader>, IEnumerable<MaterialTrader>> GetNearestTraders()
        {
            //This should never happen if we have any logs
            if (CurrentSystem == null)
                return Tuple.Create(Enumerable.Empty<MaterialTrader>(), Enumerable.Empty<MaterialTrader>(), Enumerable.Empty<MaterialTrader>());

            return materialTraderService.GetNearestTraders(CurrentSystem.Position);
        }
    }
}
