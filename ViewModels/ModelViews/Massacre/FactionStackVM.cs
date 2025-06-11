using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Massacre
{
    public class FactionStackVM : ODObservableObject
    {
        public FactionStackVM(string targetFaction, string targetSystem, List<MassacreMissionVM> missions)
        {
            this.TargetFaction = targetFaction;
            this.TargetSystem = targetSystem;

            this.missions = [.. missions.Where(x => x.CurrentState < Models.Missions.MissionState.Completed)];
            UpdateKillCounts();
        }

        private readonly List<MassacreMissionVM> missions;
        private int killCount;
        private int killsRemaining;
        private int killsToNextCompletion;
        public string TargetFaction { get; }
        public string TargetSystem { get; }
        public int CurrentMissionCount => missions.Count;
        //Info
        public string MissionCount => $"{missions.Count}";
        public string ActiveMissions => $"{missions.Where(x => x.CurrentState == Models.Missions.MissionState.Active).Count()}";
        public string MissionsRemaining => $"{missions.Count - missions.Where(x => x.CurrentState == Models.Missions.MissionState.Redirected).Count()}";
        public string KillCount => $"{killCount:N0}";
        public string TotalKills => $"{missions.Sum(x => x.KillCount)}";
        public string KillRatio => $"{missions.Sum(x => x.KillCount) / (double)killCount:N2}";
        public string KillsToNextCompletion => $"{killsToNextCompletion}";
        public string KillsRemaining => $"{killsRemaining:N0}";

        //Values
        public string AvgPerKill => $"{missions.Sum(x => x.Reward) / killCount:N0} cr";
        public string AvgPerMission => $"{missions.Sum(x => x.Reward) / missions.Count:N0} cr";
        public string StackValue => $"{missions.Sum(x => x.Reward):N0} cr";
        public string ShareValue => $"{missions.Where(x => x.Wing == true).Sum(x => x.Reward):N0} cr";
        public string TurnInStackValue => $"{missions.Where(x => x.CurrentState == Models.Missions.MissionState.Redirected).Sum(x => x.Reward):N0} cr";
        public string TurnInShareValue => $"{missions.Where(x => x.CurrentState == Models.Missions.MissionState.Redirected && x.Wing).Sum(x => x.Reward):N0} cr";

        private void UpdateKillCounts()
        {
            var missions = this.missions.GroupBy(x => x.IssuingFaction).ToDictionary(x => x.Key, x => x.ToList());

            var killCount = 0;
            var killsRemaining = 0;
            var killsToNextCompletion = int.MaxValue;

            foreach (var mission in missions)
            {
                var k_Count = mission.Value.Sum(x => x.KillCount);
                var kills = mission.Value.Sum(x => x.Kills);
                killCount = Math.Max(killCount, k_Count);
                killsRemaining = Math.Max(killsRemaining, k_Count - kills);

                var firstActiveMission = mission.Value.FirstOrDefault(x => x.CurrentState == Models.Missions.MissionState.Active);

                if (firstActiveMission == null)
                    continue;

                killsToNextCompletion = Math.Min(killsToNextCompletion, firstActiveMission.KillCount - firstActiveMission.Kills);
            }

            this.killCount = killCount;
            this.killsRemaining = killsRemaining;
            this.killsToNextCompletion = killsToNextCompletion == int.MaxValue ? 0 : killsToNextCompletion;

            OnPropertyChanged(nameof(KillCount));
            OnPropertyChanged(nameof(KillRatio));
            OnPropertyChanged(nameof(KillsToNextCompletion));
            OnPropertyChanged(nameof(KillsRemaining));
        }

        /// <summary>
        /// return true if a mission has been removed from the stack otherwise false
        /// </summary>
        /// <param name="mission"></param>
        /// <returns>bool</returns>
        public bool Update(MassacreMissionVM mission)
        {
            //Still active so just updating kills
            if (mission.CurrentState == Models.Missions.MissionState.Active)
            {
                UpdateKillCounts();
                return false;
            }

            if (mission.CurrentState == Models.Missions.MissionState.Redirected)
            {
                UpdateKillCounts();
                UpdateAllStats();
                return false;
            }

            //mission is either completed, failed or abandoned so not part of the stack
            missions.Remove(mission);
            UpdateAllStats();
            return true;
        }

        public void AddMission(MassacreMissionVM mission)
        {
            if (missions.Contains(mission))
            {
                return;
            }

            missions.Add(mission);
            UpdateAllStats();
        }

        private void UpdateAllStats()
        {
            UpdateKillCounts();
            OnPropertyChanged(nameof(MissionCount));
            OnPropertyChanged(nameof(CurrentMissionCount));
            OnPropertyChanged(nameof(ActiveMissions));
            OnPropertyChanged(nameof(MissionsRemaining));
            OnPropertyChanged(nameof(KillCount));
            OnPropertyChanged(nameof(TotalKills));
            OnPropertyChanged(nameof(KillRatio));
            OnPropertyChanged(nameof(KillsToNextCompletion));
            OnPropertyChanged(nameof(KillsRemaining));

            OnPropertyChanged(nameof(AvgPerKill));
            OnPropertyChanged(nameof(AvgPerMission));
            OnPropertyChanged(nameof(StackValue));
            OnPropertyChanged(nameof(ShareValue));
            OnPropertyChanged(nameof(TurnInStackValue));
            OnPropertyChanged(nameof(TurnInShareValue));
        }
    }
}
