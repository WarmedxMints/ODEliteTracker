using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class ColonisationSettings
    {
        public event EventHandler<CommoditySorting>? CommoditySortingChanged;
        public event EventHandler<CommoditySorting>? ShoppingListSortingChanged;

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

        public int SelectedDepotTab { get; set; }
        public GridSize ColonisationGridSize { get; set; } = new() { GridLengths = [new(1, GridUnitType.Star), new(15, GridUnitType.Pixel), new(292.5, GridUnitType.Pixel)] };
    }
}
