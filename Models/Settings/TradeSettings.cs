using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class TradeSettings
    {
        public GridSize ActiveMissionsGridSize { get; set; } = new() { GridLengths = [new(1.3, GridUnitType.Star), new(15, GridUnitType.Pixel), new(1, GridUnitType.Star)] };

        internal static TradeSettings GetDefault()
        {
            return new();
        }
    }
}
