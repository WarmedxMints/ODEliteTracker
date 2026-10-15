using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Spansh;

namespace ODEliteTracker.Models.Settings
{
    public record ApiRouteSystemSave
    {
        public ulong Id64 { get; set; }
        public string SystemName { get; set; } = string.Empty;
        public NameCheckStatus Status { get; set; }
        public ApiRouteSystemSave() {  }
        public ApiRouteSystemSave(ApiRouteSystemVM vm)
        {
            Id64 = vm.Id64;
            SystemName = vm.SystemName;
            Status = vm.Status;
        }
    }
    public sealed class SpashAPISave
    {
        public Dictionary<string, bool> BoolValues = [];
        public Dictionary<string, long> IntValues = [];
        public Dictionary<string, double> DoubleValues = [];
        public string? JobID { get; set; }
        public SpanshRouteTypeComboBox RouteType { get; set; }
        public ApiRouteSystemSave SourceSystem { get; set; } = new();
        public ApiRouteSystemSave TargetSystem { get; set; } = new();
        public List<ApiRouteSystemSave> AdditionalSystems { get; set; } = [];
        public SpanshRoutingAlgorithm Algorithm { get; set; } = SpanshRoutingAlgorithm.optimistic;
    }
}
