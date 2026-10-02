using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Trade
{
    public sealed class StationTradeStackVM : ODObservableObject
    {
        public StationTradeStackVM(IEnumerable<TradeMissionVM> missions)
        {
            var first = missions.First();
            OriginSystem = first.OriginSystemName;
            OriginStation = first.OriginStationName;
            SystemAddress = first.OriginSystemAddress;
            MarketID = first.OriginMarketID;
            var stacks = missions.GroupBy(x => x.Commodity_Localised);

            foreach (var kvp in stacks)
            {
                var stack = new CommodityTradeStackInfo([.. kvp]);

                Stacks.Add(stack);
            }

            Stacks.Sort((x, y) => x.Commodity.CompareTo(y.Commodity));
        }

        public ulong SystemAddress { get; }
        public ulong MarketID { get; }
        public string OriginSystem { get; }
        public string OriginStation { get; }

        public List<CommodityTradeStackInfo> Stacks { get; private set; } = [];
    }
}
