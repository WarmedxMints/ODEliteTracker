using ODEliteTracker.Helpers;
using ODEliteTracker.Models;
using ODEliteTracker.Models.FleetCarrier;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.FleetCarrier;
using ODMVVM.Commands;
using ODMVVM.Services.MessageBox;
using ODMVVM.Utils;
using ODMVVM.ViewModels;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class FleetCarrierViewModel : ODViewModel
    {
        public FleetCarrierViewModel(FleetCarrierDataStore dataStore, SettingsStore settings, NotificationService notificationService)
        {
            this.dataStore = dataStore;
            this.settings = settings;
            this.notificationService = notificationService;
            this.dataStore.StoreLive += OnStoreLive;
            this.dataStore.CarrierUpdated += OnCarrierUpdated;
            this.dataStore.SquadCarrierUpdated += OnSquadCarrierUpdated;
            this.dataStore.CarrierStockUpdated += OnCarrierStockUpdated;
            this.dataStore.OnCarrierTimeTick += DataStore_OnCarrierTimeTick;
            this.dataStore.OnSquadCarrierTimeTick += DataStore_OnSquadCarrierTimeTick;
            this.dataStore.CarrierDestinationUpdated += OnCarrierDestinationUpdated;
            this.dataStore.SquadCarrierDestinationUpdated += OnSquadCarrierDestinationUpdated;

            RefreshCarrierStockCommand = new ODAsyncRelayCommand(OnRefreshCarrierStock, () => CanCallCAPI);
            CopyToClipboard = new ODRelayCommand<string>(OnCopyToClipboard);
            SetAudioFile = new ODRelayCommand(OnSetAudioFile);
            ClearAudioFile = new ODRelayCommand(OnClearAudioFile);
            PlayAudioFile = new ODAsyncRelayCommand(OnPlayAudioFile, () => CanPlay);

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
            this.dataStore.SquadCarrierUpdated -= OnSquadCarrierUpdated;
            this.dataStore.CarrierStockUpdated -= OnCarrierStockUpdated;
            this.dataStore.OnCarrierTimeTick -= DataStore_OnCarrierTimeTick;
            this.dataStore.OnSquadCarrierTimeTick -= DataStore_OnSquadCarrierTimeTick;
            this.dataStore.CarrierDestinationUpdated -= OnCarrierDestinationUpdated;
            this.dataStore.SquadCarrierDestinationUpdated -= OnSquadCarrierDestinationUpdated;
        }

        private readonly FleetCarrierDataStore dataStore;
        private readonly SettingsStore settings;
        private readonly NotificationService notificationService;
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

        public bool PlayOnPeronalCarrierCooldown
        {
            get => settings.FleetCarrierSettings.PlayOnPersonalCarrierCooldown;
            set
            {
                settings.FleetCarrierSettings.PlayOnPersonalCarrierCooldown = value;
                OnPropertyChanged(nameof(PlayOnPeronalCarrierCooldown));
            }
        }

        public string? AudioFile => string.IsNullOrEmpty(settings.FleetCarrierSettings.TimerAudioFile) ?
            "System Beep" :
            Path.GetFileName(settings.FleetCarrierSettings.TimerAudioFile);

        public bool PlayOnSquadronCarrierCooldown
        {
            get => settings.FleetCarrierSettings.PlayOnSquadronCarrierCooldown;
            set
            {
                settings.FleetCarrierSettings.PlayOnSquadronCarrierCooldown = value;
                OnPropertyChanged(nameof(PlayOnSquadronCarrierCooldown));
            }
        }

        public float Volume
        {
            get => settings.FleetCarrierSettings.Volume;
            set
            {
                settings.FleetCarrierSettings.Volume = value;
                OnPropertyChanged(nameof(Volume));
            }
        }

        private bool canPlay = true;
        public bool CanPlay
        {
            get => canPlay;
            set
            {
                canPlay = value;
                OnPropertyChanged(nameof(CanPlay));
            }
        }

        public FleetCarrierVM? CarrierData { get; set; } = null;
        public FleetCarrierVM? SquadCarrierData { get; set; } = null;

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
        public ICommand CopyToClipboard { get; }
        public ICommand SetAudioFile { get; }
        public ICommand ClearAudioFile { get; }
        public ICommand PlayAudioFile { get; }

        private async Task OnRefreshCarrierStock()
        {
            StartCAPITimer();
            await dataStore.UpdateCarrierCargo();
        }

        private void OnCopyToClipboard(string obj)
        {
            if (string.IsNullOrEmpty(obj))
                return;

            notificationService.SetClipboard(obj);
        }

        private void OnSetAudioFile(object? obj)
        {
            var file = ODDialogService.OpenFileDialog("Select wave or mp3 file", "wav | mp3", "Sound files (*.wav, *.mp3)|*.wav;*.mp3");

            if (string.IsNullOrEmpty(file))
                return;

            settings.FleetCarrierSettings.TimerAudioFile = file;
            
            OnPropertyChanged(nameof(AudioFile));
        }

        private void OnClearAudioFile(object? obj)
        {
            settings.FleetCarrierSettings.TimerAudioFile = null;

            OnPropertyChanged(nameof(AudioFile));
        }

        private async Task OnPlayAudioFile()
        {
            CanPlay = false;
            await AudioPlayer.PlayFile(settings.FleetCarrierSettings.TimerAudioFile, settings.FleetCarrierSettings.Volume).ConfigureAwait(true);
            CanPlay = true;
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

                    if (dataStore.SquadCarrierData != null)
                    {
                        SquadCarrierData = new(dataStore.SquadCarrierData);
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
        private void OnSquadCarrierUpdated(object? sender, FleetCarrier e)
        {
            if (SquadCarrierData == null)
                return;

            SquadCarrierData.UpdateData(e);
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

        private void DataStore_OnSquadCarrierTimeTick(object? sender, string e)
        {
            if (SquadCarrierData == null)
                return;

            SquadCarrierData.UpdateTimes();
        }

        private void OnCarrierDestinationUpdated(object? sender, FleetCarrier e)
        {
            if (CarrierData == null)
                return;

            CarrierData.UpdateDestination(e);
        }

        private void OnSquadCarrierDestinationUpdated(object? sender, FleetCarrier e)
        {
            if (SquadCarrierData == null)
                return;

            SquadCarrierData.UpdateDestination(e);
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
