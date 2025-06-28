using ODEliteTracker.Models.PowerPlay;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.PowerPlay;
using ODMVVM.Commands;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class PowerPlayViewModel : ODViewModel
    {
        public PowerPlayViewModel(PowerPlayDataStore dataStore, SettingsStore settings, NotificationService notification)
        {
            this.dataStore = dataStore;
            this.settings = settings;
            this.notification = notification;
            this.dataStore.StoreLive += OnStoreLive;
            this.dataStore.PledgeDataUpdated += OnPledgeDataUpdated;
            this.dataStore.CyclesUpdated += OnCyclesUpdated;
            this.dataStore.SystemAdded += OnSystemAdded;
            this.dataStore.SystemUpdated += OnSystemUpdated;
            this.dataStore.MeritsEarned += OnMeritsEarned;
            this.dataStore.SystemCycleUpdated += OnSystemUpdated;

            SetSelectedSystemCommand = new ODRelayCommand<PowerPlaySystemVM>(OnSetSelectedSystem);
            CreateDiscordPostCommand = new ODRelayCommand(OnCreateDiscordPost);
            OpenInaraCommand = new ODRelayCommand(OnOpenInara);

            currentCycleNo = EliteHelpers.CurrentCycleNo();

            if (this.dataStore.IsLive)
                OnStoreLive(null, true);
        }

        private readonly PowerPlayDataStore dataStore;
        private readonly SettingsStore settings;
        private readonly NotificationService notification;
        private int currentCycleNo;

        private PledgeDataVM? pledgeData;
        public PledgeDataVM? PledgeData
        {
            get => pledgeData;
            set
            {
                pledgeData = value;
                OnPropertyChanged(nameof(PledgeData));
            }
        }

        private int tabIndex;
        public int TabIndex
        {
            get => tabIndex;
            set
            {
                tabIndex = value;
                SetSelectedSystem();
                OnPropertyChanged(nameof(TabIndex));
                OnPropertyChanged(nameof(CurrentCycle));
                OnPropertyChanged(nameof(SelectedSystemData));
            }
        }
        public override bool IsLive => dataStore.IsLive;

        public bool HideSystemsWithoutMerits
        {
            get => settings.PowerPlaySettings.HideSystemsWithoutMerits;
            set
            {
                settings.PowerPlaySettings.HideSystemsWithoutMerits = value;
                OnPropertyChanged(nameof(HideSystemsWithoutMerits));
                OnPropertyChanged(nameof(LastCycleSystems));
                OnPropertyChanged(nameof(ThisCycleSystems));
            }
        }

        private string discordButtonText = "Create Post";
        public string DiscordButtonText
        {
            get => discordButtonText;
            set
            {
                discordButtonText = value;
                OnPropertyChanged(nameof(DiscordButtonText));
            }
        }

        public List<PowerPlaySystemVM>? lastCycleSystems { get; private set; }
        public IEnumerable<PowerPlaySystemVM>? LastCycleSystems
        {
            get
            {
                if(HideSystemsWithoutMerits)
                {
                    return lastCycleSystems?.Where(x => x.MeritsEarned(dataStore.PreviousCycle));
                }
                return lastCycleSystems;
            }
        }
        public List<PowerPlaySystemVM>? thisCycleSystems { get; private set; }
        public IEnumerable<PowerPlaySystemVM>? ThisCycleSystems
        {
            get
            {
                if (HideSystemsWithoutMerits)
                {
                    return thisCycleSystems?.Where(x => x.MeritsEarned(dataStore.CurrentCycle));
                }
                return thisCycleSystems;
            }
        }
        private PowerPlaySystemVM? selectedSystem;
        public PowerPlaySystemVM? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                selectedSystem = value;

                OnPropertyChanged(nameof(SelectedSystem));
                OnPropertyChanged(nameof(SelectedSystemData));
            }
        }

        public PowerPlayCycleDataVM? SelectedSystemData
        {
            get
            {
                if (selectedSystem is null)
                    return null;

                if (selectedSystem.Data.TryGetValue(GetSelectedCycle(), out var data))
                    return data;
                return null;
            }
        }

        public string CurrentCycle => $"Cycle 2.{currentCycleNo - tabIndex}";

        public ICommand SetSelectedSystemCommand { get; }
        public ICommand CreateDiscordPostCommand { get; }
        public ICommand OpenInaraCommand { get; }

        private DateTime GetSelectedCycle()
        {
            if (tabIndex == 0)
                return dataStore.CurrentCycle;
            return dataStore.PreviousCycle;
        } 

        private void OnSetSelectedSystem(PowerPlaySystemVM model)
        {
            SelectedSystem = model;
        }

        private void OnCyclesUpdated(object? sender, EventArgs e)
        {
            currentCycleNo = EliteHelpers.CurrentCycleNo();
            UpdateSystems(null);
            OnPropertyChanged(nameof(CurrentCycle));
        }

        private void OnMeritsEarned(object? sender, int e)
        {
            UpdateSystems(null);
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                UpdateSystems(dataStore.CurrentSystem);

                if (dataStore.PledgeData != null)
                {
                    OnPledgeDataUpdated(null, dataStore.PledgeData);
                }
            }
            OnModelLive(e);
        }

        private void OnSystemAdded(object? sender, PowerPlaySystem e)
        {
            //TODO add the system
            UpdateSystems(e);
        }

        private void OnSystemUpdated(object? sender, PowerPlaySystem e)
        {
            //TODO update the properties instead
            UpdateSystems(e);
        }

        private void UpdateSystems(PowerPlaySystem? system)
        {
            lastCycleSystems = dataStore.Systems.Where(x => x.CycleData.ContainsKey(dataStore.PreviousCycle) /* && x.MeritsEarned > 0*/)
                                                .OrderBy(x => x.Name)
                                                .Select(x => new PowerPlaySystemVM(x)).ToList();

            thisCycleSystems = dataStore.Systems.Where(x => x.CycleData.ContainsKey(dataStore.CurrentCycle)/* && x.MeritsEarned > 0*/)
                                                .OrderBy(x => x.Name)
                                                .Select(x => new PowerPlaySystemVM(x)).ToList();

            if (system != null)
            {
                SelectedSystem = ThisCycleSystems?.FirstOrDefault(x => x.Address == system.Address);
                tabIndex = 0;
            }
            if (SelectedSystem == null)
            {
                SelectedSystem = ThisCycleSystems?.FirstOrDefault();
                tabIndex = 0;
            }
            if (SelectedSystem == null)
            {
                SelectedSystem = LastCycleSystems?.FirstOrDefault();
                tabIndex = 1;
            }

            OnPropertyChanged(nameof(TabIndex));
            OnPropertyChanged(nameof(lastCycleSystems));
            OnPropertyChanged(nameof(thisCycleSystems));
        }


        private void OnPledgeDataUpdated(object? sender, PledgeData? e)
        {
            if (e != null)
            {
                PledgeData = new(e);
                return;
            }
            PledgeData = null;
        }

        private void SetSelectedSystem()
        {
            if (SelectedSystem is null)
            {
                SelectedSystem = TabIndex == 0 ? ThisCycleSystems?.FirstOrDefault() : LastCycleSystems?.FirstOrDefault();
                return;
            }

            if (TabIndex == 0)
            {
                SelectedSystem = ThisCycleSystems?.FirstOrDefault(x => x.Address == SelectedSystem.Address) ?? ThisCycleSystems?.FirstOrDefault();
                return;
            }

            SelectedSystem = LastCycleSystems?.FirstOrDefault(x => x.Address == SelectedSystem.Address) ?? LastCycleSystems?.FirstOrDefault();
        }

        private void OnCreateDiscordPost(object? obj)
        {
            var cycleDate = TabIndex == 0 ? dataStore.CurrentCycle : dataStore.PreviousCycle;

            var data = TabIndex == 0 ? thisCycleSystems?.Where(x => x.MeritsEarned(cycleDate)) : lastCycleSystems?.Where(x => x.MeritsEarned(cycleDate));

            if(data == null || !data.Any())
            {
                DiscordButtonText = "No Data";
                Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                return;
            }
            if (Helpers.DiscordPostCreator.CreatePowerPlayPost(data, CurrentCycle, cycleDate))
            {
                DiscordButtonText = "Post Created"; 
                notification.ShowBasicNotification(new("Clipboard", ["Powerplay Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
            }
        }

        private void OnOpenInara(object? obj)
        {
            if (selectedSystem == null)
                return;

            ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/galaxy-starsystem/?search={selectedSystem.NonUpperName.Replace(' ', '+')}");
        }

        public override void Dispose()
        {
            this.dataStore.StoreLive -= OnStoreLive;
            this.dataStore.PledgeDataUpdated -= OnPledgeDataUpdated;
            this.dataStore.CyclesUpdated -= OnCyclesUpdated;
            this.dataStore.SystemAdded -= OnSystemAdded;
            this.dataStore.SystemUpdated -= OnSystemUpdated;
            this.dataStore.MeritsEarned -= OnMeritsEarned;
            this.dataStore.SystemCycleUpdated -= OnSystemUpdated;
        }
    }
}
