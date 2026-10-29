using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;
using ODMVVM.Extensions;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class TradePlannerApiVM : ApiBase
    {
        private ApiTradePlannerParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM system = new();
        public override ApiRouteSystemVM SourceSystem
        {
            get => system;
            set
            {
                system.PropertyChanged -= SourceSystem_PropertyChanged;
                ClearStations();
                system = value;
                system.PropertyChanged += SourceSystem_PropertyChanged;
                OnPropertyChanged(nameof(SourceSystem));
            }
        }             

        public bool AllowPlanetary { get => apiParameters.AllowPlanetary; set { apiParameters.AllowPlanetary = value; OnPropertyChanged(nameof(AllowPlanetary)); } }
        public bool AllowPlayerOwned { get => apiParameters.AllowPlayerOwned; set { apiParameters.AllowPlayerOwned = value; OnPropertyChanged(nameof(AllowPlayerOwned)); } }
        public bool AllowProhibited { get => apiParameters.AllowProhibited; set { apiParameters.AllowProhibited = value; OnPropertyChanged(nameof(AllowProhibited)); } }
        public bool AllowRestrictedAccess { get => apiParameters.AllowRestrictedAccess; set { apiParameters.AllowRestrictedAccess = value; OnPropertyChanged(nameof(AllowRestrictedAccess)); } }
        public bool Permit { get => apiParameters.Permit; set { apiParameters.Permit = value; OnPropertyChanged(nameof(Permit)); } }
        public bool RequiresLargePad { get => apiParameters.RequiresLargePad; set { apiParameters.RequiresLargePad = value; OnPropertyChanged(nameof(RequiresLargePad)); } }
        public bool Unique { get => apiParameters.Unique; set { apiParameters.Unique = value; OnPropertyChanged(nameof(Unique)); } }
        public int MaxCargo { get => apiParameters.MaxCargo; set { apiParameters.MaxCargo = value; OnPropertyChanged(nameof(MaxCargo)); } }
        public double MaxHopDistance { get => apiParameters.MaxHopDistance; set { apiParameters.MaxHopDistance = value; OnPropertyChanged(nameof(MaxHopDistance)); } }
        public long MaxHops { get => apiParameters.MaxHops; set { apiParameters.MaxHops = value; OnPropertyChanged(nameof(MaxHops)); } }
        public int MaxPriceAge { get => apiParameters.MaxPriceAge / 86400; set { apiParameters.MaxPriceAge = value * 86400; OnPropertyChanged(nameof(MaxPriceAge)); } }
        public long MaxSystemDistance { get => apiParameters.MaxSystemDistance; set { apiParameters.MaxSystemDistance = value; OnPropertyChanged(nameof(MaxSystemDistance)); } }
        public long StartingCapital { get => apiParameters.StartingCapital; set { apiParameters.StartingCapital = value; OnPropertyChanged(nameof(StartingCapital)); } }

        public ObservableCollection<ApiTradePlannerStationVM> Stations { get; set; } = [];
        private ApiTradePlannerStationVM? selectedStation;
        public ApiTradePlannerStationVM? SelectedStation { get => selectedStation; set { selectedStation = value; OnPropertyChanged(nameof(SelectedStation)); } }

        public TradePlannerApiVM()
        {
            system.PropertyChanged += SourceSystem_PropertyChanged;
        }

        private void SourceSystem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //clear the stations list if the name is changed
            if(e.PropertyName == nameof(SourceSystem.SystemName))
            {
                ClearStations();
            }
        }

        private void ClearStations()
        {
            Stations.ClearCollection();
            SelectedStation = null;
        }

        internal override bool CanGenerate()
        {
            return SourceSystem.HasValue && SelectedStation != null;
        }

        public override bool CanImportStats => true;
        internal override void ImportStats(ShipInfoVM vm)
        {
            MaxHopDistance = vm.JumpRangeLadenValue;
            RequiresLargePad = vm.NeedsLargePad;
            MaxCargo = vm.CargoCapacity;
        }
        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.System = system.SystemName;
            apiParameters.Station = selectedStation?.StationName;
            return apiParameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(system),
                Json = JsonConvert.SerializeObject(apiParameters),
                SelectedStationMarketID = SelectedStation?.MarketId ?? 0,
                Stations = [.. Stations.Select(x => new ApiTradePlannerStationVM(x))]
            };
            return ret;
        }

        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            return [];
        }

        internal override void LoadDefaults()
        {
            apiParameters.StartingCapital = 1000;
            apiParameters.MaxHopDistance = 50;
            apiParameters.MaxCargo = 7;
            apiParameters.MaxHops = 5;
            apiParameters.MaxSystemDistance = 100_000;
            apiParameters.MaxPriceAge = 30 * 86400;
        }

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiTradePlannerParameters>(saveData.Json);

            if (parameters == null)
                return;
            system.PropertyChanged -= SourceSystem_PropertyChanged;
            system = new(saveData.SourceSystem);
            system.PropertyChanged += SourceSystem_PropertyChanged;
            SelectedStation = null;
            Stations.ClearCollection();
            if(saveData.Stations?.Count > 0)
            {
                Stations.AddRange(saveData.Stations.Select(x => new ApiTradePlannerStationVM(x)));

                SelectedStation = Stations.FirstOrDefault(x => x.MarketId == saveData.SelectedStationMarketID);
            }
            apiParameters = parameters;
        }
    }
}
