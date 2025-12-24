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
    }

    public enum BuildItemSorting
    {
        Category,
        Name,
        Required,
        Game,
    }
    public sealed class ColonisationBuildTotalsVM(FleetCarrierDataStore fleetCarrierData, SharedDataStore sharedData) : ODObservableObject
    {
        private readonly FleetCarrierDataStore fleetCarrierData = fleetCarrierData;
        private readonly SharedDataStore sharedData = sharedData;

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

        private readonly IEnumerable<AssetStatsVM> allAssetStats = ColonisationAssetStats.GetAll().Select(x => new AssetStatsVM(x));
        public IEnumerable<AssetStatsVM> AllAssetStats
        {
            get => allAssetStats.Where(x => assetFilter.HasFlag(x.Filter));
        }

        private readonly ObservableCollection<AssetStatsVM> assetStats = [];
        public IEnumerable<AssetStatsVM> AssetStats => assetStats;
       
        private BuildItemSorting sorting = BuildItemSorting.Category;

        private ColonisationBuild selectedBuild = ColonisationBuild.OutpostP;

        private readonly ObservableCollection<ColonisationBuildWishListItem> wishListBuilds = [];
        public IEnumerable<ColonisationBuildWishListItem> WishListBuilds => wishListBuilds;

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

            if (fleetCarrierData.IsLive && fleetCarrierData.CarrierData != null)
                CheckCarrierStock(fleetCarrierData.CarrierData, items);

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

            if (fleetCarrierData.IsLive && fleetCarrierData.CarrierData != null)
                CheckCarrierStock(fleetCarrierData.CarrierData, wishListItems);

            WishListTotal = $"{wishListItems.Sum(x => x.RequiredAmount):N0} t";
            SetSelectedBuildLabel(selectedBuild);
            OnPropertyChanged(nameof(WishListItems));
            OnPropertyChanged(nameof(WishListBuilds));
        }

        private static List<ColonisationBuildItemVM> GetBuildItems(ColonisationBuild build, int count = 1)
        {
            Dictionary<string, int> items = build switch
            {
                ColonisationBuild.OutpostP => Outpost.Primary,
                ColonisationBuild.Outpost => Outpost.Secondary,
                ColonisationBuild.Starport_T2P => Starport_Tier2.Primary,
                ColonisationBuild.Starport_T2 => Starport_Tier2.Secondary,
                ColonisationBuild.Starport_T3P => Starport_Tier3.Primary,
                ColonisationBuild.Starport_T3 => Starport_Tier3.Secondary,
                ColonisationBuild.DodecP => Starport_Tier3.DodecPrimary,
                ColonisationBuild.Dodec => Starport_Tier3.DodecSecondary,
                ColonisationBuild.Satellite_Comms => SpaceInstalltion_Tier1.Satellite_Comms,
                ColonisationBuild.SpaceFarm => SpaceInstalltion_Tier1.SpaceFarm,
                ColonisationBuild.PirateInstallation => SpaceInstalltion_Tier1.PirateInstallation,
                ColonisationBuild.MiningOutpost => SpaceInstalltion_Tier1.MiningOutpost,
                ColonisationBuild.RelayStation => SpaceInstalltion_Tier1.RelayStation,
                ColonisationBuild.MilitaryInstallation => SpaceInstalltion_Tier2.Military,
                ColonisationBuild.SecurityStation => SpaceInstalltion_Tier2.SecurityStation,
                ColonisationBuild.GovernmentInstallation => SpaceInstalltion_Tier2.Government,
                ColonisationBuild.MedicalInstallation => SpaceInstalltion_Tier2.Medical,
                ColonisationBuild.ResearchStation => SpaceInstalltion_Tier2.ResearchStation,
                ColonisationBuild.TouristInstallation => SpaceInstalltion_Tier2.Tourist,
                ColonisationBuild.BarInstallation => SpaceInstalltion_Tier2.Bar,
                ColonisationBuild.SurfaceOutpostT1 => PlanetaryPort.Outpost_T1,
                ColonisationBuild.SurfaceOutpostT3 => PlanetaryPort.Outpost_T3,
                ColonisationBuild.SettlementAgriSmall => Settlement_Tier1.Agriculture_Small,
                ColonisationBuild.SettlementAgriMedium => Settlement_Tier1.Agriculture_Medium,
                ColonisationBuild.SettlementAgriLarge => Settlement_Tier1.Agriculture_Large,
                ColonisationBuild.SettlementExtractionSmall => Settlement_Tier1.Extraction_Small,
                ColonisationBuild.SettlementExtractionMedium => Settlement_Tier1.Extraction_Medium,
                ColonisationBuild.SettlementExtractionLarge => Settlement_Tier1.Extraction_Large,
                ColonisationBuild.SettlementIndustrialSmall => Settlement_Tier1.Industrial_Small,
                ColonisationBuild.SettlementIndustrialMedium => Settlement_Tier1.Industrial_Medium,
                ColonisationBuild.SettlementIndustrialLarge => Settlement_Tier1.Industrial_Large,
                ColonisationBuild.SettlementMiltarySmall => Settlement_Tier1.Military_Small,
                ColonisationBuild.SettlementMiltaryMedium => Settlement_Tier1.Military_Medium,
                ColonisationBuild.SettlementMiltaryLarge => Settlement_Tier1.Military_Large,
                ColonisationBuild.SettlementResearchBioSmall => Settlement_Tier2.ResearchBio_Small,
                ColonisationBuild.SettlementResearchBioMedium => Settlement_Tier2.ResearchBio_Medium,
                ColonisationBuild.SettlementResearchBioLarge => Settlement_Tier2.ResearchBio_Large,
                ColonisationBuild.SettlementTourismSmall => Settlement_Tier2.Tourism_Small,
                ColonisationBuild.SettlementTourismMedium => Settlement_Tier2.Tourism_Medium,
                ColonisationBuild.SettlementTourismLarge => Settlement_Tier2.Tourism_Large,
                ColonisationBuild.HubExtraction => Hub.Extraction,
                ColonisationBuild.HubCivilian => Hub.Civilian,
                ColonisationBuild.HubExploration => Hub.Exploration,
                ColonisationBuild.HubOutpost => Hub.Outpost,
                ColonisationBuild.HubScientific => Hub.Scientific,
                ColonisationBuild.HubMilitary => Hub.Military,
                ColonisationBuild.HubRefinery => Hub.Refinery,
                ColonisationBuild.HubHighTech => Hub.HighTech,
                ColonisationBuild.HubIndustrial => Hub.Industrial,
                _ => Outpost.Primary,
            };

            var ret = new List<ColonisationBuildItemVM>();

            foreach (var item in items)
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