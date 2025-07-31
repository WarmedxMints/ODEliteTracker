using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class CarrierSettings
    {
        public CarrierCommoditySorting Sorting { get; set; } = CarrierCommoditySorting.Category;
        public bool AutoStartTimer { get; set; } = true;
        public GridSize GridSize { get; set; } = new() { GridLengths = [new(1, GridUnitType.Star), new(15, GridUnitType.Pixel), new(260, GridUnitType.Pixel)] };
        internal static CarrierSettings GetDefault()
        {
            return new()
            {
                Sorting = CarrierCommoditySorting.Category,
                AutoStartTimer = true
            };
        }
    }
}
