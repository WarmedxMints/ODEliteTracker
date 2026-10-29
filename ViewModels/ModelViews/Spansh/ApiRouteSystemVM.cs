using ODEliteTracker.Models.Galaxy;
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
        public string SystemName 
        { 
            get => systemName;
            set
            { 
                systemName = value;
                Status = NameCheckStatus.Unchecked;
                Id64 = 0;
                OnPropertyChanged(nameof(SystemName)); 
            } 
        }
        private NameCheckStatus status = NameCheckStatus.Unchecked;
        public NameCheckStatus Status { get => status; set { status = value; OnPropertyChanged(nameof(Status)); } }
        public ApiRouteSystemVM() { }
        public ApiRouteSystemVM(ApiRouteSystemSave sourceSystem)
        {
            Id64 = sourceSystem.Id64;
            systemName = sourceSystem.SystemName;
            status = sourceSystem.Status;
        }

        public ApiRouteSystemVM(SystemInRouteVM gameRouteSystem)
        {
            Id64 = gameRouteSystem.Address;
            systemName = gameRouteSystem.Name;
        }

        public ApiRouteSystemVM(string systemName)
        {
            this.systemName = systemName;
        }

        public ApiRouteSystemVM(Models.FleetCarrier.FleetCarrier carrierData)
        {
            Id64 = carrierData.SystemAddress;
            systemName = carrierData.StarSystem;
        }

        public ApiRouteSystemVM(StarSystem starSystem)
        {
            Id64 = starSystem.Address;
            systemName = starSystem.Name;
        }        

        public bool HasValue => !string.IsNullOrEmpty(systemName);
    }
}
