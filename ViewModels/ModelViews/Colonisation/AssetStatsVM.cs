using ODMVVM.Helpers;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class AssetStatsVM(AssetStats stats)
    {
        private readonly AssetStats stats = stats;

        public string Type => stats.Type;
        public string SubType => stats.SubType;
        public string Pad
        {
            get
            {
                return stats.Padsize switch
                {
                    Models.LandingPadSize.None => string.Empty,
                    _ => stats.Padsize.ToString()
                };
            }
        }
        public string Tier => stats.Tier.ToString();
        public string PointCost => stats.PointCost > 0 ? $"{stats.PointCost}x {stats.CostTier.GetEnumDescription()}" : string.Empty;
        public PointTier CostTier => stats.CostTier;
        public string PointGain => stats.PointGain > 0 ? $"{stats.PointGain}x {stats.GainTier.GetEnumDescription()}" : string.Empty;
        public AssetEconomy Economy => stats.Economy;
        public short Population => stats.Population;
        public short MaxPopulation => stats.MaxPopulation;
        public short Security => stats.Security;
        public short Wealth => stats.Wealth;
        public short TechLevel => stats.TechLevel;
        public short StandardOfLiving => stats.StandardOfLiving;
        public short Development => stats.Development;
        public short Score => stats.Score;
    }
}