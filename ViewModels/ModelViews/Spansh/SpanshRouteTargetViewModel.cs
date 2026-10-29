using ODEliteTracker.Models.Spansh;
using ODMVVM.Helpers;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class SpanshRouteTargetViewModel
    {
        public SpanshRouteTargetViewModel(SpanshTargetSystem target)
        {
            Target = target;
            BodiesInfo = target.BodiesInfo?.Select(x => new BodiesInfoViewModel(x)).ToList() ?? [];

            if (target.TradeRestult != null)
            {
                TradeResult = new ApiTradePlannerResultVM(target.TradeRestult);
            }
        }

        public SpanshTargetSystem Target { get; }
        public string SystemName => Target.SystemName;
        public string? Property1 => Target.Property1;
        public string? Property2 => Target.Property2;
        public string? Property3 => Target.Property3;
        public string? Property4 => Target.Property4;
        public bool FuelStar => string.Equals(Target.Property3, "Yes", StringComparison.OrdinalIgnoreCase) && Property4 != null;
        public List<BodiesInfoViewModel> BodiesInfo { get; private set; }
        public ApiTradePlannerResultVM? TradeResult { get; private set; }
    }

    public sealed record ApiTradePlannerResultVM(ApiTradePlannerResult result)
    {
        public string SourceSystem => result.Source.System;
        public string DestinationSystem => result.Destination.System;
        public string HeaderText => $"{result.Source.Station} (updated {EliteHelpers.EpochToString(result.Source.MarketUpdatedAt)}) in {SourceSystem}";
        public string FooterText => $"Fly {result.Distance:N2} ly to {result.Destination.Station} in {DestinationSystem}";
        public List<ApiTradeCommodity> Commodities => [.. result.Commodities.Select(x => new ApiTradeCommodity(x.Name,
                                                                                                               $"{x.Amount:N0}",
                                                                                                               $"{x.SourceCommodity.BuyPrice:N0}",
                                                                                                               $"{x.DestinationCommodity.SellPrice:N0}",
                                                                                                               $"{x.Profit:N0}",
                                                                                                               $"{x.TotalProfit:N0}"))];
        public string TotalProfit => $"{result.TotalProfit:N0}";
        public string CumulativeProfit => $"{result.CumulativeProfit:N0}";       
    }

    public sealed record ApiTradeCommodity(string Name, string Amount, string BuyPrice, string SellPrice, string Profit, string TotalProfit);
}
