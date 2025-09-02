using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Market;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ColonisationShoppingList : ODObservableObject
    {
        public ObservableCollection<ConstructionDepotVM> Depots { get; set; } = [];

        public List<ConstructionResourceVM> Resources { get; set; } = [];

        public string TotalRemaining => $"Total {Resources.Sum(x => x.RemainingCount):N0} t";
        public void AddDepots(IEnumerable<ConstructionDepotVM> depots, Models.FleetCarrier.FleetCarrier? e, Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> purchases)
        {
            Depots.ClearCollection();
            Depots.AddRange(depots);
            PopulateResources(e, purchases);
            OnPropertyChanged(nameof(Depots));
        }

        public void AddDepot(ConstructionDepotVM depot,Models.FleetCarrier.FleetCarrier? e, Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> purchases)
        {
            Depots.AddItem(depot);
            PopulateResources(e, purchases);
            OnPropertyChanged(nameof(Depots));
        }

        public void RemoveDepot(ConstructionDepotVM depot, Models.FleetCarrier.FleetCarrier? e, Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> purchases)
        {
            if (Depots.RemoveItem(depot))
            {
                PopulateResources(e, purchases);
                OnPropertyChanged(nameof(Depots));
            }
        }

        public void PopulateResources(Models.FleetCarrier.FleetCarrier? e, Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>>? purchases)
        {
            Resources.Clear();

            if (Depots.Count == 0)
            {
                return;
            }

            foreach (var depot in Depots)
            {
                foreach (var resource in depot.Resources)
                {
                    var known = Resources.FirstOrDefault(x => x.commodity == resource.commodity);

                    if (known == null)
                    {
                        known = new ConstructionResourceVM(resource);
                        Resources.Add(known);
                        continue;
                    }

                    known.RequiredAmount += resource.RequiredAmount;
                    known.ProvidedAmount += resource.ProvidedAmount;

                    known.UpdateShoppingList();
                }
            }

            UpdateCarrierStock(e);

            if (purchases != null)
                UpdateMostRecentPurchase(purchases);
        }

        public void UpdateCarrierStock(Models.FleetCarrier.FleetCarrier? e)
        {
            if (e == null)
                return;

            foreach(var item in Resources)
            {
                var known = e.Stock.FirstOrDefault(x => x.commodity == item.commodity && x.Stolen == false);

                item.UpdateCarrierStock(known);
            }
        }

        internal void UpdateMostRecentPurchase(Dictionary<ODMVVM.Helpers.Commodity, List<CommodityPurchase>> purchases)
        {
            foreach (var item in purchases)
            {
                var known = Resources.FirstOrDefault(x => x.commodity == item.Key);

                if (known == null)
                    continue;

                var purchased = item.Value.OrderByDescending(x => x.PurchaseDate).FirstOrDefault();

                if (purchased == null)
                    continue;

                MarketPurchaseVM purchase = new(purchased, null);

                known.UpdatePurchase(purchase);
            }
        }

        internal void UpdateDepot(ConstructionDepot e, Models.FleetCarrier.FleetCarrier? carrier)
        {
            var known = Depots.FirstOrDefault(x => x.MarketID == e.MarketID);

            if (known != null)
            {
                known.Update(e);
                PopulateResources(carrier, null);
            }
        }
    }
}
