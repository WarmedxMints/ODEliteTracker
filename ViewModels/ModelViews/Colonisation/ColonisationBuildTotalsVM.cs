using ODEliteTracker.Models.Colonisation.Builds;
using ODEliteTracker.Stores;
using ODMVVM.Extensions;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public enum ColonisationBuild
    {
        [Description("Outpost [Primary Port]")]
        OutpostP,
        [Description("Outpost")]
        Outpost,
        [Description("Coriolis / Asteroid Base [Primary Port]")]
        Starport_T2P,
        [Description("Coriolis / Asteroid Base")]
        Starport_T2,
        [Description("Ocellus / Orbis [Primary Port]")]
        Starport_T3P,
        [Description("Ocellus / Orbis")]
        Starport_T3,
        [Description("Dodec [Primary Port]")]
        DodecP,
        [Description("Dodec")]
        Dodec,
        [Description("Satellite / Communication Station")]
        Satellite_Comms,
        [Description("Space Farm")]
        SpaceFarm,
        [Description("Pirate Base")]
        PirateInstallation,
        [Description("Mining Outpost")]
        MiningOutpost,
        [Description("Relay Station")]
        RelayStation,
        [Description("Military Installation")]
        MilitaryInstallation,
        [Description("Security Installation")]
        SecurityStation,
        [Description("Government Installation")]
        GovernmentInstallation,
        [Description("Medical Installation")]
        MedicalInstallation,
        [Description("Research Station")]
        ResearchStation,
        [Description("Tourist Installation")]
        TouristInstallation,
        [Description("Bar Installation")]
        BarInstallation,
        [Description("Planetary Outpost Tier 1")]
        SurfaceOutpostT1,
        [Description("Planetary Outpost Tier 3")]
        SurfaceOutpostT3,
        [Description("Agriculture Settlement [Small]")]
        SettlementAgriSmall,
        [Description("Agriculture Settlement [Medium]")]
        SettlementAgriMedium,
        [Description("Agriculture Settlement [Large]")]
        SettlementAgriLarge,
        [Description("Extraction Settlement [Small]")]
        SettlementExtractionSmall,
        [Description("Extraction Settlement [Medium]")]
        SettlementExtractionMedium,
        [Description("Extraction Settlement [Large]")]
        SettlementExtractionLarge,
        [Description("Industrial Settlement [Small]")]
        SettlementIndustrialSmall,
        [Description("Industrial Settlement [Medium]")]
        SettlementIndustrialMedium,
        [Description("Industrial Settlement [Large]")]
        SettlementIndustrialLarge,
        [Description("Miltary Settlement [Small]")]
        SettlementMiltarySmall,
        [Description("Miltary Settlement [Medium]")]
        SettlementMiltaryMedium,
        [Description("Miltary Settlement [Large]")]
        SettlementMiltaryLarge,
        [Description("Research Bio Settlement [Small]")]
        SettlementResearchBioSmall,
        [Description("Research Bio Settlement [Medium]")]
        SettlementResearchBioMedium,
        [Description("Research Bio Settlement [Large]")]
        SettlementResearchBioLarge,
        [Description("Tourism Settlement [Small]")]
        SettlementTourismSmall,
        [Description("Tourism Settlement [Medium]")]
        SettlementTourismMedium,
        [Description("Tourism Settlement [Large]")]
        SettlementTourismLarge,
        [Description("Extraction Hub")]
        HubExtraction,
        [Description("Civilian Hub")]
        HubCivilian,
        [Description("Exploration Hub")]
        HubExploration,
        [Description("Outpost Hub")]
        HubOutpost,
        [Description("Scientific Hub")]
        HubScientific,
        [Description("Military Hub")]
        HubMilitary,
        [Description("Refinery Hub")]
        HubRefinery,
        [Description("High Tech Hub")]
        HubHighTech,
        [Description("Industrial Hub")]
        HubIndustrial,
        Unknown
    }

    public enum BuildItemSorting
    {
        Category,
        Name,
        Required,
        Game,
    }
    public sealed class ColonisationBuildTotalsVM(FleetCarrierDataStore fleetCarrierData, SharedDataStore sharedData, BuildStatsHelper buildStats) : ODObservableObject
    {
        private readonly FleetCarrierDataStore fleetCarrierData = fleetCarrierData;
        private readonly SharedDataStore sharedData = sharedData;
        private readonly BuildStatsHelper buildStats = buildStats;
        private readonly List<ColonisationBuildItemVM> items = [];
        public IEnumerable<ColonisationBuildItemVM> Items
        {
            get
            {
                return sorting switch
                {
                    BuildItemSorting.Category => items.OrderBy(x => x.Category).ThenBy(x => x.Name),
                    BuildItemSorting.Name => items.OrderBy(x => x.Name),
                    BuildItemSorting.Game => [.. items],
                    _ => items.OrderByDescending(x => x.RequiredAmount),
                };
            }
        }

        private readonly List<ColonisationBuildItemVM> wishListItems = [];
        public IEnumerable<ColonisationBuildItemVM> WishListItems
        {
            get
            {
                return sorting switch
                {
                    BuildItemSorting.Category => wishListItems.OrderBy(x => x.Category).ThenBy(x => x.Name),
                    BuildItemSorting.Name => wishListItems.OrderBy(x => x.Name),
                    _ => wishListItems.OrderByDescending(x => x.RequiredAmount),
                };
            }
        }

        private AssetFilter assetFilter = AssetFilter.All;
        public AssetFilter AssetFilter
        {
            get => assetFilter;
            set 
            { 
                assetFilter = value;
                OnPropertyChanged(nameof(assetFilter));
                OnPropertyChanged(nameof(AllAssetStats));
            }
        }

        private readonly List<AssetStatsVM> allAssetStats = [.. ColonisationAssetStats.GetAll().Select(x => new AssetStatsVM(x))];
        public List<AssetStatsVM> AllAssetStats
        {
            get => [.. allAssetStats.Where(x => assetFilter.HasFlag(x.Filter))];
        }

        private readonly ObservableCollection<AssetStatsVM> assetStats = [];
        public ObservableCollection<AssetStatsVM> AssetStats => assetStats;
       
        private BuildItemSorting sorting = BuildItemSorting.Category;

        private ColonisationBuild selectedBuild = ColonisationBuild.OutpostP;

        private readonly ObservableCollection<ColonisationBuildWishListItem> wishListBuilds = [];
        public ObservableCollection<ColonisationBuildWishListItem> WishListBuilds => wishListBuilds;

        private string buildName = string.Empty;
        public string BuildName
        {
            get => buildName;
            set
            {
                buildName = value;
                OnPropertyChanged(nameof(BuildName));
            }
        }

        private bool menuOpen = false;
        public bool MenuOpen
        {
            get => menuOpen;
            set
            {
                menuOpen = value;
                OnPropertyChanged(nameof(MenuOpen));
            }
        }

        private string total = string.Empty;
        public string Total
        {
            get => total;
            set
            {
                total = value;
                OnPropertyChanged(nameof(Total));
            }
        }

        private string wishListTotal = string.Empty;



        public string WishListTotal
        {
            get => wishListTotal;
            set 
            { 
                wishListTotal = value; 
                OnPropertyChanged(nameof(WishListTotal));
            }
        }

        public void BuildItemList(ColonisationBuild build)
        {
            items.Clear();
            items.AddRange(GetBuildItems(build));

            assetStats.ClearCollection();
            assetStats.AddRange(ColonisationAssetStats.GetAssestStats(build).Select(x => new AssetStatsVM(x)));

            if (fleetCarrierData.IsLive && fleetCarrierData.SwitchableCarrierData != null)
                CheckCarrierStock(fleetCarrierData.SwitchableCarrierData, items);

            Total = $"{items.Sum(x => x.RequiredAmount):N0} t";
            selectedBuild = build;
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(AssetStats));
            SetSelectedBuildLabel(selectedBuild);
            MenuOpen = false;
        }

        private void SetSelectedBuildLabel(ColonisationBuild build)
        {
            if (selectedBuild != build)
                return;
            var onWishList = wishListBuilds.FirstOrDefault(x => x.Build == build);
            BuildName = onWishList?.Count > 0 ? $"{build.GetEnumDescription()} [{onWishList.Count} in Wishlist]" : build.GetEnumDescription();
        }

        public void AddSelectedToWishList()
        {
            AddToWishlist(selectedBuild);
        }

        public void RemoveSelectedFromWishList()
        {
            RemoveFromWishlist(selectedBuild); 
        }

        public void AddToWishlist(ColonisationBuild build)
        {
            MenuOpen = false;
            var onWishList = wishListBuilds.FirstOrDefault(x => x.Build == build);

            if (onWishList != null)
            {
                onWishList.Count++;
                sharedData.UpdateColonisationWishListItem(onWishList);
                BuildWishList();
                return;
            }

            var newItem = new ColonisationBuildWishListItem() { Build = build, Count = 1 };
            wishListBuilds.AddItem(newItem);
            sharedData.UpdateColonisationWishListItem(newItem);
            BuildWishList();
        }

        public void RemoveFromWishlist(ColonisationBuild build)
        {
            var onWishList = wishListBuilds.FirstOrDefault(x => x.Build == build);

            if (onWishList != null)
            {
                onWishList.Count--;

                if(onWishList.Count <= 0)
                {
                    wishListBuilds.RemoveItem(onWishList);
                }
                sharedData.UpdateColonisationWishListItem(onWishList);
                BuildWishList();
                return;
            }
        }

        private void BuildWishList()
        {
            wishListItems.Clear();

            if (wishListBuilds.Count == 0)
            {
                WishListTotal = string.Empty;
                SetSelectedBuildLabel(selectedBuild);
                OnPropertyChanged(nameof(WishListItems));
                OnPropertyChanged(nameof(WishListBuilds));
                return;
            }

            foreach (var item in WishListBuilds)
            {
                var buildItems = GetBuildItems(item.Build, item.Count);

                foreach (var buildItem in buildItems)
                {
                    var inList = wishListItems.FirstOrDefault(x => x.Commodity == buildItem.Commodity);

                    if (inList != null)
                    {
                        inList.AddToRequired(buildItem.RequiredAmount);
                        continue;
                    }
                    wishListItems.Add(buildItem);
                }
            }

            if (fleetCarrierData.IsLive && fleetCarrierData.SwitchableCarrierData != null)
                CheckCarrierStock(fleetCarrierData.SwitchableCarrierData, wishListItems);

            WishListTotal = $"{wishListItems.Sum(x => x.RequiredAmount):N0} t";
            SetSelectedBuildLabel(selectedBuild);
            OnPropertyChanged(nameof(WishListItems));
            OnPropertyChanged(nameof(WishListBuilds));
        }

        private List<ColonisationBuildItemVM> GetBuildItems(ColonisationBuild build, int count = 1)
        {

            if (buildStats.TryGetBuild(build, out var stats) == false)
            {
                return [];
            }

            var ret = new List<ColonisationBuildItemVM>();

            foreach (var item in stats.Stats)
            {
                var commodity = EliteCommodityHelpers.GetCommodityDetails(item.Key);

                if (commodity == null)
                    continue;

                var newItem = new ColonisationBuildItemVM(commodity, item.Value * count);
                ret.Add(newItem);
            }

            return ret;
        }

        internal void OnCarrierStockUpdated(Models.FleetCarrier.FleetCarrier e)
        {
            CheckCarrierStock(e, items);
            CheckCarrierStock(e, wishListItems);
        }

        private static void CheckCarrierStock(Models.FleetCarrier.FleetCarrier e, List<ColonisationBuildItemVM> items)
        {
            if (items == null || items.Count == 0)
            {
                return;
            }

            foreach (var item in items)
            {
                var onCarrier = e.Stock.FirstOrDefault(x => string.Equals(x.commodity.FdevName, item.FDEVName, StringComparison.OrdinalIgnoreCase)
                                    && x.Stolen == false);

                if (onCarrier == null)
                {
                    item.SetCarrierStock(0);
                    continue;
                }
                item.SetCarrierStock(onCarrier.StockCount);
            }
        }

        internal void SortLists(BuildItemSorting value)
        {
            sorting = value;
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(WishListItems));
        }

        internal void OnSharedDataLive()
        {
            wishListBuilds.ClearCollection();
            wishListBuilds.AddRange(sharedData.ColonisationWishList.Select(x => new ColonisationBuildWishListItem() { Build = x.Key, Count = x.Value }));
            BuildWishList();
        }
    }
}