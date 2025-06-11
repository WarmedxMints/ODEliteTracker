using ODEliteTracker.Models.Missions;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Massacre;
using ODMVVM.Extensions;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class MassacrePopOutViewModel : PopOutViewModel
    {
        public MassacrePopOutViewModel(MassacreMissionStore missionStore)
        {
            this.massacreStore = missionStore;

            this.massacreStore.StoreLive += OnStoreLive;
            this.massacreStore.MissionAddedEvent += OnMissionAdded;
            this.massacreStore.MissionUpdatedEvent += OnMissionUpdated;
            this.massacreStore.MissionsUpdatedEvent += OnMissionsUpdated;

            if (massacreStore.IsLive)
                OnStoreLive(null, true);
        }

        protected override void Dispose()
        {
            massacreStore.StoreLive -= OnStoreLive;
            massacreStore.MissionAddedEvent -= OnMissionAdded;
            massacreStore.MissionUpdatedEvent -= OnMissionUpdated;
            massacreStore.MissionsUpdatedEvent -= OnMissionsUpdated;
        }

        private readonly MassacreMissionStore massacreStore;

        public override string Name => "Massacre Overlay";
        public override bool IsLive => massacreStore.IsLive;
        public override Uri TitleBarIcon => new("/Assets/Icons/assassin.png", UriKind.Relative);
        private List<MassacreStackVM> stacks { get; } = [];

        public ObservableCollection<FactionStackVM> FactionStacks { get; private set; } = [];

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                BuildStacks();
                OnModelLive();
            }
        }

        private void OnMissionAdded(object? sender, MassacreMission e)
        {
            var mission = AddMissionToStack(e);
            OnPropertyChanged(nameof(FactionStacks));

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
        }

        private void OnMissionsUpdated(object? sender, EventArgs e)
        {
            stacks.Clear();
            FactionStacks.ClearCollection();
            BuildStacks();
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


            FactionStacks = factionMissions.Count != 0 ? [.. factionMissions.Select(x => new FactionStackVM(x.Key.TargetFaction, x.Key.TargetSystem, x.Value))] : [];

            OnPropertyChanged(nameof(FactionStacks));
        }

        private MassacreMissionVM? AddMissionToStack(MassacreMission mission)
        {
            var stack = stacks.FirstOrDefault(x => string.Equals(x.IssuingFaction, mission.IssuingFaction)
                                                && string.Equals(x.TargetFaction, mission.TargetFaction));

            if (stack is null)
            {
                stack = new MassacreStackVM(mission.IssuingFaction, mission.TargetFaction, mission.OriginSystemName, mission.TargetSystem);
                stacks.Add(stack);
            }
            return stack.AddMission(mission);
        }
    }
}
