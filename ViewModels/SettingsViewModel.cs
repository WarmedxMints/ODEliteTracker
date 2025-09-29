using ODCapi.Services;
using ODEliteTracker.Models;
using ODEliteTracker.Stores;
using ODEliteTracker.Themes;
using ODEliteTracker.ViewModels.ModelViews;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Navigation;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class SettingsViewModel : ODViewModel
    {
        public SettingsViewModel(ThemeManager themeManager,
                                 SettingsStore setting,
                                 IODNavigationService navigationService,
                                 IODDatabaseProvider databaseProvider,
                                 IManageJournalEvents journalManager,
                                 CAPIService capiService)
        {
            this.themeManager = themeManager;
            this.setting = setting;
            this.navigationService = navigationService;
            this.databaseProvider = databaseProvider;
            this.journalManager = journalManager;
            this.capiService = capiService;

            SetTheme = new ODRelayCommand<Theme>(OnChangeTheme);
            OpenUrlCommand = new ODRelayCommand<string>(OpenUrl);

            ResetLastReadFile = new ODRelayCommand(OnResetLastFile, (_) => SelectedCommander != null && SelectedCommander?.Id != setting.SelectedCommanderID);
            ChangeJourneyDirectoryCommand = new ODRelayCommand(OnChangeJournalDirectory, (_) => SelectedCommander != null && SelectedCommander?.Id != setting.SelectedCommanderID);
            SaveCommanderChanges = new ODAsyncRelayCommand(OnSaveCommanderChanges, () => SelectedCommander != null);
            ReadNewDirectoryCommand = new ODAsyncRelayCommand(OnReadNewDirectory);
            DeleteCommander = new ODAsyncRelayCommand<Window?>(OnDeleteCommander, (_) => SelectedCommander?.Id != setting.SelectedCommanderID);
            ResetDataBaseCommand = new ODAsyncRelayCommand<Window?>(OnResetDataBase);
            CAPIButtonCommand = new ODAsyncRelayCommand(OnCAPIButtonPressed, () => CAPISate != State.AwaitingCallback);

            this.capiService.StateChange += OnCAPIStateChange;
            _ = LoadCommanders();
            OnModelLive(true);
        }

        public override void Dispose()
        {
            this.capiService.StateChange -= OnCAPIStateChange;
            base.Dispose();
        }

        private readonly ThemeManager themeManager;
        private readonly SettingsStore setting;
        private readonly IODNavigationService navigationService;
        private readonly IODDatabaseProvider databaseProvider;
        private readonly IManageJournalEvents journalManager;
        private readonly CAPIService capiService;

        public override bool IsLive => true;

        private State CAPISate => capiService.CurrentState;
        public string CAPIStatus => capiService.CurrentState switch
        {
             State.AwaitingCallback => "Awaiting CallBack",
             State.Authorised => $"CMDR {capiService.CommanderName}\nLogged In",
             State.NotAuthorised => "Authorisation Failed",
             _ => "Logged Out",
        };
         
        public string CAPIButtonText => capiService.CurrentState switch
        {            
            State.Authorised => "Log Out",
            _ => "Log In",
        };

        public Theme CurrentTheme => setting.CurrentTheme;
        public JournalLogAge LogAge
        {
            get
            {
                return setting.JournalAge;
            }
            set
            {
                setting.JournalAge = value;
                _ = ChangeJournalAge();
            }
        }

        public SystemWebSite SystemPreference
        {
            get => setting.StatusBarSettings.SystemSitePreference;
            set
            {
                setting.StatusBarSettings.SystemSitePreference = value;
                OnPropertyChanged(nameof(SystemPreference));
            }
        }
        public SystemWebSite StationBodyPreference
        {
            get => setting.StatusBarSettings.StationBodyPreference;
            set
            {
                setting.StatusBarSettings.StationBodyPreference = value;
                OnPropertyChanged(nameof(StationBodyPreference));
            }
        }
        public ShipyardWebSite ShipyardPrefence
        {
            get => setting.StatusBarSettings.ShipyardPreference;
            set
            {
                setting.StatusBarSettings.ShipyardPreference = value;
                OnPropertyChanged(nameof(ShipyardPrefence));
            }
        }

        private ObservableCollection<JournalCommanderVM> journalCommanderViewModelCollection = [];
        public ObservableCollection<JournalCommanderVM> JournalCommanderViews
        {
            get => journalCommanderViewModelCollection;
            set => journalCommanderViewModelCollection = value;
        }

        private JournalCommanderVM? selectedCommander;
        public JournalCommanderVM? SelectedCommander
        {
            get => selectedCommander;
            set
            {
                selectedCommander = value;
                OnPropertyChanged(nameof(SelectedCommander));
            }
        }

        #region Commands
        public ICommand SetTheme { get; }
        public ICommand OpenUrlCommand { get; }
        public ICommand ResetLastReadFile { get; }
        public ICommand ChangeJourneyDirectoryCommand { get; }
        public ICommand SaveCommanderChanges { get; }
        public ICommand DeleteCommander { get; }
        public ICommand ReadNewDirectoryCommand { get; }
        public ICommand ResetDataBaseCommand { get; }
        public ICommand CAPIButtonCommand { get; }
        #endregion

        #region Command Methods
        private async Task OnCAPIButtonPressed()
        {
            var activeCommander = JournalCommanderViews.FirstOrDefault(x => x.Id == setting.SelectedCommanderID);

            if (activeCommander == null)
            {
                return;
            }

            switch (capiService.CurrentState)
            {
                case State.NotAuthorised:
                case State.LoggedOut:
                    await capiService.Login(activeCommander.Name, activeCommander.Name.Contains("(Legacy)"));
                    break;
                case State.AwaitingCallback:
                    return;
                case State.Authorised:
                    capiService.LogOut();
                    break;
                default:
                    break;
            }

            OnPropertyChanged(nameof(CAPIStatus));
            OnPropertyChanged(nameof(CAPIButtonText));
            OnPropertyChanged(nameof(CAPISate));
            OnPropertyChanged(nameof(CAPIButtonCommand));
        }

        private void OnChangeTheme(Theme theme)
        {
            setting.CurrentTheme = theme;
            themeManager.SetTheme(theme);
            OnPropertyChanged(nameof(CurrentTheme));
        }

        private void OpenUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return;
            ODMVVM.Helpers.OperatingSystem.OpenUrl(url);
        }

        private void OnChangeJournalDirectory(object? obj)
        {
            if (SelectedCommander == null)
            {
                return;
            }
            var directory = ODDialogService.DirectorySelectDialog(SelectedCommander.JournalPath);

            if (string.IsNullOrEmpty(directory) == false && SelectedCommander != null)
            {
                SelectedCommander.JournalPath = directory;
                SelectedCommander.LastFile = string.Empty;
                _= OnSaveCommanderChanges();
            }
        }

        private void OnResetLastFile(object? obj)
        {
            if (SelectedCommander != null)
                SelectedCommander.LastFile = string.Empty;
        }

        private async Task OnDeleteCommander(Window? window)
        {
            if (SelectedCommander is null)
                return;

            var result = ODDialogService.ShowWithOwner(window, "Delete CMDR?", $"Delete CMDR {SelectedCommander.Name}?", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                navigationService.NavigateTo<LoadingViewModel>();
                await Task.Factory.StartNew(() => databaseProvider.DeleteCommander(SelectedCommander.Id)).ConfigureAwait(true);
                await journalManager.UpdateCommanders();
                navigationService.NavigateTo<SettingsViewModel>();
            }
        }

        private async Task OnReadNewDirectory()
        {
            var directory = ODDialogService.DirectorySelectDialog();

            if (string.IsNullOrEmpty(directory) == false)
            {
                navigationService.NavigateTo<LoadingViewModel>();
                await journalManager.ReadNewDirectory(directory);
                navigationService.NavigateTo<SettingsViewModel>();
            }
        }

        private async Task OnSaveCommanderChanges()
        {
            if (JournalCommanderViews == null || JournalCommanderViews.Count == 0)
                return;

            foreach (var commander in JournalCommanderViews)
            {
                databaseProvider.AddCommander(new(commander.Id, commander.Name, commander.JournalPath, commander.LastFile, commander.IsHidden, commander.UseCAPI), true, true);
            }
            await journalManager.UpdateCommanders();
        }

        private async Task OnResetDataBase(Window? window)
        {
            var dialog = ODDialogService.ShowWithOwner(window, "Reset Database?", "This will delete all commanders and their records.\nAre you sure?", MessageBoxButton.YesNo);

            if (dialog == MessageBoxResult.No)
                return;

            JournalCommanderViews.ClearCollection();
            SelectedCommander = null;
            OnPropertyChanged(nameof(JournalCommanderViews));
            OnPropertyChanged(nameof(SelectedCommander));
            setting.SelectedCommanderID = 0;
            navigationService.NavigateTo<LoadingViewModel>();
            if(navigationService.CurrentView is LoadingViewModel viewModel)
            {
                viewModel.StatusText = "Resetting Database...";
                await journalManager.ResetDatabase();
            }
            navigationService.NavigateTo<SettingsViewModel>();
        }
        #endregion

        private void OnCAPIStateChange(object? sender, State e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((ODAsyncRelayCommand)CAPIButtonCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(CAPIStatus));
                OnPropertyChanged(nameof(CAPIButtonText));
            });
        }

        private async Task ChangeJournalAge()
        {
            navigationService.NavigateTo<LoadingViewModel>();
            await journalManager.ChangeCommander();
            navigationService.NavigateTo<SettingsViewModel>();
        }

        private async Task LoadCommanders()
        {
            var commanders = await databaseProvider.GetAllJournalCommanders(true);

            var vms = commanders.Select(x => new JournalCommanderVM(x));

            JournalCommanderViews.ClearCollection();

            foreach (var commander in vms)
            {
                JournalCommanderViews.AddItem(commander);
            }

            SelectedCommander = JournalCommanderViews.FirstOrDefault(x => x.Id == setting.SelectedCommanderID);
            OnPropertyChanged(nameof(SelectedCommander));
            OnPropertyChanged(nameof(JournalCommanderViews));
        }
    }
}
