using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class ColonisationSettings
    {
        public event EventHandler<CommoditySorting>? CommoditySortingChanged;
        public event EventHandler<CommoditySorting>? ShoppingListSortingChanged;
        public event EventHandler<CommoditySorting>? WatchedMarketSortingChanged;
        public event EventHandler? StraightLineStarPosChanged;
        public event EventHandler? StraightLineEndPosChanged;

        private CommoditySorting colonisationCommoditySorting = CommoditySorting.Category;
        public CommoditySorting ColonisationCommoditySorting
        {
            get => colonisationCommoditySorting;
            internal set
            {
                if (value != colonisationCommoditySorting)
                {
                    colonisationCommoditySorting = value;
                    CommoditySortingChanged?.Invoke(this, ColonisationCommoditySorting);
                }
            }
        }

        private CommoditySorting shoppingListSorting = CommoditySorting.Category;
        public CommoditySorting ShoppingListSorting
        {
            get => shoppingListSorting;
            internal set
            {
                if (value != shoppingListSorting)
                {
                    shoppingListSorting = value;
                    ShoppingListSortingChanged?.Invoke(this, ShoppingListSorting);
                }
            }
        }

        private CommoditySorting watchedMarketSorting = CommoditySorting.Category;
        private Position? lineStart;
        private Position? lineEnd;

        public CommoditySorting WatchedMarketSorting
        {
            get => watchedMarketSorting;
            internal set
            {
                if (value != watchedMarketSorting)
                {
                    watchedMarketSorting = value;
                    WatchedMarketSortingChanged?.Invoke(this, WatchedMarketSorting);
                }
            }

        }

        public BuildItemSorting BuildTotalSorting { get; set; } = BuildItemSorting.Category;
        public int SelectedDepotTab { get; set; }
        public GridSize ColonisationGridSize { get; set; } = new() { GridLengths = [new(1, GridUnitType.Star), new(15, GridUnitType.Pixel), new(292.5, GridUnitType.Pixel)] };
        public ColonisationBuild SelectedBuild { get; set; } = ColonisationBuild.OutpostP;

        #region Routeplanner Settings
        public Position? LineStart { get => lineStart; set { lineStart = value; StraightLineStarPosChanged?.Invoke(this, EventArgs.Empty); } }
        public Position? LineEnd { get => lineEnd; set { lineEnd = value; StraightLineEndPosChanged?.Invoke(this, EventArgs.Empty); } }
        public ulong SelectedPlannerAddress { get; set; }
        public bool CheckSpansh { get; set; } = true;
        public bool FollowRoute { get; set; }
        #endregion
    }
}
