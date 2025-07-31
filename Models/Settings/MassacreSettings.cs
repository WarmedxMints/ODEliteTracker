using System.Windows;

namespace ODEliteTracker.Models.Settings
{
    public sealed class MassacreSettings
    {
        public bool HideCompletedStacks { get; set; }
        public MissionSorting MissionSorting { get; set; }
        public MassacreStackSorting StackSorting { get; set; }
        public GridSize ActiveMissionsGridSize { get; set; } = new() { GridLengths = [new(1, GridUnitType.Star), new(15, GridUnitType.Pixel), new(1, GridUnitType.Star)] };

        public static MassacreSettings GetDefault()
        {
            return new()
            {
                HideCompletedStacks = false
            };
        }
    }
}
