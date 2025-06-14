using Newtonsoft.Json;

namespace ODEliteTracker.Models.Spansh
{
    public sealed class SpanshCsvContainer(List<ExplorationTarget> targets, int currentIndex)
    {
        public List<ExplorationTarget> Targets { get; set; } = targets;
        [JsonIgnore]
        public ExplorationTarget? CurrentTarget { get; set; } = targets.FirstOrDefault();
        public int CurrentIndex { get; set; } = currentIndex;
        [JsonIgnore]
        public bool HasValue => Targets.Count > 0;
    }
}
