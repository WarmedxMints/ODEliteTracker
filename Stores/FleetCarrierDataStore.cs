using EliteJournalReader;
using EliteJournalReader.Events;
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
        public FleetCarrierDataStore(IManageJournalEvents journalManager, SharedDataStore sharedData, NotificationService notificationService) 
        {
            this.journalManager = (JournalManager)journalManager;

            if (this.journalManager != null)
            {
                _ = this.journalManager.RegisterLogProcessor(this);
            }

            this.sharedData = sharedData;
            this.notificationService = notificationService;
            this.sharedData.MarketEvent += OnMarketEvent;
            fleetCarrierTimer.CountDownFinishedEvent += FleetCarrierTimer_CountDownFinishedEvent;
        }

        private readonly JournalManager? journalManager;
        private readonly SharedDataStore sharedData;
        private readonly NotificationService notificationService;
        private readonly CountdownTimer fleetCarrierTimer = new(new(0, 20, 0), new(0, 0, 1));
        private FleetCarrier? carrierData;
        private bool dockedOnCarrier = false;

        public FleetCarrier? CarrierData => carrierData;

        public EventHandler<FleetCarrier>? CarrierUpdated;
        public EventHandler<FleetCarrier>? CarrierStockUpdated;
        public EventHandler<FleetCarrier>? CarrierDestinationUpdated;

        public bool CanCallCAPI => journalManager?.CanCallCAPI ?? false;

        #region Events
        public event EventHandler<string>? OnCarrierTimeTick
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
                { JournalTypeEnum.MarketBuy, false },
                { JournalTypeEnum.MarketSell, false },
            };
        }

        public override void ClearData()
        {
            carrierData = null;
            dockedOnCarrier = false;
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
                case CarrierStatsEvent.CarrierStatsEventArgs carrier:
                    carrierData ??= new(carrier);
                    carrierData.FuelLevel = carrier.FuelLevel;
                    carrierData.Name = carrier.Name;
                    carrierData.DockingAccess = carrier.DockingAccess;
                    carrierData.AllowNotorious = carrier.AllowNotorious;
                    carrierData.FuelLevel = carrier.FuelLevel;
                    carrierData.Balance = carrier.Finance.CarrierBalance;
                    carrierData.CarrierID = carrier.CarrierID;
                    carrierData.Callsign = carrier.Callsign;

                    carrierData.AssignCrew(carrier.Crew);

                    if(IsLive)
                    {
                        CarrierUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierLocationEvent.CarrierLocationEventArgs cLocation:
                    if(carrierData != null)
                    {
                        carrierData.StarSystem = cLocation.StarSystem;
                        carrierData.SystemAddress = cLocation.SystemAddress;
                        carrierData.BodyID = cLocation.BodyID;
                        carrierData.Destination.Reset();
                        if (IsLive)
                        {
                            CarrierUpdated?.Invoke(this, carrierData);
                        }
                    }
                    break;
                case CarrierJumpRequestEvent.CarrierJumpRequestEventArgs jumpRequest:
                    if (carrierData is null)
                        break;
                    carrierData.Destination = new(jumpRequest.SystemName, jumpRequest.Body, jumpRequest.SystemAddress, jumpRequest.DepartureTime);

                    var span = (jumpRequest.DepartureTime - DateTime.UtcNow) + TimeSpan.FromMinutes(5);

                    if (span > TimeSpan.Zero)
                    {
                        fleetCarrierTimer.UpdateRuntime(span);
                        fleetCarrierTimer.Start();
                    }

                    if (IsLive)
                    {
                        CarrierDestinationUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierJumpCancelledEvent.CarrierJumpCancelledEventArgs:
                    if (carrierData is null)
                        break;
                    carrierData.Destination = new();
                    fleetCarrierTimer.Stop();
                    if (IsLive)
                    {
                        CarrierDestinationUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierTradeOrderEvent.CarrierTradeOrderEventArgs carrierTradeOrder:
                    if (carrierData is null || carrierTradeOrder.BlackMarket)
                        break;

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
                            if (IsLive)
                            {
                                CarrierStockUpdated?.Invoke(this, carrierData);
                            }
                        }
                        break;
                    }
                    var salePrice = carrierTradeOrder.SaleOrder > 0 ? carrierTradeOrder.Price : 0;
                    if (known == null)
                    {
                        known = new CarrierCommodity(commodity, false, salePrice);
                        carrierData.Stock.Add(known);
                    }
                    known.SalePrice = salePrice;
                    known.BuyOrderCount = carrierTradeOrder.PurchaseOrder;

                    if(IsLive)
                    {
                        CarrierStockUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierCrewServicesEvent.CarrierCrewServicesEventArgs carrierCrewServicesEventArgs:
                    if (carrierData is null)
                        break;

                    carrierData.UpdateCrew(carrierCrewServicesEventArgs);

                    if (IsLive)
                    {
                        CarrierUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierBankTransferEvent.CarrierBankTransferEventArgs bank:
                    if (carrierData is null)
                        break;

                    carrierData.Balance = bank.CarrierBalance;

                    if (IsLive)
                    {
                        CarrierUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case CarrierDepositFuelEvent.CarrierDepositFuelEventArgs fuel:
                    if (carrierData is null)
                        break;

                    carrierData.FuelLevel = fuel.Total;

                    if (IsLive)
                    {
                        CarrierUpdated?.Invoke(this, carrierData);
                    }
                    break;
                case DockedEvent.DockedEventArgs docked:
                    dockedOnCarrier = docked.MarketID == carrierData?.CarrierID;
                    break;
                case UndockedEvent.UndockedEventArgs:
                    dockedOnCarrier = false;
                    break;
                case CargoTransferEvent.CargoTransferEventArgs cargoTransfer:
                    if (dockedOnCarrier == false || CarrierData == null || cargoTransfer.Transfers == null || cargoTransfer.Transfers.Count <= 0)
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

                        cargoMoved = TransferCargo(transfer.Type, transfer.Type, transfer.Count * direction, stolen: false) || cargoMoved;
                    }

                    if(IsLive && cargoMoved)
                        CarrierStockUpdated?.Invoke(this, CarrierData);
                    break;
                case MarketBuyEvent.MarketBuyEventArgs buy:
                    if (dockedOnCarrier == false || CarrierData is null || CarrierData.CarrierID != buy.MarketID)
                        break;

                    if (TransferCargo(buy.Type, buy.Type_Localised ?? buy.Type, buy.Count * -1, stolen: false) && IsLive)
                    {
                        CarrierStockUpdated?.Invoke(this, CarrierData);
                    }
                    break;
                case MarketSellEvent.MarketSellEventArgs sell:
                    if (dockedOnCarrier == false || CarrierData is null || CarrierData.CarrierID != sell.MarketID || sell.BlackMarket)
                        break;

                    var commod = EliteCommodityHelpers.GetCommodityFromPartial(sell.Type, string.IsNullOrEmpty(sell.Type_Localised) ? sell.Type : sell.Type_Localised);

                    var inStock = CarrierData.Stock.FirstOrDefault(x => x.commodity == commod && x.Stolen == false);

                    if(inStock != null)
                    {
                        inStock.StockCount += sell.Count;
                        inStock.BuyOrderCount -= sell.Count;

                        if (IsLive)
                        {
                            CarrierStockUpdated?.Invoke(this, CarrierData);
                        }
                    }
                    break;
            }
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

            if(carrierData != null && carrierData.Destination.Arrived)
            {
                carrierData.Destination = new();
            }

            base.RunAfterParsingHistory();
        }
        #endregion

        private void FleetCarrierTimer_CountDownFinishedEvent(object? sender, EventArgs e)
        {
            var args = new NotificationArgs("Fleet Carrier", ["Carrier Cooldown Complete"], Models.Settings.NotificationOptions.FleetCarrierReady);

            notificationService.ShowBasicNotification(args);
        }

#if DEBUG
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
        private async Task OnCAPILive(object? s, bool e)
#if DEBUG
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
        {
            if (e == false)
                return;
#if !DEBUG
            await UpdateCarrierCargo();
#endif
        }

        public async Task UpdateCarrierCargo()
        {
            if (journalManager == null)
            {
                return;
            }
            var capiCarrier = await journalManager.GetCarrier();

            if (capiCarrier == null || carrierData != null && carrierData.CarrierID != capiCarrier.Market.Id)
            {
                return;
            }

            carrierData ??= new(capiCarrier);

            carrierData.FuelLevel = capiCarrier.Fuel;
            carrierData.Balance = carrierData.Balance;

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

                if(known == null)
                {
                    known = new(value, false, 0);
                    cargo.Add(known);
                }

                known.BuyOrderCount += item.Outstanding;
            }

            carrierData.Stock = cargo;
            CarrierStockUpdated?.Invoke(this, carrierData);
        }

        private bool TransferCargo(string type, string typeLocalised, int value, bool stolen)
        {
            if (carrierData == null)
                return false;

            var commodity = EliteCommodityHelpers.GetCommodityFromPartial(type, typeLocalised);

            var known = carrierData.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == stolen);

            if (known != null)
            {
                known.StockCount += value;
                if (known.StockCount <= 0 && known.SalePrice <= 0)
                    CarrierData?.Stock.Remove(known);
                return true;
            }

            if (known == null && value > 0)
            {
                carrierData.Stock.Add(new CarrierCommodity(commodity, stolen, 0) { StockCount = value });
                return true;
            }
            //If we are still null then check for stolen shite on board as the transfere event doesn't tell us if it is
            known = carrierData.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == true);
            if (known != null)
            {               
                known.StockCount += value;
                if (known.StockCount <= 0)
                    CarrierData?.Stock.Remove(known);
                return true;
            }
            return false;
        }

        private void OnMarketEvent(object? sender, StationMarket? e)
        {
            if(carrierData == null || e == null || e.MarketID != carrierData.CarrierID) 
            { 
                return; 
            }

            var stockUpdated = false;

            foreach (var item in e.ItemsForSale)
            {
                var commodity = EliteCommodityHelpers.GetCommodityDetails(item.Name);

                var known = carrierData.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == false);

                if (known == null)
                {
                    known = new CarrierCommodity(commodity, stolen: false, item.SellPrice);
                    carrierData.Stock.Add(known);
                }

                known.StockCount = item.Stock;
                stockUpdated = true;     
                
                //If the item is now out of stock
                //if(known.StockCount <= 0)
                //{
                //    carrierData.Stock.Remove(known);
                //}
            }

            //Make a copy of the current carrier items for purchase
            var carrierPurchases = carrierData.Stock.Where(x => x.BuyOrderCount > 0).ToList();

            foreach (var item in e.ItemsForPurchase)
            {
                var commodity = EliteCommodityHelpers.GetCommodityDetails(item.Name);

                var inStock = carrierData.Stock.FirstOrDefault(x => x.commodity == commodity && x.Stolen == false);
                //If we somehow missed the purchase order creation
                if (inStock == null)
                {
                    inStock = new CarrierCommodity(commodity, stolen: false, 0)
                    {
                        BuyOrderCount = item.Demand
                    };
                    carrierData.Stock.Add(inStock);
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
                inStock.StockCount = inStock.StockCount + stockToAdd;
                stockUpdated = true;
            }

            //If we still have outstanding purchases that were not found in the market event
            //then another commander must have filled the order
            if(carrierPurchases.Count > 0)
            {
                foreach (var item in carrierPurchases)
                {
                    item.StockCount += item.BuyOrderCount;
                    item.BuyOrderCount = 0;
                }
                stockUpdated = true;
            }

            if (IsLive && CarrierData != null && stockUpdated)
                CarrierStockUpdated?.Invoke(this, CarrierData);
        }
    }
}
