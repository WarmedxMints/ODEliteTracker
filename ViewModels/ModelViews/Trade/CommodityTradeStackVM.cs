using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Trade
{
    public class CommodityTradeStackVM : ODObservableObject
    {
        public CommodityTradeStackVM(IEnumerable<TradeMissionVM> missions)
        {
            this.missions = missions;
            var stacks = missions.GroupBy(x => x.Commodity_Localised);

            foreach (var kvp in stacks)
            {
                var stack = new CommodityTradeStackInfo([.. kvp]);

                Stacks.Add(stack);
            }

            Stacks.Sort((x, y) => x.Commodity.CompareTo(y.Commodity));

            CalcTotals();
        }

        private readonly IEnumerable<TradeMissionVM> missions;
        public string TotalDelivered { get; private set; } = string.Empty;
        public string TotalCount { get; private set; } = string.Empty;
        public string TotalMissionCount => $"{missions?.Count() ?? 0}";
        public string MissionsProgress => $"{missions?.Where(x => x.ReadyToTurnIn).Count() ?? 0} / {missions?.Count() ?? 0}";
        public string TotalValue { get; private set; } = string.Empty;
        public string TotalTurnInValue { get; private set; } = string.Empty;
        public string TotalRemaining { get; private set; } = string.Empty;
        public string ShareableValue { get; private set; } = string.Empty;
        public string SharableTurnInValue { get; private set; } = string.Empty;
        public List<CommodityTradeStackInfo> Stacks { get; private set; } = [];

        private void CalcTotals()
        {
            int totalDelivered = 0;
            int totalCount = 0;
            int totalValue = 0;
            int totalRemaining = 0;

            foreach (var stack in Stacks)
            {
                totalDelivered += stack.DeliveredCountInt;
                totalCount += stack.CommodityCountInt;
                totalValue += stack.ValueInt;
                totalRemaining += stack.RemainingCountInt;
            }

            TotalDelivered = $"{totalDelivered:N0} t";
            TotalCount = $"{totalCount:N0} t";
            TotalValue = $"{totalValue:N0} cr";
            TotalRemaining = $"{totalRemaining:N0} t";

            int totalTurnInValue = 0;
            int shareableValue = 0;
            int sharableTurnInValue = 0;

            foreach (var mission in missions)
            {
                var canTurnIn = mission.Count - mission.ItemsDelivered == 0;
                if (canTurnIn)
                {
                    totalTurnInValue += mission.Reward;
                }
                if (mission.Wing == false)
                    continue;

                shareableValue += mission.Reward;
                if (canTurnIn)
                {
                    sharableTurnInValue += mission.Reward;
                }
            }

            TotalTurnInValue = $"{totalTurnInValue:N0} cr";
            ShareableValue = $"{shareableValue:N0} cr";
            SharableTurnInValue = $"{sharableTurnInValue:N0} cr";

            OnPropertyChanged(nameof(TotalDelivered));
            OnPropertyChanged(nameof(TotalMissionCount));
            OnPropertyChanged(nameof(MissionsProgress));
            OnPropertyChanged(nameof(TotalCount));
            OnPropertyChanged(nameof(TotalValue));
            OnPropertyChanged(nameof(TotalRemaining));
            OnPropertyChanged(nameof(TotalTurnInValue));
            OnPropertyChanged(nameof(ShareableValue));
            OnPropertyChanged(nameof(SharableTurnInValue));
        }
    }
}
