using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Helpers;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Market;
using ODEliteTracker.Notifications;
using ODEliteTracker.Services;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Helpers;
using ODMVVM.Utils;

namespace ODEliteTracker.Stores
{
    public sealed class FleetCarrierDataStore : LogProcessorBase
    {
        public FleetCarrierDataStore(IManageJournalEvents journalManager,
                                     SharedDataStore sharedData,
                                     NotificationService notificationService,
                                     SettingsStore settings)
        {
            this.journalManager = (JournalManager)journalManager;

            if (this.journalManager != null)
            {
                _ = this.journalManager.RegisterLogProcessor(this);
            }

            this.sharedData = sharedData;
            this.notificationService = notificationService;
            this.settings = settings;
            this.sharedData.MarketEvent += OnMarketEvent;
            fleetCarrierTimer.CountDownFinishedEvent += FleetCarrierTimer_CountDownFinishedEvent;
            squadFleetCarrierTimer.CountDownFinishedEvent += SquadFleetCarrierTimer_CountDownFinishedEvent;
        }

        private readonly JournalManager? journalManager;
        private readonly SharedDataStore sharedData;
        private readonly NotificationService notificationService;
        private readonly SettingsStore settings;
        private readonly CountdownTimer fleetCarrierTimer = new(new(0, 20, 0), new(0, 0, 1));
        private readonly CountdownTimer squadFleetCarrierTimer = new(new(0, 20, 0), new(0, 0, 1));
        private FleetCarrier? personalCarrierData;
        private FleetCarrier? squadCarrierData;

        private ulong dockedMarketId;

        public CarrierType SwitchableCarrierType
        {
            get => settings.CarrierSettings.SwitchableCarrierType;
            set
            {
                settings.CarrierSettings.SwitchableCarrierType = value;
                if (SwitchableCarrierData != null)
                    SwitchableCarrierStockUpdated?.Invoke(this, SwitchableCarrierData);
            }
        }

        public FleetCarrier? SwitchableCarrierData => SwitchableCarrierType switch
        {
            CarrierType.Personal => PersonalCarrierData,
            CarrierType.Squadron => SquadCarrierData,
            _ => null,
        };

        public FleetCarrier? PersonalCarrierData => personalCarrierData;
        public FleetCarrier? SquadCarrierData => squadCarrierData;

        public EventHandler<FleetCarrier>? PersonalCarrierUpdated;
        public EventHandler<FleetCarrier>? SquadCarrierUpdated;
        public EventHandler<FleetCarrier>? PersonalCarrierStockUpdated;
        public EventHandler<FleetCarrier>? SquadCarrierStockUpdated;
        public EventHandler<FleetCarrier>? SwitchableCarrierStockUpdated;
        public EventHandler<FleetCarrier>? PersonalCarrierDestinationUpdated;
        public EventHandler<FleetCarrier>? SquadCarrierDestinationUpdated;

        public bool CanCallCAPI => journalManager?.CanCallCAPI ?? false;

        #region Events
        public event EventHandler<string>? OnPersonalCarrierTimeTick
        {
            add { fleetCarrierTimer.OnTick += value; }
            remove { fleetCarrierTimer.OnTick -= value; }
        }

        public event EventHandler<bool>? OnCarrierTimerRunning
        {
            add { fleetCarrierTimer.OnTimerRunning += value; }
            remove { fleetCarrierTimer.OnTimerRunning -= value; }
        }

        public event EventHandler? OnCarrierTimerFinished
        {
            add { fleetCarrierTimer.CountDownFinishedEvent += value; }
            remove { fleetCarrierTimer.CountDownFinishedEvent -= value; }
        }

        public event EventHandler<string>? OnSquadCarrierTimeTick
        {
            add { squadFleetCarrierTimer.OnTick += value; }
            remove { squadFleetCarrierTimer.OnTick -= value; }
        }

        public event EventHandler<bool>? OnSquadCarrierTimerRunning
        {
            add { squadFleetCarrierTimer.OnTimerRunning += value; }
            remove { squadFleetCarrierTimer.OnTimerRunning -= value; }
        }

        public event EventHandler? OnSquadCarrierTimerFinished
        {
            add { squadFleetCarrierTimer.CountDownFinishedEvent += value; }
            remove { squadFleetCarrierTimer.CountDownFinishedEvent -= value; }
        }
        #endregion

