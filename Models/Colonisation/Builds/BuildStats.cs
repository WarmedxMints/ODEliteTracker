using ODEliteTracker.ViewModels.ModelViews.Colonisation;

namespace ODEliteTracker.Models.Colonisation.Builds
{
    public abstract class BuildStats(ColonisationBuild type, Dictionary<string, int> stats)
    {
        public ColonisationBuild BuildType { get; } = type;

        public Dictionary<string, int> Stats { get; } = stats;

        public bool Contains(IEnumerable<ConstructionResource> resources)
        {
            foreach (var resource in resources)
            {
                if (Stats.TryGetValue(resource.FDEVName, out int value) && value == resource.RequiredAmount)
                {
                    continue;
                }

                return false;
            }
            return true;
        }
    }
}
