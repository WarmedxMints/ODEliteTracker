using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;

namespace ODEliteTracker.Models.Colonisation.Builds
{
    public sealed class BuildStatsHelper
    {
        private readonly Dictionary<ColonisationBuild, BuildStats> buildStats;

        public BuildStatsHelper()
        {
            var buildStatsClasses = AppDomain
                          .CurrentDomain
                          .GetAssemblies()
                          .SelectMany(assembly => assembly.GetTypes())
                          .Where(type => typeof(BuildStats)
                          .IsAssignableFrom(type))
                          .Where(type => !(type.IsAbstract || type.IsGenericTypeDefinition || type.IsInterface)); ;
            var instances = buildStatsClasses.Select(x => Activator.CreateInstance(x) as BuildStats);

            buildStats = [];

            foreach (var buildStatsClass in instances)
            {
                if (buildStatsClass == null)
                    continue;

                buildStats.TryAdd(buildStatsClass.BuildType, buildStatsClass);
            }
        }

        internal ColonisationBuild GetBuildType(List<ConstructionResource> resources)
        {
            foreach (var build in buildStats)
            {
                if (build.Value.Contains(resources))
                    return build.Key;
            }

            return ColonisationBuild.Unknown;
        }

        internal bool TryGetBuild(ColonisationBuild build, [MaybeNullWhen(false)] out BuildStats stats)
        {
            if (buildStats.TryGetValue(build, out var buildStat))
            {
                stats = buildStat;
                return true;
            }
            stats = default;
            return false;
        }
    }
}