        #region Logprocessor Implementation
        public override string StoreName => "Fleet Carrier";
        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.CarrierStats, true },
                { JournalTypeEnum.CarrierLocation, true },
                { JournalTypeEnum.CarrierJumpRequest, true },
                { JournalTypeEnum.CarrierJumpCancelled, true },
                { JournalTypeEnum.CarrierTradeOrder, false },
                { JournalTypeEnum.CarrierCrewServices, true },
                { JournalTypeEnum.CarrierBankTransfer, true },
                { JournalTypeEnum.CarrierDepositFuel, true },
                { JournalTypeEnum.Docked, true },
                { JournalTypeEnum.Undocked, true },
                { JournalTypeEnum.CargoTransfer, false },
            };
        }

        public override void ClearData()
        {
            personalCarrierData = null;
            dockedMarketId = 0;
            fleetCarrierTimer.Stop();

            if (this.journalManager != null)
            {
                this.journalManager.CAPILive -= async (s, e) => await OnCAPILive(s, e).ConfigureAwait(false);
            }
        }

        public override void Dispose()
        {
            if (this.journalManager != null)
            {
                this.journalManager.CAPILive -= async (s, e) => await OnCAPILive(s, e);
            }
            this.sharedData.MarketEvent -= OnMarketEvent;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case CarrierStatsEvent.CarrierStatsEventArgs carrierStats:
                    if (GetCarrier(carrierStats.CarrierID, out var stats, out var statsType, carrierStats.Callsign))
                    {
                        UpdateCarrierDataFromStatsEven(ref stats, carrierStats);

                        if (!IsLive)
                        {
                            break;
                        }

                        switch (statsType)
                        {
                            case CarrierType.Unkown:
                                break;
                            case CarrierType.Personal:
                                if (stats != null)
                                    PersonalCarrierUpdated?.Invoke(this, stats);
                                break;
                            case CarrierType.Squadron:
                                if (stats != null)
                                    SquadCarrierUpdated?.Invoke(this, stats);
                                break;
                        }
                    }
                    if (string.IsNullOrEmpty(carrierStats?.CarrierType))
                        return;
                    if (carrierStats.CarrierType.Equals("FleetCarrier"))
                    {
                        UpdateCarrierDataFromStatsEven(ref personalCarrierData, carrierStats);
                        break;
                    }
                    if (carrierStats.CarrierType.Equals("SquadronCarrier"))
                    {
                        UpdateCarrierDataFromStatsEven(ref squadCarrierData, carrierStats);
                        break;
                    }
                    break;
                case CarrierLocationEvent.CarrierLocationEventArgs cLocation:
                    if (!GetCarrier(cLocation.CarrierID, out var locationCar, out var locType))
                        return;

                    locationCar.StarSystem = cLocation.StarSystem;
                    locationCar.SystemAddress = cLocation.SystemAddress;
                    locationCar.BodyID = cLocation.BodyID;
                    locationCar.Destination.Reset();

                    if (!IsLive)
                    {
                        break;
                    }

                    switch (locType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            PersonalCarrierUpdated?.Invoke(this, locationCar);
                            break;
                        case CarrierType.Squadron:
                            SquadCarrierUpdated?.Invoke(this, locationCar);
                            //Carrier location event is fired when logging in so check that isn't the case
                            if (squadFleetCarrierTimer.TimerRunning || cLocation.Timestamp - sharedData.LastLoginTime < TimeSpan.FromSeconds(30))
                                break;

                            var notificationTime = (cLocation.Timestamp - DateTime.UtcNow) + TimeSpan.FromMinutes(5);

                            if (notificationTime <= TimeSpan.Zero)
                                break;

                            //Jump request only fires for the person who set the jump
                            //So if we get a new location for the 
                            squadFleetCarrierTimer.UpdateRuntime(notificationTime);
                            squadFleetCarrierTimer.Start();
                            break;
                    }
                    break;
                case CarrierJumpRequestEvent.CarrierJumpRequestEventArgs jumpRequest:
                    if (!GetCarrier(jumpRequest.CarrierID, out var jumpCar, out var jumpType) || !IsLive)
                        break;

                    jumpCar.Destination = new(jumpRequest.SystemName, jumpRequest.Body, jumpRequest.SystemAddress, jumpRequest.DepartureTime);

                    var timespan = (jumpRequest.DepartureTime - DateTime.UtcNow) + TimeSpan.FromMinutes(5);

                    switch (jumpType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            if (timespan > TimeSpan.Zero)
                            {
                                fleetCarrierTimer.UpdateRuntime(timespan);
                                fleetCarrierTimer.Start();
                            }
                            PersonalCarrierDestinationUpdated?.Invoke(this, jumpCar);
                            break;
                        case CarrierType.Squadron:
                            if (timespan > TimeSpan.Zero)
                            {
                                squadFleetCarrierTimer.UpdateRuntime(timespan);
                                squadFleetCarrierTimer.Start();
                            }

                            SquadCarrierDestinationUpdated?.Invoke(this, jumpCar);
                            break;
                    }
                    break;
                case CarrierJumpCancelledEvent.CarrierJumpCancelledEventArgs jCancelled:
                    if (!GetCarrier(jCancelled.CarrierID, out var cancelledCar, out var canType))
                        break;

                    cancelledCar.Destination = new();

                    switch (canType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            fleetCarrierTimer.Stop();
                            if (IsLive)
                            {
                                PersonalCarrierDestinationUpdated?.Invoke(this, cancelledCar);
                            }
                            break;
                        case CarrierType.Squadron:
                            squadFleetCarrierTimer.Stop();
                            if (IsLive)
                            {
                                SquadCarrierDestinationUpdated?.Invoke(this, cancelledCar);
                            }
                            break;
                    }
                    break;
                case CarrierTradeOrderEvent.CarrierTradeOrderEventArgs carrierTradeOrder:
                    if (carrierTradeOrder.BlackMarket)
                        break;
                    if (!GetCarrier(carrierTradeOrder.CarrierID, out var trade, out var orderType))
                        break;
                    switch (orderType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            ProcessCarrierTradeOrder(ref trade, carrierTradeOrder, CarrierType.Personal);
                            break;
                        case CarrierType.Squadron:
                            ProcessCarrierTradeOrder(ref trade, carrierTradeOrder, CarrierType.Squadron);
                            break;
                    }
                    break;
                case CarrierCrewServicesEvent.CarrierCrewServicesEventArgs carrierCrewServicesEventArgs:
                    if (!GetCarrier(carrierCrewServicesEventArgs.CarrierID, out var crew, out var crewType))
                        break;
                    if (!crew.UpdateCrew(carrierCrewServicesEventArgs) && IsLive)
                        return;
                    switch (crewType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            PersonalCarrierUpdated?.Invoke(this, crew);
                            break;
                        case CarrierType.Squadron:
                            SquadCarrierUpdated?.Invoke(this, crew);
                            break;
                    }
                    break;
                case CarrierBankTransferEvent.CarrierBankTransferEventArgs bank:
                    if (!GetCarrier(bank.CarrierID, out var bankCarrier, out var bankType))
                        break;

                    bankCarrier.Balance = bank.CarrierBalance;

                    switch (bankType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            PersonalCarrierUpdated?.Invoke(this, bankCarrier);
                            break;
                        case CarrierType.Squadron:
                            SquadCarrierUpdated?.Invoke(this, bankCarrier);
                            break;
                    }
                    break;
                case CarrierDepositFuelEvent.CarrierDepositFuelEventArgs fuel:
                    if (!GetCarrier(fuel.CarrierID, out var refueled, out var fuelType))
                        break;
                    refueled.FuelLevel = fuel.Total;

                    if (!IsLive)
                        break;
                    switch (fuelType)
                    {
                        case CarrierType.Unkown:
                            break;
                        case CarrierType.Personal:
                            PersonalCarrierUpdated?.Invoke(this, refueled);
                            break;
                        case CarrierType.Squadron:
                            SquadCarrierUpdated?.Invoke(this, refueled);
                            break;
                    }
                    break;
                case DockedEvent.DockedEventArgs docked:
                    dockedMarketId = docked.MarketID;
                    break;
                case UndockedEvent.UndockedEventArgs:
                    dockedMarketId = 0;
                    break;
                case CargoTransferEvent.CargoTransferEventArgs cargoTransfer:
                    if (GetCarrier(dockedMarketId, out var carrier, out var transferType) == false || cargoTransfer.Transfers == null || cargoTransfer.Transfers.Count <= 0)
                        break;

                    var cargoMoved = false;

                    foreach (var transfer in cargoTransfer.Transfers)
                    {
                        var direction = 1;

                        switch (transfer.Direction)
                        {
                            case CargoTransferDirection.ToCarrier:
                                direction = 1;
                                break;
                            case CargoTransferDirection.ToShip:
                                direction = -1;
                                break;
                            default:
                                continue;
                        }

                        cargoMoved = TransferCargo(ref carrier, transfer.Type, transfer.Type, transfer.Count * direction, stolen: false) || cargoMoved;
                    }

                    if (cargoMoved)
                    {
                        FireCarrierStockUpdatedEventsIfLive(transferType);
                    }
                    break;
            }
        }
        private bool GetCarrier(ulong id, out FleetCarrier carrier, out CarrierType carrierType, string callsign = "")
        {
            if (personalCarrierData?.CarrierID == id)
            {
                carrier = personalCarrierData;
                carrierType = CarrierType.Personal;
                return true;
            }
            if (squadCarrierData?.CarrierID == id)
            {
                carrier = squadCarrierData;
                carrierType = CarrierType.Squadron;
                return true;
            }
            //If we've got this far, maybe we only have the squadron carrier from the CAPI which exludes to ID for some unknown reason
            if (squadCarrierData != null && string.Equals(squadCarrierData.Callsign, callsign))
            {
                carrier = squadCarrierData;
                carrierType = CarrierType.Squadron;
                return true;
            }
            //Maybe it's the personal carrier, though this should never happen
            if (personalCarrierData != null && string.Equals(personalCarrierData.Callsign, callsign))
            {
                carrier = personalCarrierData;
                carrierType = CarrierType.Personal;
                return true;
            }
            carrier = new();
            carrierType = CarrierType.Unkown;
            return false;
        }
        private bool ProcessCarrierTradeOrder(ref FleetCarrier? carrierData, CarrierTradeOrderEvent.CarrierTradeOrderEventArgs carrierTradeOrder, CarrierType carrierType)
        {
            if (carrierData == null)
                return false;

            var commodity = EliteCommodityHelpers.GetCommodityFromPartial(carrierTradeOrder.Commodity, string.IsNullOrEmpty(carrierTradeOrder.Commodity_Localised) ? carrierTradeOrder.Commodity : carrierTradeOrder.Commodity_Localised);

            var known = carrierData.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == false);

            if (carrierTradeOrder.CancelTrade)
            {
                if (known != null)
                {
                    known.BuyOrderCount = 0;
                    known.SalePrice = 0;
                    if (known.StockCount <= 0)
                    {
                        carrierData.Stock.Remove(known);
                    }
                    FireCarrierStockUpdatedEventsIfLive(carrierType);
                }
                return false;
            }
            var salePrice = carrierTradeOrder.SaleOrder > 0 ? carrierTradeOrder.Price : 0;
            if (known == null)
            {
                known = new CarrierCommodity(commodity, false, salePrice);
                carrierData.Stock.Add(known);
            }
            known.SalePrice = salePrice;
            known.BuyOrderCount = carrierTradeOrder.PurchaseOrder;

            FireCarrierStockUpdatedEventsIfLive(carrierType);
            return true;
        }

        private static void UpdateCarrierDataFromStatsEven(ref FleetCarrier? carrier, CarrierStatsEvent.CarrierStatsEventArgs eventData)
        {
            carrier ??= new(eventData);
            carrier.FuelLevel = eventData.FuelLevel;
            carrier.Name = eventData.Name;
            carrier.DockingAccess = eventData.DockingAccess;
            carrier.AllowNotorious = eventData.AllowNotorious;
            carrier.FuelLevel = eventData.FuelLevel;
            carrier.Balance = eventData.Finance.CarrierBalance;
            carrier.CarrierID = eventData.CarrierID;
            carrier.Callsign = eventData.Callsign;
            carrier.AssignCrew(eventData.Crew);
            carrier.TotalCapacity = eventData.SpaceUsage.TotalCapacity;
            carrier.FreeSpace = eventData.SpaceUsage.FreeSpace;
        }

        public override void RunAfterParsingHistory()
        {
            if (this.journalManager != null)
            {
                this.journalManager.CAPILive += async (s, e) => await OnCAPILive(s, e).ConfigureAwait(false);

                if (this.journalManager.CAPIIsLive)
                {
                    _ = OnCAPILive(null, true);
                }
            }

            if (personalCarrierData != null && personalCarrierData.Destination.Arrived)
            {
                personalCarrierData.Destination = new();
            }

            if (squadCarrierData != null && squadCarrierData.Destination.Arrived)
            {
                squadCarrierData.Destination = new();
            }

            base.RunAfterParsingHistory();
        }
        #endregion

        private void FleetCarrierTimer_CountDownFinishedEvent(object? sender, EventArgs e)
        {
            var args = new NotificationArgs("Fleet Carrier", ["Carrier Cooldown Complete"], Models.Settings.NotificationOptions.FleetCarrierReady);

            notificationService.ShowBasicNotification(args);

            if (settings.FleetCarrierSettings.PlayOnPersonalCarrierCooldown)
            {
                _ = AudioPlayer.PlayFile(settings.FleetCarrierSettings.TimerAudioFile, settings.FleetCarrierSettings.Volume);
            }
        }

        private void SquadFleetCarrierTimer_CountDownFinishedEvent(object? sender, EventArgs e)
        {
            var args = new NotificationArgs("Sqaudron Fleet Carrier", ["Carrier Cooldown Complete"], Models.Settings.NotificationOptions.SqaudCarrierReady);

            notificationService.ShowBasicNotification(args);

            if (settings.FleetCarrierSettings.PlayOnSquadronCarrierCooldown)
            {
                _ = AudioPlayer.PlayFile(settings.FleetCarrierSettings.TimerAudioFile, settings.FleetCarrierSettings.Volume);
            }
        }

