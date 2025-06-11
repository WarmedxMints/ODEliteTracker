using ODEliteTracker.Models;
using ODEliteTracker.Models.Missions;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Massacre;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels
{
    public sealed class MassacreMissionsViewModel : ODViewModel
    {
        public MassacreMissionsViewModel(MassacreMissionStore massacreStore,
                                         SharedDataStore sharedData,
                                         SettingsStore settings)
        {
            this.massacreStore = massacreStore;
            this.sharedDataStore = sharedData;
            this.settings = settings;

            this.massacreStore.StoreLive += OnStoreLive;
            this.massacreStore.MissionAddedEvent += OnMissionAdded;
            this.massacreStore.MissionUpdatedEvent += OnMissionUpdated;
            this.massacreStore.MissionsUpdatedEvent += OnMissionsUpdated;

            this.sharedDataStore.CurrentBody_StationChanged += OnCurrentStationChanged;
            //Timer to update expiry time every minute
            expiryTimeUpdateTimer = new Timer(OnUpdateExpiry, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            if (this.massacreStore.IsLive)
            {
                OnStoreLive(null, true);
            }
        }

        private void OnCurrentStationChanged(object? sender, string? e)
        {
            foreach (var mission in activeMissions)
            {
                mission.UpdateStation(sharedDataStore.CurrentMarketID);
            }
        }

        public override bool IsLive { get => massacreStore.IsLive; }
        public override void Dispose()
        {
            massacreStore.StoreLive -= OnStoreLive;
            massacreStore.MissionAddedEvent -= OnMissionAdded;
            massacreStore.MissionUpdatedEvent -= OnMissionUpdated;
            massacreStore.MissionsUpdatedEvent -= OnMissionsUpdated;
            sharedDataStore.CurrentBody_StationChanged -= OnCurrentStationChanged;
            expiryTimeUpdateTimer.Dispose();
        }

        private readonly MassacreMissionStore massacreStore;
        private readonly SharedDataStore sharedDataStore;
        private readonly SettingsStore settings;
        private readonly Timer expiryTimeUpdateTimer;

        public MassacreStackSorting StackSorting
        {
            get => settings.MassacreSettings.StackSorting;
            set
            {
                settings.MassacreSettings.StackSorting = value;
                OnPropertyChanged(nameof(StackSorting));
                OnPropertyChanged(nameof(Stacks));
            }
        }

        private readonly List<MassacreStackVM> stacks = [];
        public IEnumerable<MassacreStackVM> Stacks
        {
            get
            {
                var massacreStacks = HideCompletedStacks ? stacks.Where(x => x.ActiveMissionCount > 0 && x.KillsRemaining > 0)
                    : stacks.Where(x => x.ActiveMissionCount > 0);

                return StackSorting switch
                {
                    MassacreStackSorting.TargetFaction => massacreStacks.OrderBy(x => x.TargetFaction).ThenBy(x => x.IssuingFaction),
                    MassacreStackSorting.Reward => massacreStacks.OrderByDescending(x => x.Reward),
                    MassacreStackSorting.Kills => massacreStacks.OrderByDescending(x => x.Kills),
                    MassacreStackSorting.Remaining => massacreStacks.OrderByDescending(x => x.KillsRemaining),
                    MassacreStackSorting.Difference => massacreStacks.OrderByDescending(x => x.KillDifference),
                    MassacreStackSorting.MissionCount => massacreStacks.OrderByDescending(x => x.ActiveMissionCount),
                    _ => massacreStacks.OrderBy(x => x.IssuingFaction),
                };
            }
        }

        private IEnumerable<MassacreMissionVM> activeMissions { get; set; } = [];
        public IEnumerable<MassacreMissionVM> ActiveMissions
        {
            get
            {
                return Sorting switch
                {
                    MissionSorting.System => activeMissions.OrderBy(x => x.OriginSystemName).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Station => activeMissions.OrderBy(x => x.OriginStationName).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Faction => activeMissions.OrderBy(x => x.IssuingFaction).ThenBy(x => x.AcceptedTime),
                    MissionSorting.Target => activeMissions.OrderBy(x => x.TargetFaction).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Kills => activeMissions.OrderByDescending(x => x.KillCount).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Reward => activeMissions.OrderByDescending(x => x.Reward).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Expiry => activeMissions.OrderBy(x => x.Expiry).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Wing => activeMissions.OrderBy(x => x.Wing).ThenBy(x => x.IssuingFaction),
                    _ => activeMissions.OrderBy(x => x.AcceptedTime).ThenBy(x => x.IssuingFaction),
                };
            }
        }

        private IEnumerable<MassacreMissionVM> completedMissions { get; set; } = [];
        public IEnumerable<MassacreMissionVM> CompletedMissions
        {
            get
            {
                return Sorting switch
                {
                    MissionSorting.System => completedMissions.OrderBy(x => x.OriginSystemName).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Station => completedMissions.OrderBy(x => x.OriginStationName).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Faction => completedMissions.OrderBy(x => x.IssuingFaction).ThenBy(x => x.AcceptedTime),
                    MissionSorting.Target => completedMissions.OrderBy(x => x.TargetFaction).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Kills => completedMissions.OrderByDescending(x => x.KillCount).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Reward => completedMissions.OrderByDescending(x => x.Reward).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Expiry => completedMissions.OrderBy(x => x.Expiry).ThenBy(x => x.IssuingFaction),
                    MissionSorting.Wing => completedMissions.OrderBy(x => x.Wing).ThenBy(x => x.IssuingFaction),
                    _ => completedMissions.OrderBy(x => x.AcceptedTime).ThenBy(x => x.IssuingFaction),
                };
            }
        }

        public ObservableCollection<FactionStackVM> FactionStacks { get; private set; } = [];
        public int ActiveMissionCount => activeMissions.Where(x => x.CurrentState == MissionState.Active).Count();
        public int RedirectedMissionCount => activeMissions.Where(x => x.CurrentState == MissionState.Redirected).Count();

        public bool HideCompletedStacks
        {
            get => settings.MassacreSettings.HideCompletedStacks;
            set
            {
                settings.MassacreSettings.HideCompletedStacks = value;
                OnPropertyChanged(nameof(HideCompletedStacks));
                OnPropertyChanged(nameof(Stacks));
            }
        }

        public MissionSorting Sorting
        {
            get => settings.MassacreSettings.MissionSorting;
            set
            {
                settings.MassacreSettings.MissionSorting = value;
                OnPropertyChanged(nameof(Sorting));
                OnPropertyChanged(nameof(ActiveMissions));
                OnPropertyChanged(nameof(CompletedMissions));
            }
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                BuildStacks();
                expiryTimeUpdateTimer.Change(new TimeSpan(0, 5, 0), new TimeSpan(0, 5, 0));
                OnModelLive(true);
            }
        }

        private void BuildStacks()
        {
            foreach (var mission in massacreStore.Missions)
            {
                AddMissionToStack(mission);
            }

            var factionMissions = stacks.Where(x => x.ActiveMissionCount > 0)
                                        .SelectMany(x => x.Missions)
                                        .Where(x => x.CurrentState <= MissionState.Completed)
                                        .GroupBy(x => new { x.TargetFaction, x.TargetSystem })
                                        .ToDictionary(x => x.Key, x => x.ToList());


            FactionStacks = factionMissions.Count != 0 ? [.. factionMissions.Select(x => new FactionStackVM(x.Key.TargetFaction, x.Key.TargetSystem,x.Value))] : [];

            SetMissionCollections();
            OnPropertyChanged(nameof(FactionStacks));
            OnPropertyChanged(nameof(Stacks));
            OnCurrentStationChanged(null, null);
        }

        private void SetMissionCollections()
        {
            activeMissions = stacks.Where(x => x.ActiveMissionCount > 0)
                                   .SelectMany(x => x.Missions)
                                   .Where(x => x.CurrentState < MissionState.Completed)
                                   .OrderBy(x => x.IssuingFaction)
                                   .ThenBy(m => m.AcceptedTime);

            completedMissions = stacks.SelectMany(x => x.Missions)
                                      .Where(x => x.CurrentState == MissionState.Completed)
                                      .OrderByDescending(x => x.CompletionTime);

            if (stacks.Count != 0)
            {
                var maxKills = stacks.Max(x => x.Kills);

                foreach (var stack in stacks)
                {
                    stack.KillDifference = maxKills - stack.Kills;
                }
            }
            OnPropertyChanged(nameof(ActiveMissions));
            OnPropertyChanged(nameof(CompletedMissions));
            OnPropertyChanged(nameof(ActiveMissionCount));
            OnPropertyChanged(nameof(RedirectedMissionCount));
        }

        private void OnMissionAdded(object? sender, MassacreMission e)
        {
            var mission = AddMissionToStack(e);
            SetMissionCollections();
            OnPropertyChanged(nameof(FactionStacks));
            OnPropertyChanged(nameof(Stacks));

            if (mission is null)
                return;

            var knownFactionStack = FactionStacks.FirstOrDefault(x => string.Equals(x.TargetFaction, e.TargetFaction));

            if (knownFactionStack != null)
            {
                knownFactionStack.AddMission(mission);
                return;
            }

            FactionStacks.AddItem(new(e.TargetFaction, e.TargetSystem, [mission]));
        }

        private void OnMissionUpdated(object? sender, MassacreMission e)
        {
            var mission = stacks.SelectMany(x => x.Missions).FirstOrDefault(x => x.MissionID == e.MissionID);

            if (mission == null)
                return;

            mission.Update(e);

            var stack = stacks.FirstOrDefault(x => string.Equals(x.IssuingFaction, mission.IssuingFaction));
            stack?.UpdateKills();

            if (e.CurrentState >= MissionState.Completed)
            {

                if (stack != null && stack.UpdateMission(e))
                {
                    stacks.Remove(stack);
                }
                SetMissionCollections();
            }

            var factionStack = FactionStacks.FirstOrDefault(x => string.Equals(x.TargetFaction, e.TargetFaction));

            if (factionStack == null)
            {
                return;
            }

            if (factionStack.Update(mission) && factionStack.CurrentMissionCount == 0)
            {
                FactionStacks.RemoveItem(factionStack);
            }

            OnPropertyChanged(nameof(ActiveMissionCount));
            OnPropertyChanged(nameof(RedirectedMissionCount));
            OnPropertyChanged(nameof(Stacks));
            OnPropertyChanged(nameof(ActiveMissions));
            OnPropertyChanged(nameof(CompletedMissions));
        }

        private void OnMissionsUpdated(object? sender, EventArgs e)
        {
            stacks.Clear();
            FactionStacks.ClearCollection();
            BuildStacks();
        }

        private MassacreMissionVM? AddMissionToStack(MassacreMission mission)
        {
            var stack = stacks.FirstOrDefault(x => string.Equals(x.IssuingFaction, mission.IssuingFaction)
                                                && string.Equals(x.TargetFaction, mission.TargetFaction)
                                                && string.Equals(x.TargetSystem, mission.TargetSystem));

            if (stack is null)
            {
                stack = new MassacreStackVM(mission.IssuingFaction, mission.TargetFaction, mission.OriginSystemName, mission.TargetSystem);
                stacks.Add(stack);
            }
            return stack.AddMission(mission);
        }

        private void OnUpdateExpiry(object? state)
        {
            if (activeMissions.Any() == false)
                return;

            lock (activeMissions)
            {
                foreach (var mission in activeMissions)
                {
                    mission.UpdateExpiryTime();
                }
            }
        }
    }
}
