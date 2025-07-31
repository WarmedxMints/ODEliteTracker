using ODEliteTracker.Models;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.FleetCarrier;
using ODMVVM.Commands;
using ODMVVM.Utils;
using ODMVVM.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class FleetCarrierViewModel : ODViewModel
    {
        public FleetCarrierViewModel(FleetCarrierDataStore dataStore, SettingsStore settings)
        {
            this.dataStore = dataStore;
            this.settings = settings;
            this.dataStore.StoreLive += OnStoreLive;
            this.dataStore.CarrierUpdated += OnCarrierUpdated;
            this.dataStore.CarrierStockUpdated += OnCarrierStockUpdated;
            this.dataStore.OnCarrierTimeTick += DataStore_OnCarrierTimeTick;
            this.dataStore.CarrierDestinationUpdated += OnCarrierDestinationUpdated;

            RefreshCarrierStockCommand = new ODAsyncRelayCommand(OnRefreshCarrierStock, () => CanCallCAPI);

            if (this.dataStore.CanCallCAPI == false)
            {
                StartCAPITimer();
            }
            if (this.dataStore.IsLive)
                OnStoreLive(null, true);
        }

        public override void Dispose()
        {
            this.dataStore.StoreLive -= OnStoreLive;
            this.dataStore.CarrierUpdated -= OnCarrierUpdated;
            this.dataStore.CarrierStockUpdated -= OnCarrierStockUpdated;
            this.dataStore.OnCarrierTimeTick -= DataStore_OnCarrierTimeTick;
            this.dataStore.CarrierDestinationUpdated -= OnCarrierDestinationUpdated;
        }

        private readonly FleetCarrierDataStore dataStore;
        private readonly SettingsStore settings;
        private CountdownTimer? capiTimer;
        public override bool IsLive => dataStore.IsLive;
        private bool CanCallCAPI => dataStore.CanCallCAPI;

        public GridSize GridSize => settings.CarrierSettings.GridSize;
        public CarrierCommoditySorting CommoditySorting
        {
            get => settings.CarrierSettings.Sorting;
            set
            {
                settings.CarrierSettings.Sorting = value;
                OnPropertyChanged(nameof(CarrierStock));
                OnPropertyChanged(nameof(SellOrders));
            }
        }

        public FleetCarrierVM? CarrierData { get; set; } = null;

        public IEnumerable<CarrierCommodityVM>? CarrierStock
        {
            get
            {
                if (CarrierData == null)
                    return null;

                return CommoditySorting switch
                {
                    CarrierCommoditySorting.Category => CarrierData.Stock.Where(x => x.StockCount > 0 || x.DemandValue > 0).OrderBy(x => x.Category).ThenBy(x => x.Name),
                    _ => CarrierData.Stock.Where(x => x.StockCount > 0 || x.DemandValue > 0).OrderBy(x => x.Name),
                };
            }
        }

        public IEnumerable<CarrierCommodityVM>? SellOrders
        {
            get
            {
                if (CarrierData == null)
                    return null;

                return CommoditySorting switch
                {
                    CarrierCommoditySorting.Category => CarrierData.Stock.Where(x => x.SalePriceValue > 0).OrderBy(x => x.Category).ThenBy(x => x.Name),
                    _ => CarrierData.Stock.Where(x => x.SalePriceValue > 0).OrderBy(x => x.Name),
                };
            }

        }
        public ICommand RefreshCarrierStockCommand { get; }

        private async Task OnRefreshCarrierStock()
        {
            StartCAPITimer();
            await dataStore.UpdateCarrierCargo();
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (dataStore.CarrierData != null)
                    {
                        CarrierData = new(dataStore.CarrierData);
                    }
                    OnPropertyChanged(nameof(CarrierData));
                    OnModelLive(true);
                });
            }
        }

        private void OnCarrierUpdated(object? sender, FleetCarrier e)
        {
            if (CarrierData == null)
                return;

            CarrierData.UpdateData(e);
        }

        private void OnCarrierStockUpdated(object? sender, FleetCarrier e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CarrierData?.UpdateStock(e);
                OnPropertyChanged(nameof(CarrierData));
                OnPropertyChanged(nameof(CarrierStock));
                OnPropertyChanged(nameof(SellOrders));
            });
        }

        private void DataStore_OnCarrierTimeTick(object? sender, string e)
        {
            if (CarrierData == null)
                return;

            CarrierData.UpdateTimes();
        }

        private void OnCarrierDestinationUpdated(object? sender, FleetCarrier e)
        {
            if (CarrierData == null)
                return;

            CarrierData.UpdateDestination(e);
        }

        private void StartCAPITimer()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(CanCallCAPI));
                capiTimer?.Stop();
                capiTimer = new(new(0, 15, 0), new(0, 0, 1));

                capiTimer.OnTick += CapiTimer_OnTick;
                capiTimer.CountDownFinishedEvent += CapiTimer_CountDownFinishedEvent;
                capiTimer.Start();
            });
        }

        private void CapiTimer_CountDownFinishedEvent(object? sender, EventArgs e)
        {
            capiTimer?.Stop();
            capiTimer = null;

            if (dataStore.CanCallCAPI == false)
            {
                StartCAPITimer();
                return;
            }
        }

        private void CapiTimer_OnTick(object? sender, string e)
        {
            if (dataStore.CanCallCAPI == false)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                capiTimer?.Stop();
                capiTimer = null;
                ((ODAsyncRelayCommand)RefreshCarrierStockCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CanCallCAPI));
            });
        }
    }
}