#if DEBUG
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
        private async Task OnCAPILive(object? s, bool e)
        {
            if (e == false)
                return;

            await UpdatePersonalCarrierCargo();
            await UpdateSquadronCarrierCargo();
        }

        public async Task UpdatePersonalCarrierCargo()
        {
            if (journalManager == null)
            {
                return;
            }
            var capiCarrier = await journalManager.GetCAPICarrier();
            await journalManager.GetCAPISquadron();
            if (capiCarrier == null || personalCarrierData != null && personalCarrierData.CarrierID != capiCarrier.Market.Id)
            {
                return;
            }
            capiCarrier.Capacity.totalCapacity = 25000;
            UpdateCarrier(ref personalCarrierData, capiCarrier);

            if (personalCarrierData == null)
                return;
            FireCarrierStockUpdatedEventsIfLive(CarrierType.Personal);
        }

        public async Task UpdateSquadronCarrierCargo()
        {
            if (journalManager == null)
            {
                return;
            }
            var squadron = await journalManager.GetCAPISquadron();

            if (squadron == null || squadron.SquadronCarrier == null)
            {
                return;
            }
            squadron.SquadronCarrier.Capacity.totalCapacity = 60000;
            UpdateCarrier(ref squadCarrierData, squadron.SquadronCarrier);

            if (squadCarrierData == null)
                return;

            FireCarrierStockUpdatedEventsIfLive(CarrierType.Squadron);
        }

        private void FireCarrierStockUpdatedEventsIfLive(CarrierType carrierType)
        {
            if (!IsLive || carrierType == CarrierType.Unkown)
                return;

            switch (carrierType)
            {
                case CarrierType.Personal:
                    if (personalCarrierData == null)
                        return;
                    PersonalCarrierUpdated?.Invoke(this, personalCarrierData);
                    PersonalCarrierStockUpdated?.Invoke(this, personalCarrierData);
                    if (carrierType == SwitchableCarrierType)
                        SwitchableCarrierStockUpdated?.Invoke(this, personalCarrierData);
                    break;
                case CarrierType.Squadron:
                    if (squadCarrierData == null)
                        return;
                    SquadCarrierUpdated?.Invoke(this, squadCarrierData);
                    SquadCarrierStockUpdated?.Invoke(this, squadCarrierData);
                    if (carrierType == SwitchableCarrierType)
                        SwitchableCarrierStockUpdated?.Invoke(this, squadCarrierData);
                    break;
            }
        }
        private static void UpdateCarrier(ref FleetCarrier? carrier, ODCapi.Models.CAPIFleetCarrier capiCarrier)
        {
            carrier ??= new(capiCarrier);

            carrier.FuelLevel = capiCarrier.Fuel;
            carrier.Balance = capiCarrier.Balance;
            carrier.StarSystem = capiCarrier.CurrentStarSystem;
            var cargo = new List<CarrierCommodity>();

            foreach (var item in capiCarrier.Cargo)
            {
                var value = EliteCommodityHelpers.GetCommodityFromPartial(item.Commodity, item.LocalName);

                var known = cargo.FirstOrDefault(x => x.commodity == value && item.Stolen == x.Stolen);

                if (known == null)
                {
                    known = new(value, item.Stolen, 0);
                    cargo.Add(known);
                }
                known.StockCount += item.Qty;
            }

            foreach (var item in capiCarrier.Orders.Commodities.Sales)
            {
                //We don't care about black market items
                if (item.BlackMarket)
                    continue;

                var value = EliteCommodityHelpers.GetCommodityFromPartial(item.Name, item.Name);

                var known = cargo.FirstOrDefault(x => x.commodity == value);

                if (known == null)
                {
                    known = new(value, false, item.Price);
                    cargo.Add(known);
                }

                known.BuyOrderCount += item.Outstanding;
            }

            foreach (var item in capiCarrier.Orders.Commodities.Purchases)
            {
                //We don't care about black market items
                if (item.BlackMarket)
                    continue;
                var value = EliteCommodityHelpers.GetCommodityFromPartial(item.Name, item.Name);

                var known = cargo.FirstOrDefault(x => x.commodity == value);

                if (known == null)
                {
                    known = new(value, false, 0);
                    cargo.Add(known);
                }

                known.BuyOrderCount += item.Outstanding;
            }

            carrier.Stock = cargo;
        }

        private static bool TransferCargo(ref FleetCarrier carrier, string type, string typeLocalised, int value, bool stolen)
        {
            if (carrier == null)
                return false;

            var commodity = EliteCommodityHelpers.GetCommodityFromPartial(type, typeLocalised);

            var known = carrier.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == stolen);

            if (known != null)
            {
                known.StockCount += value;
                //if (known.StockCount <= 0 && known.SalePrice <= 0)
                //    CarrierData?.Stock.Remove(known);
                return true;
            }

            if (known == null && value > 0)
            {
                carrier.Stock.Add(new CarrierCommodity(commodity, stolen, 0) { StockCount = value });
                return true;
            }
            //If we are still null then check for stolen shite on board as the transfere event doesn't tell us if it is
            known = carrier.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == true);
            if (known != null)
            {
                known.StockCount += value;
                if (known.StockCount <= 0)
                    carrier?.Stock.Remove(known);
                return true;
            }
            return false;
        }

        private void OnMarketEvent(object? sender, StationMarket? e)
        {
            if (e == null)
                return;
            if (!GetCarrier(e.MarketID, out var carrier, out var carrierType))
                return;

            var stockUpdated = false;

            foreach (var item in e.ItemsForSale)
            {
                var commodity = EliteCommodityHelpers.GetCommodityDetails(item.Name);

                var known = carrier.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == false);

                if (known == null)
                {
                    known = new CarrierCommodity(commodity, stolen: false, item.SellPrice);
                    carrier.Stock.Add(known);
                }

                known.StockCount = item.Stock;
                stockUpdated = true;

                //If the item is now out of stock
                //if (known.StockCount <= 0)
                //{
                //    carrierData.Stock.Remove(known);
                //}
            }

            //Make a copy of the current carrier items for purchase
            var carrierPurchases = carrier.Stock.Where(x => x.BuyOrderCount > 0).ToList();

            foreach (var item in e.ItemsForPurchase)
            {
                var commodity = EliteCommodityHelpers.GetCommodityDetails(item.Name);

                var inStock = carrier.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == false);
                //If we somehow missed the purchase order creation
                if (inStock == null)
                {
                    inStock = new CarrierCommodity(commodity, stolen: false, 0)
                    {
                        BuyOrderCount = item.Demand
                    };
                    carrier.Stock.Add(inStock);
                    stockUpdated = true;
                }

                //The amount the order was set for minus the amount in demand now
                var stockToAdd = inStock.BuyOrderCount - item.Demand;
                inStock.BuyOrderCount = item.Demand;

                //remove the item from our copied list
                carrierPurchases.Remove(inStock);
                //If nothing has been sold, carry on
                if (stockToAdd <= 0)
                {
                    continue;
                }
                inStock.StockCount += stockToAdd;
                stockUpdated = true;
            }

            //If we still have outstanding purchases that were not found in the market event
            //then another commander must have filled the order
            if (carrierPurchases.Count > 0)
            {
                foreach (var item in carrierPurchases)
                {
                    item.StockCount += item.BuyOrderCount;
                    item.BuyOrderCount = 0;
                }
                stockUpdated = true;
            }

            if (IsLive && stockUpdated)
            {
                switch (carrierType)
                {
                    case CarrierType.Unkown:
                        return;
                    case CarrierType.Personal:
                        PersonalCarrierStockUpdated?.Invoke(this, carrier);
                        break;
                    case CarrierType.Squadron:
                        SquadCarrierStockUpdated?.Invoke(this, carrier);
                        break;
                }
                SwitchableCarrierStockUpdated?.Invoke(this, carrier);
            }
        }
    }
}
