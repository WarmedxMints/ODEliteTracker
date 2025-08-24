using ODEliteTracker.Controls.Navigation;
using ODEliteTracker.Models.Galaxy;
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

            this.journalManager.CommandersUpdated += OnCommandersUpdated;

            navigationService.CurrentViewLive += NavigationService_CurrentViewLive;

            ResetWindowPositionCommand = new ODRelayCommand(OnResetWindow);
            RefreshCommanderCommand = new ODRelayCommand(OnRefreshCommander);
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
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/trade.png", UriKind.Relative)),
                TargetView = typeof(TradeMissionViewModel)
            },
            new EliteStyleNavigationButton() 
            { 
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/ColonisationBtn.png", UriKind.Relative)), 
                TargetView = typeof(ColonisationViewModel)
            },            
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/bgs.png", UriKind.Relative)),
                TargetView = typeof(BGSViewModel)
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/powerplay.png", UriKind.Relative)),
                TargetView = typeof(PowerPlayViewModel)
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/Asteroid.png", UriKind.Relative)),
                TargetView = typeof(MiningViewModel)
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/fleetcarrier.png", UriKind.Relative)),
                TargetView = typeof(FleetCarrierViewModel)
            },
             new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/spanshbtn.png", UriKind.Relative)),
                TargetView = typeof(SpanshViewModel)
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
            },
            new EliteStyleNavigationButton()
            {
                ButtonImage = new BitmapImage(new Uri("/Assets/Notifications/monitor.png", UriKind.Relative)),
                TargetView = typeof(NotificationSettingsViewModel),
            },
            new EliteStyleNavigationButton() 
            { 
                ButtonImage = new BitmapImage(new Uri("/Assets/Icons/settings.png", UriKind.Relative)),
                TargetView = typeof(SettingsViewModel), 
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
                if(uiEnabled == value) return;
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

        public double UiScale 
        {
            get => settings.UiScale;
            set
            {
                if(settings.UiScale == value) 
                    return;
                settings.UiScale = value;
                OnPropertyChanged(nameof(UiScale));
            }
        }

        public ICommand ResetWindowPositionCommand { get; }
        public ICommand RefreshCommanderCommand { get; }

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
            navigationService.NavigateTo(settings.CurrentViewModel);
            OnPropertyChanged(nameof(CurrentSystemName));
            OnPropertyChanged(nameof(CurrentBody_Station));
            UiEnabled = true;
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
            if(e && journalManager is JournalManager manager)
            {
                Dispatcher.CurrentDispatcher.Invoke(new Action(() =>
                {
                    var cmdrs = manager.Commanders.Select(x => new JournalCommanderVM(x));

                    JournalCommanders.ClearCollection();
                    JournalCommanders.AddRange(cmdrs);
                    SelectedCommander = JournalCommanders.FirstOrDefault(x => x.Id == settings.SelectedCommanderID);
                    popOutService.OpenSavedViews(settings.SelectedCommanderID);
                    OnPropertyChanged(nameof(JournalCommanders));
                    OnPropertyChanged(nameof(SelectedCommander));
                    OnPropertyChanged(nameof(CurrentSystemName));
                    OnPropertyChanged(nameof(CurrentBody_Station));
                }), DispatcherPriority.DataBind);
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

        internal void OnClose()
        {
            journalManager.Shutdown();
            sharedData.Save();
            popOutService.CloseViews(settings.SelectedCommanderID);
            notificationService.Dispose();
        }
    }
}
