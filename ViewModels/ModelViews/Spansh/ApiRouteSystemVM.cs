using ODEliteTracker.Models.Settings;
using ODEliteTracker.ViewModels.ModelViews.Shared;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public enum NameCheckStatus
    {
        Unchecked,
        Good,
        Bad
    }
    public sealed class ApiRouteSystemVM : ODObservableObject
    {
        public ulong Id64 { get; set; }
        private string systemName = string.Empty;
        public string SystemName { get => systemName; set { systemName = value; Status = NameCheckStatus.Unchecked; OnPropertyChanged(nameof(SystemName)); } }
        private NameCheckStatus status = NameCheckStatus.Unchecked;

        public ApiRouteSystemVM() { }
        public ApiRouteSystemVM(ApiRouteSystemSave sourceSystem)
        {
            Id64 = sourceSystem.Id64;
            systemName = sourceSystem.SystemName;
            status = sourceSystem.Status;
        }

        public ApiRouteSystemVM(SystemInRouteVM lastGameRouteSystem)
        {
            Id64 = lastGameRouteSystem.Address;
            systemName = lastGameRouteSystem.Name;
        }

        public NameCheckStatus Status { get => status; set { status = value; OnPropertyChanged(nameof(Status)); } }

        public bool HasValue => !string.IsNullOrEmpty(systemName);
    }
}
