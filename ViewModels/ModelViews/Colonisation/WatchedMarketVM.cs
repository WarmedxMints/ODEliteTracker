using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.ViewModels.ModelViews.Market;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class WatchedMarketVM(WatchedMarket watchedMarket) : ODObservableObject
    {
        private readonly WatchedMarket watchedMarket = watchedMarket;

        public string Name => watchedMarket.Name;
        public ulong MarketId => watchedMarket.MarketID;
        public ObservableCollection<StationCommodityVM> Commodities => [..watchedMarket.ItemsForPurchase.Select(x => new StationCommodityVM(x, false))];
        public string Total => $"{Math.Max(Commodities.Sum(x => x.Demand), 0):N0} t";

        internal void UpdateMarket()
        {
            //Copy our current list
            var currentVmItems = Commodities.ToList();

            foreach(var commodity in watchedMarket.ItemsForPurchase)
            {
                //Check if we know about the commodity
                var known = currentVmItems.FirstOrDefault(x => string.Equals(x.Name, commodity.Name));

                //If we do, update it
                if(known != null)
                {
                    known.Demand = commodity.Demand;
                    //We know about these items so remove them from our copied lists
                    currentVmItems.Remove(known);
                    continue;
                }
                //If we do not then add it
                Commodities.AddItem(new StationCommodityVM(commodity, false));
            }

            //if there are any commodities which are no longer in demand then remove them
            if (currentVmItems.Count > 0)
            {
                Commodities.RemoveRange(currentVmItems);
            }

            OnPropertyChanged(nameof(Total));
        }
    }
}
