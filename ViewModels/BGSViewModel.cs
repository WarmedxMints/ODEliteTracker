using ODEliteTracker.Models.BGS;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Missions;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.BGS;
using ODEliteTracker.ViewModels.ModelViews.Shared;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class BGSViewModel : ODViewModel
    {
        public BGSViewModel(BGSDataStore dataStore,
                            SharedDataStore sharedDataStore,
                            SettingsStore settings,
                            NotificationService notification,
                            PopOutService popOutService)
        {
            this.dataStore = dataStore;
            this.sharedDataStore = sharedDataStore;
            this.settings = settings;
            this.notification = notification;
            this.popOutService = popOutService;
            this.dataStore.StoreLive += OnStoreLive;
            this.dataStore.MissionAddedEvent += OnMissionAdded;
            this.dataStore.MissionUpdatedEvent += OnMissionUpdated;
            this.dataStore.MissionsUpdatedEvent += OnMissionsUpdated;
            this.dataStore.SystemAdded += OnSystemAdded;
            this.dataStore.SystemUpdated += OnSystemUpdated;
            this.dataStore.VouchersClaimedEvent += OnVoucherClaimed;
            this.dataStore.OnNewTickDetected += OnNewTick;
            this.dataStore.SettlementActivityUpdated += DataStore_SettlementActivityUpdated;

            this.sharedDataStore.StoreLive += OnSharedDataLive;
            this.sharedDataStore.CurrentSystemChanged += OnCurrentSystemChanged;
            SetSelectedSystemCommand = new ODRelayCommand<BGSTickSystemVM>(OnSetSelectedSystem);
            AddNewTickCommand = new ODAsyncRelayCommand<Window?>(OnAddNewTick);
            DeletedTickCommand = new ODAsyncRelayCommand(OnDeleteTick, () => SelectedTick?.ManualTick == true);
            CreateDiscordPostCommand = new ODRelayCommand(OnCreateDiscordPost);
            OpenInaraCommand = new ODRelayCommand(OnOpenInara);
            SetExcludedSystem = new ODRelayCommand<BGSTickSystemVM>(OnSetIgnored);
            OpenPopOut = new ODRelayCommand<Type>(OnOpenPopOut);

            missionExpiryUpdateTimer = new Timer(OnUpdateExpiry, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            bountyManager = new(this.sharedDataStore, this);
            bountyManager.TopFactionSet += OnTopFactionSet;
            bountyManager.TopFaction = settings.BGSViewSettings.TopMostBountyFaction;

            materialTraders = new(this.notification);

            if (this.dataStore.IsLive)
                OnStoreLive(null, true);

            if (this.sharedDataStore.IsLive)
                OnSharedDataLive(null, true);
        }

        public override void Dispose()
        {
            this.dataStore.StoreLive -= OnStoreLive;
            this.dataStore.MissionAddedEvent -= OnMissionAdded;
            this.dataStore.MissionUpdatedEvent -= OnMissionUpdated;
            this.dataStore.MissionsUpdatedEvent -= OnMissionsUpdated;
            this.dataStore.SystemAdded -= OnSystemAdded;
            this.dataStore.SystemUpdated -= OnSystemUpdated;
            this.dataStore.VouchersClaimedEvent -= OnVoucherClaimed;
            this.dataStore.OnNewTickDetected -= OnNewTick;
            this.dataStore.SettlementActivityUpdated -= DataStore_SettlementActivityUpdated;

            missionExpiryUpdateTimer.Dispose();
            bountyManager.TopFactionSet -= OnTopFactionSet;
            bountyManager.Dispose();
            this.sharedDataStore.StoreLive -= OnSharedDataLive;
            this.sharedDataStore.CurrentSystemChanged -= OnCurrentSystemChanged;
        }

        private readonly Timer missionExpiryUpdateTimer;
        private readonly BGSDataStore dataStore;
        private readonly SharedDataStore sharedDataStore;
        private readonly SettingsStore settings;
        private readonly NotificationService notification;
        private readonly PopOutService popOutService;

        public override bool IsLive => dataStore.IsLive;
        public SettingsStore Settings => settings;
        public ICommand SetSelectedSystemCommand { get; }
        public ICommand AddNewTickCommand { get; }
        public ICommand DeletedTickCommand { get; }
        public ICommand CreateDiscordPostCommand { get; }
        public ICommand OpenInaraCommand { get; }
        public ICommand SetExcludedSystem { get; }
        public ICommand OpenPopOut { get; }

        public bool HideSystemsWithoutBGSData
        {
            get => settings.BGSViewSettings.HideSystemsWithoutData;
            set
            {
                settings.BGSViewSettings.HideSystemsWithoutData = value;
                OnPropertyChanged(nameof(HideSystemsWithoutBGSData));
                OnPropertyChanged(nameof(Systems));
            }
        }

        public int SelectedTab
        {
            get => settings.BGSViewSettings.SelectedTab;
            set
            {
                settings.BGSViewSettings.SelectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }

        private BGSTickSystemVM? selectedSystem;
        public BGSTickSystemVM? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                selectedSystem = value;
                bountyManager.OnSelectedSystemChanged();
                OnPropertyChanged(nameof(SelectedSystem));
            }
        }

        private ObservableCollection<BGSTickSystemVM> systems { get; set; } = [];
        public IEnumerable<BGSTickSystemVM> Systems
        {
            get
            {
                if (HideSystemsWithoutBGSData)
                    return systems.Where(x => x.Address == dataStore.CurrentSystem?.Address || x.HasData).OrderBy(x => x.Name);
                return systems;
            }
        }

        public IEnumerable<MegaShipScanVM> MegaShipScans { get; private set; } = [];

        public ObservableCollection<TickDataVM> Ticks { get; set; } = [];

        public ObservableCollection<BGSMissionVM> Missions { get; set; } = [];

        private TickDataVM? selectedTick;
        public TickDataVM? SelectedTick
        {
            get => selectedTick;
            set
            {
                selectedTick = value;
                PopulateSystems();
                OnPropertyChanged(nameof(SelectedTick));
            }
        }

        private SettlementActivityVM? settlementActivityVM;
        public SettlementActivityVM? SettlementActivityVM
        {
            get => settlementActivityVM;
            set
            {
                settlementActivityVM = value;
                OnPropertyChanged(nameof(SettlementActivityVM));    
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

        private BountyManagerVM bountyManager;
        public BountyManagerVM BountyManager
        {
            get => bountyManager;
            set
            {
                bountyManager = value;
                OnPropertyChanged(nameof(BountyManager));
            }
        }

        private MaterialTradersVM materialTraders;
        public MaterialTradersVM MaterialTraders
        {
            get => materialTraders;
            set
            {
                materialTraders = value;
                OnPropertyChanged(nameof(MaterialTraders));
            }
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {

                var ticks = dataStore.TickData.OrderByDescending(x => x.Time).Select(x => new TickDataVM(x));
                Ticks.AddRange(ticks);

                SelectedTick = Ticks.FirstOrDefault(x => string.Equals(x.ID, dataStore.SelectedTick?.ID)) ?? Ticks.FirstOrDefault();
                PopulateSystems();
                PopulateMegaships();
                PopulateMissions();
                OnPropertyChanged(nameof(Ticks));
                missionExpiryUpdateTimer.Change(new TimeSpan(0, 1, 0), new TimeSpan(0, 1, 0));
                OnModelLive(true);
                SettlementActivityVM = new(dataStore.SettlementActivity);
            }
        }

        private void OnCreateDiscordPost(object? obj)
        {
            if (selectedTick is null)
            {
                return;
            }

            if (Helpers.DiscordPostCreator.CreateBGSPost(systems, selectedTick))
            {
                DiscordButtonText = "Post Created";
                notification.ShowBasicNotification(new("Clipboard", ["BGS Activity Post", "Copied To Clipboard"], Models.Settings.NotificationOptions.CopyToClipboard));
                Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
                return;
            }

            DiscordButtonText = "No Data";
            Task.Delay(4000).ContinueWith(e => { DiscordButtonText = "Create Post"; });
        }

        private void OnOpenInara(object? obj)
        {
            if (selectedSystem == null)
                return;

            ODMVVM.Helpers.OperatingSystem.OpenUrl($"https://inara.cz/galaxy-starsystem/?search={selectedSystem.NonUpperName.Replace(' ', '+')}");
        }

        private void OnVoucherClaimed(object? sender, BGSStarSystem e)
        {
            //TODO Implement updating properties
            PopulateSystems(e);
        }

        private void OnSystemUpdated(object? sender, BGSStarSystem e)
        {
            //TODO Implement updating properties
            PopulateSystems(e);
        }

        private void OnSystemAdded(object? sender, BGSStarSystem e)
        {
            //TODO Implement updating properties
            PopulateSystems(e);
        }

        private void OnMissionsUpdated(object? sender, EventArgs e)
        {
            //TODO Implement updating properties
            PopulateSystems();
            PopulateMissions();
        }

        private void OnMissionUpdated(object? sender, BGSMission e)
        {
            //TODO Implement updating properties
            PopulateSystems();
            PopulateMissions();
        }

        private void OnMissionAdded(object? sender, BGSMission e)
        {
            //TODO Implement updating properties
            PopulateSystems();
            PopulateMissions();
        }

        private void OnUpdateExpiry(object? state)
        {
            if (Missions.Count == 0)
                return;

            lock (Missions)
            {
                foreach (var mission in Missions)
                {
                    mission.UpdateExpiry();
                }
            }
        }

        private void PopulateSystems(BGSStarSystem? system = null)
        {
            var tickData = dataStore.GetTickInfo(SelectedTick?.ID);

            var systems = tickData.Item1.OrderBy(x => x.Name).Select(x => new BGSTickSystemVM(x, dataStore.BGSIgnoredSystems.FirstOrDefault(i => i.Address == x.Address) == null)).ToList();

            foreach (var mission in tickData.Item2)
            {
                switch (mission.CurrentState)
                {
                    case MissionState.Completed:
                        if (mission.FactionEffects is null || mission.FactionEffects.Count == 0)
                        {
                            continue;
                        }

                        foreach (var effect in mission.FactionEffects)
                        {
                            foreach (var inf in effect.Influence)
                            {
                                var faction = systems.FirstOrDefault(x => x.Address == inf.SystemAddress)?
                                    .Factions.FirstOrDefault(x => string.Equals(x.Name, effect.FactionName));

                                if (faction != null)
                                {
                                    faction.InfPlus += inf.Influence;
                                }
                            }
                        }
                        break;
                    case MissionState.Failed:
                        var fction = systems.FirstOrDefault(x => x.Address == mission.OriginSystemAddress)?
                                   .Factions.FirstOrDefault(x => string.Equals(x.Name, mission.IssuingFaction));

                        if (fction != null)
                        {
                            fction.Failed++;
                        }
                        break;
                }
            }

            this.systems.ClearCollection();
            this.systems.AddRange(systems);
            var address = SelectedSystem == null ? dataStore.CurrentSystem?.Address ?? 0 : SelectedSystem.Address;
            SelectedSystem = system == null ? this.systems.FirstOrDefault(x => x.Address == address) : this.systems.FirstOrDefault(x => x.Address == system.Address);
            OnPropertyChanged(nameof(Systems));
        }

        private void PopulateMissions()
        {
            var missions = dataStore.Missions.Where(x => x.CurrentState < MissionState.Completed).Select(x => new BGSMissionVM(x));

            Missions.ClearCollection();
            Missions.AddRange(missions);
        }

        private void PopulateMegaships()
        {
            MegaShipScans = dataStore.MegaShipScans.Select(x => new MegaShipScanVM(x));
            OnPropertyChanged(nameof(MegaShipScans));
        }

        private void OnSetSelectedSystem(BGSTickSystemVM vM)
        {
            SelectedSystem = vM;
        }

        private void OnNewTick(object? sender, EventArgs e)
        {
            var ticks = dataStore.TickData.OrderByDescending(x => x.Time).Select(x => new TickDataVM(x));
            Ticks.ClearCollection();
            Ticks.AddRange(ticks);

            SelectedTick = Ticks.FirstOrDefault();

            PopulateSystems();
            PopulateMissions();
            OnPropertyChanged(nameof(Ticks));
        }

        private async Task OnDeleteTick()
        {
            if (SelectedTick == null)
                return;

            await dataStore.DeleteTick(SelectedTick.ID);

            var ticks = dataStore.TickData.OrderByDescending(x => x.Time).Select(x => new TickDataVM(x));
            Ticks.ClearCollection();
            Ticks.AddRange(ticks);

            SelectedTick = Ticks.FirstOrDefault();

            PopulateSystems();
            PopulateMissions();
            OnPropertyChanged(nameof(Ticks));
        }

        private async Task OnAddNewTick(Window? window)
        {
            var time = ODDialogService.ShowDateTimeSelector(window, "Add Tick", DateTime.UtcNow.AddYears(1286));

            if (time.Result == true)
            {
                var tick = await dataStore.AddTick(time.Time.AddYears(-1286));

                var ticks = dataStore.TickData.OrderByDescending(x => x.Time).Select(x => new TickDataVM(x));
                Ticks.ClearCollection();
                Ticks.AddRange(ticks);

                SelectedTick = Ticks.FirstOrDefault(x => string.Equals(x.ID, tick.Id)) ?? Ticks.FirstOrDefault();

                PopulateSystems();
                PopulateMissions();
                OnPropertyChanged(nameof(Ticks));
            }
        }

        private void OnTopFactionSet(object? sender, string e)
        {
            this.settings.BGSViewSettings.TopMostBountyFaction = e;
        }

        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            if (e is null)
                return;

            MaterialTraders.PopulateTraders(sharedDataStore.GetNearestTraders(), e.Position);
        }

        private void OnSharedDataLive(object? sender, bool e)
        {
            if (e)
            {
                OnCurrentSystemChanged(sender, sharedDataStore.CurrentSystem);
            }
        }

        private void DataStore_SettlementActivityUpdated(object? sender, SettlementActivity e)
        {
            if(dataStore.IsLive)
            {
                SettlementActivityVM?.Update();
            }
        }

        private void OnSetIgnored(BGSTickSystemVM vM)
        {
            var include = !vM.IncludeInPost;
            vM.IncludeInPost = include;

            dataStore.AddIgnoredSystem(vM.Address, vM.Name, !include);
        }

        private void OnOpenPopOut(Type type)
        {
            popOutService.OpenPopOut(type, settings.SelectedCommanderID);
        }
    }
}
