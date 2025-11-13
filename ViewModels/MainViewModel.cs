using ODEliteTracker.Controls.Navigation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Navigation;
using ODMVVM.Navigation.Controls;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ODEliteTracker.ViewModels
{
    public sealed class MainViewModel : ODViewModel
    {
        public MainViewModel(IODNavigationService oDNavigationService,
                                      IManageJournalEvents journalManager,
                                      SharedDataStore sharedDataStore,
                                      NotificationService notificationService,
                                      SpanshCsvStore spanshCsv,
                                      SettingsStore settings,
                                      TickDataStore tickDataStore,
                                      FleetCarrierDataStore carrierDataStore,
                                      PopOutService popOutService)
        {
            navigationService = oDNavigationService;
            this.journalManager = journalManager;
            this.sharedData = sharedDataStore;
            this.notificationService = notificationService;
            this.spanshCsv = spanshCsv;
            this.settings = settings;
            this.tickDataStore = tickDataStore;
            this.carrierDataStore = carrierDataStore;
            this.popOutService = popOutService;
            this.sharedData.StoreLive += OnStoreLive;
            this.sharedData.CurrentSystemChanged += OnCurrentSystemChanged;
            this.sharedData.CurrentBody_StationChanged += OnCurrentBody_StationChanged;
            this.sharedData.ShipChangedEvent += OnCurrentShipChanged;

            this.journalManager.CommandersUpdated += OnCommandersUpdated;

            navigationService.CurrentViewLive += NavigationService_CurrentViewLive;

            ResetWindowPositionCommand = new ODRelayCommand(OnResetWindow);
            RefreshCommanderCommand = new ODRelayCommand(OnRefreshCommander);
            OpenSystemCommand = new ODRelayCommand(OnOpenSystemCommand);
            OpenStationCommand = new ODRelayCommand(OnOpenStationCommand);
            OpenShipyardCommand = new ODRelayCommand(OnOpenShipyardCommand);
        }



        public override bool IsLive => true;
        private readonly IODNavigationService navigationService;
        private readonly IManageJournalEvents journalManager;
        private readonly SharedDataStore sharedData;
        private readonly NotificationService notificationService;       
        private readonly SpanshCsvStore spanshCsv;
        private readonly SettingsStore settings;
        private readonly TickDataStore tickDataStore;
        private readonly FleetCarrierDataStore carrierDataStore;
        private readonly PopOutService popOutService;
        public EventHandler? WindowPositionReset;

        public string Title => $"Elite Tracker v{App.AppVersion}";

        public ObservableCollection<ODNavigationButton> MenuButtons { get; } =
        [
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/assassin.png", UriKind.Relative)),
                TargetView = typeof(MassacreMissionsViewModel),
                ToolTip = "Massacre Mission Stacker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/trade.png", UriKind.Relative)),
                TargetView = typeof(TradeMissionViewModel),
                ToolTip = "Trade Mission Stacker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/ColonisationBtn.png", UriKind.Relative)),
                TargetView = typeof(ColonisationViewModel),
                ToolTip = "Colonisation Tracker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/bgs.png", UriKind.Relative)),
                TargetView = typeof(BGSViewModel),
                ToolTip = "BGS Tracker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/powerplay.png", UriKind.Relative)),
                TargetView = typeof(PowerPlayViewModel),
                ToolTip = "Powerplay Tracker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/Asteroid.png", UriKind.Relative)),
                TargetView = typeof(MiningViewModel),
                ToolTip = "Mining Tracker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/fleetcarrier.png", UriKind.Relative)),
                TargetView = typeof(FleetCarrierViewModel),
                ToolTip = "Fleet Carrier Tracker"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/spanshbtn.png", UriKind.Relative)),
                TargetView = typeof(SpanshViewModel),
                ToolTip = "Spansh CSV Parser"
            },
        ];

        public ObservableCollection<UtilNavigationButton> UtilButtons { get; } =
        [
            new UiScaleButton()
        ];

        public ObservableCollection<ODNavigationButton> FooterButtons { get; } =
        [
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/PopOutBtn.png", UriKind.Relative)),
                TargetView = typeof(PopOutControlViewModel),
                ToolTip = "Overlays"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Notifications/monitor.png", UriKind.Relative)),
                TargetView = typeof(NotificationSettingsViewModel),
                ToolTip = "Notification Settings"
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/settings.png", UriKind.Relative)),
                TargetView = typeof(SettingsViewModel),
                ToolTip = "Settings"
            },
        ];

        public ObservableCollection<JournalCommanderVM> JournalCommanders { get; set; } = [];

        private JournalCommanderVM? selectedCommander;
        public JournalCommanderVM? SelectedCommander
        {
            get => selectedCommander;
            set
            {
                if (value == selectedCommander)
                    return;
                var currentId = selectedCommander?.Id ?? 0;
                selectedCommander = value;
                if (UiEnabled && selectedCommander != null && selectedCommander.Id != settings.SelectedCommanderID)
                {
                    popOutService.CloseViews(currentId);
                    settings.SelectedCommanderID = selectedCommander.Id;
                    _ = ChangeCommander();
                }

                OnPropertyChanged(nameof(SelectedCommander));
            }
        }

        public ODWindowPosition WindowPosition
        {
            get => settings.MainWindowPosition;
            set
            {
                settings.MainWindowPosition = value;
                OnPropertyChanged(nameof(WindowPosition));
            }
        }

        private bool uiEnabled;
        public bool UiEnabled
        {
            get => uiEnabled;
            set
            {
                if (uiEnabled == value) return;
                uiEnabled = value;
                OnPropertyChanged(nameof(UiEnabled));
            }
        }

        public string CurrentSystemName
        {
            get
            {
                return sharedData.CurrentSystem?.Name.ToUpper() ?? string.Empty;
            }
        }

        public string CurrentBody_Station
        {
            get
            {
                return sharedData.CurrentBody_Station?.ToUpper() ?? string.Empty;
            }
        }

        public string CurrentShipName
        {
            get
            {
                return sharedData.CurrentShipInfo?.Name ?? string.Empty;
            }
        }
        public double UiScale
        {
            get => settings.UiScale;
            set
            {
                if (settings.UiScale == value)
                    return;
                settings.UiScale = value;
                OnPropertyChanged(nameof(UiScale));
            }
        }

        public ICommand ResetWindowPositionCommand { get; }
        public ICommand RefreshCommanderCommand { get; }
        public ICommand OpenSystemCommand { get; }
        public ICommand OpenStationCommand { get; }
        public ICommand OpenShipyardCommand { get; }

        private void OnOpenShipyardCommand(object? obj)
        {
            if (sharedData.CurrentShipInfo == null)
                return;

            sharedData.CurrentShipInfo.OpenShipyard(settings.StatusBarSettings.ShipyardPreference);
        }
        private void OnOpenStationCommand(object? obj)
        {
            if (sharedData.CurrentSystem == null || sharedData.CurrentBody_Station == null)
                return;

            if (sharedData.CurrentMarketID > 0)
            {
                switch (settings.StatusBarSettings.StationBodyPreference)
                {
                    case Models.SystemWebSite.Inara:
                        ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/station/?search={sharedData.CurrentBody_Station.Replace(' ', '+')} [{sharedData.CurrentSystem.Name.Replace(' ', '+')}]");
                        break;
                    case Models.SystemWebSite.Spansh:
                        ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://spansh.co.uk/station/{sharedData.CurrentMarketID}");
                        break;
                }
                return;
            }

            if (sharedData.CurrentBody == null)
                return;

            switch (settings.StatusBarSettings.StationBodyPreference)
            {
                case Models.SystemWebSite.Inara:
                    ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/galaxy-starsystem/?search={sharedData.CurrentSystem.Name.Replace(' ', '+')}");
                    break;
                case Models.SystemWebSite.Spansh:
                    ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://spansh.co.uk/body/{sharedData.CurrentSystem.Address | (sharedData.CurrentBody.BodyID) << 55}");
                    break;
            }
        }

        private void OnOpenSystemCommand(object? obj)
        {
            if (sharedData.CurrentSystem == null)
                return;

            switch (settings.StatusBarSettings.SystemSitePreference)
            {
                case Models.SystemWebSite.Inara:
                    ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/galaxy-starsystem/?search={sharedData.CurrentSystem.Name.Replace(' ', '+')}");
                    break;
                case Models.SystemWebSite.Spansh:
                    ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://spansh.co.uk/system/{sharedData.CurrentSystem.Address}");
                    break;
            }            
        }
        public async Task UpdateCommanders()
        {
            if (journalManager is JournalManager manager)
            {
                await manager.UpdateCommanders();
                var cmdrs = manager.Commanders.Select(x => new JournalCommanderVM(x));

                JournalCommanders.ClearCollection();
                JournalCommanders.AddRange(cmdrs.Where(x => x.MigratedTo < 0));
                SelectedCommander = JournalCommanders.FirstOrDefault(x => x.Id == settings.SelectedCommanderID);
                OnPropertyChanged(nameof(JournalCommanders));
                OnPropertyChanged(nameof(SelectedCommander));
                await Task.Delay(500);
                await ChangeCommander().ConfigureAwait(true);
            }
        }
        private void OnResetWindow(object? obj)
        {            
            ODWindowPosition.ResetWindowPosition(WindowPosition);
            WindowPositionReset?.Invoke(this, EventArgs.Empty);
        }

        private void OnRefreshCommander(object? obj)
        {
            _ = ChangeCommander();
        }

        private void NavigationService_CurrentViewLive(object? sender, bool e)
        {
            UiEnabled = e;
        }

        public async Task Initialise()
        {
            UiEnabled = false;
            navigationService.NavigateTo<LoadingViewModel>();
            await tickDataStore.Initialise().ConfigureAwait(true);
            if (navigationService.CurrentView is LoadingViewModel loadingViewModel)
            {
                await Task.Run(journalManager.Initialise).ConfigureAwait(true);
            }
            //navigationService.NavigateTo(settings.CurrentViewModel);
            //OnPropertyChanged(nameof(CurrentSystemName));
            //OnPropertyChanged(nameof(CurrentBody_Station));
            //OnPropertyChanged(nameof(CurrentShipName));
            //UiEnabled = true;
        }

        public async Task ChangeCommander()
        {
            UiEnabled = false;
            navigationService.NavigateTo<LoadingViewModel>();
            if (navigationService.CurrentView is LoadingViewModel loadingViewModel)
            {
                loadingViewModel.StatusText = "Reading History";
                await Task.Run(journalManager.ChangeCommander).ConfigureAwait(true);
            }
            navigationService.NavigateTo(settings.CurrentViewModel);
            UiEnabled = true;
            OnPropertyChanged(nameof(CurrentSystemName));
            OnPropertyChanged(nameof(CurrentBody_Station));
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e && journalManager is JournalManager manager)
            {
              
                Application.Current.Dispatcher.Invoke(() => 
                {
                    navigationService.NavigateTo(settings.CurrentViewModel);
                    
                    var cmdrs = manager.Commanders.Select(x => new JournalCommanderVM(x));

                    JournalCommanders.ClearCollection();
                    JournalCommanders.AddRange(cmdrs.Where(x => x.MigratedTo < 0));
                    SelectedCommander = JournalCommanders.FirstOrDefault(x => x.Id == settings.SelectedCommanderID);
                    popOutService.OpenSavedViews(settings.SelectedCommanderID);
                    OnPropertyChanged(nameof(JournalCommanders));
                    OnPropertyChanged(nameof(SelectedCommander));
                    OnPropertyChanged(nameof(CurrentSystemName));
                    OnPropertyChanged(nameof(CurrentBody_Station));
                    OnPropertyChanged(nameof(CurrentShipName));
                    OnPropertyChanged(nameof(CurrentSystemName));
                    OnPropertyChanged(nameof(CurrentBody_Station));
                    OnPropertyChanged(nameof(CurrentShipName));
                    UiEnabled = true;
                }, DispatcherPriority.DataBind);
            }

        }

        private void OnCommandersUpdated(object? sender, EventArgs e)
        {
            OnStoreLive(null, true);
        }

        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            OnPropertyChanged(nameof(CurrentSystemName));
            OnPropertyChanged(nameof(CurrentBody_Station));
        }

        private void OnCurrentBody_StationChanged(object? sender, string? e)
        {
            OnPropertyChanged(nameof(CurrentBody_Station));
        }

        private void OnCurrentShipChanged(object? sender, ShipInfo? e)
        {
            OnPropertyChanged(nameof(CurrentShipName));
        }

        internal void OnClose()
        {
            journalManager.Shutdown();
            sharedData.Save();
            popOutService.CloseViews(settings.SelectedCommanderID);
            notificationService.Dispose();
        }
    }
}
