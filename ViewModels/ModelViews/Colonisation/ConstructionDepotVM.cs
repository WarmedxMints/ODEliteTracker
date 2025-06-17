using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Market;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ConstructionDepotVM(ConstructionDepot depot) : ODObservableObject
    {
        public readonly StarSystem starSystem = depot.StarSystem;
        public bool Inactive { get; set; } = depot.Inactive;
        public long SystemAddress { get; set; } = depot.SystemAddress;
        public string SystemName { get; set; } = depot.SystemName.ToUpper();
        public string SystemNameText { get; set; } = depot.SystemName;
        public string StationName { get; set; } = depot.StationName;
        public string StationNameSplit => StationName.Split([':', ';'], StringSplitOptions.TrimEntries).Last().Trim();
        public bool PlanetBuild => StationName.Contains("Planetary", StringComparison.OrdinalIgnoreCase);
        public long MarketID { get; set; } = depot.MarketID;
        public double ProgressValue { get; set; } = depot.Progress;
        public string Progress => $"{ProgressValue * 100:N2} %";
        public bool Complete { get; set; } = depot.Complete;
        public bool Failed { get; set; } = depot.Failed;
        public string TotalRemaining
        {
            get
            {
                return
                    $"{Resources.Sum(x => x.RemainingCount):N0} t";
            }
        }
        public ObservableCollection<ConstructionResourceVM> Resources { get; set; } 
            = [.. depot.Resources.Select(resource => new ConstructionResourceVM(resource))];

        public IEnumerable<ConstructionResourceVM> FilteredResources => Resources.Where(x => x.RemainingCount > 0).OrderBy(x => x.Category).ThenBy(x => x.LocalName);

        internal void Update(ConstructionDepot e)
        {
            SystemAddress = e.SystemAddress;
            SystemName = e.SystemName.ToUpper();
            StationName = e.StationName;
            ProgressValue = e.Progress;
            Complete = e.Complete;
            Failed = e.Failed;

            foreach (var resource in e.Resources)
            {
                var known = Resources.FirstOrDefault(x => x.FDEVName == resource.FDEVName);

                if (known != null)
                {
                    known.Update(resource);
                    continue;
                }

                Resources.Add(new(resource));
            }

            OnPropertyChanged(nameof(SystemAddress));
            OnPropertyChanged(nameof(SystemName));
            OnPropertyChanged(nameof(StationName));
            OnPropertyChanged(nameof(ProgressValue));
            OnPropertyChanged(nameof(Progress));
            OnPropertyChanged(nameof(Complete));
            OnPropertyChanged(nameof(Failed));
            OnPropertyChanged(nameof(Resources));
            OnPropertyChanged(nameof(FilteredResources));
            OnPropertyChanged(nameof(TotalRemaining));
        }

        internal void UpdateMostRecentPurchase(Dictionary<Commodity, List<CommodityPurchase>> purchases)
        {
            foreach(var item in purchases)
            {
                var known = Resources.FirstOrDefault(x => x.commodity == item.Key);

                if (known == null)
                    continue;

                var purchased = item.Value.OrderByDescending(x => x.PurchaseDate).FirstOrDefault(); 

                if (purchased == null)
                    continue;

                MarketPurchaseVM purchase = new (purchased,null);
   
                known.UpdatePurchase(purchase);
            }
        }
    }
}
