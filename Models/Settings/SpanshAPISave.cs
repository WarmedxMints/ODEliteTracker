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
    public sealed class SpanshAPISave
    {
        public SpanshRouteTypeComboBox RouteType { get; set; }
        public ApiRouteSystemSave SourceSystem { get; set; } = new();
        public ApiRouteSystemSave TargetSystem { get; set; } = new();
        public List<ApiRouteSystemSave> AdditionalSystems { get; set; } = [];
        public ulong SelectedStationMarketID { get; set; }
        public List<ApiTradePlannerStationVM>? Stations { get; set; }
        public string Json { get; set; } = string.Empty;
    }
}
