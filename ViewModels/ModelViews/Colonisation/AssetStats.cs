using ODEliteTracker.Models;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public record AssetStats(string Type,
                              string SubType,
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
                              short Score);
}
