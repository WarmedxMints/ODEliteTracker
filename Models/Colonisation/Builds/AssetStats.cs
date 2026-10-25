namespace ODEliteTracker.Models.Colonisation.Builds
{
    [Flags]
    public enum AssetFilter
    {
        SpaceOutpost = 1 << 0,
        Starport = 1 << 1,
        SpaceInstallation = 1 << 2,
        SurfaceOutpost = 1 << 3,
        Settlement = 1 << 4,
        Hub = 1 << 5,
        All = SpaceOutpost | Starport | SpaceInstallation | SurfaceOutpost | Settlement | Hub
    }

    public record AssetSubType(string Name,
                                short SmallPads = 0,
                                short MediumPads = 0,
                                short LargePads = 0);
    public record AssetStats(string Type,
                              IReadOnlyList<AssetSubType> SubTypes,
                              LandingPadSize Padsize,
                              short Tier,
                              short PointCost,
                              PointTier CostTier,
                              short PointGain,
                              PointTier GainTier,
                              AssetEconomy Economy,
                              short Population,
                              short MaxPopulation,
                              short Security,
                              short Wealth,
                              short TechLevel,
                              short StandardOfLiving,
                              short Development,
                              short Score,
                              AssetFilter FilterType);
}
