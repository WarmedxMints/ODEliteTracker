using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class GalaxyRouteApiVM : ApiBase
    {
        private ApiGalaxyRouteParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;

        private ApiRouteSystemVM sourceSystem = new();
        public override ApiRouteSystemVM SourceSystem { get => sourceSystem; set { sourceSystem = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? destinationSystem = new();
        public override ApiRouteSystemVM? TargetSystem
        {
            get => destinationSystem; 
            set
            {
                destinationSystem = value; OnPropertyChanged(nameof(TargetSystem));
            }
        }
        public int Cargo { get => apiParameters.Cargo; set { apiParameters.Cargo = value; OnPropertyChanged(nameof(Cargo)); } }
        public double ReserveSize { get => apiParameters.ReserveSize; set { apiParameters.ReserveSize = value; OnPropertyChanged(nameof(ReserveSize)); } }
        public bool IsSupercharged { get => apiParameters.IsSupercharged; set { apiParameters.IsSupercharged = value; OnPropertyChanged(nameof(IsSupercharged)); } }
        public bool UseSupercharge { get => apiParameters.UseSupercharge; set { apiParameters.UseSupercharge = value; OnPropertyChanged(nameof(UseSupercharge)); } }
        public bool UseInjections { get => apiParameters.UseInjections; set { apiParameters.UseInjections = value; OnPropertyChanged(nameof(UseInjections)); } }
        public bool UseInjectionsWhenRequired { get => apiParameters.UseInjectionsWhenRequired; set { apiParameters.UseInjectionsWhenRequired = value; OnPropertyChanged(nameof(UseInjectionsWhenRequired)); } }
        public bool ExcludeSecondary { get => apiParameters.ExcludeSecondary; set { apiParameters.ExcludeSecondary = value; OnPropertyChanged(nameof(ExcludeSecondary)); } }
        public bool RefuelEveryScoopable { get => apiParameters.RefuelEveryScoopable; set { apiParameters.RefuelEveryScoopable = value; OnPropertyChanged(nameof(RefuelEveryScoopable)); } }
        public int CalculationTime { get => apiParameters.MaxTime; set { apiParameters.MaxTime = value; OnPropertyChanged(nameof(CalculationTime)); } }
        public SpanshRoutingAlgorithm Algorithm { get => apiParameters.Algorithm; set { apiParameters.Algorithm = value; OnPropertyChanged(nameof(Algorithm)); } }

        internal override bool CanGenerate()
        {
            return sourceSystem.HasValue &&
                (destinationSystem?.HasValue ?? false);
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            if (vm == null)
                return null;

            var info = vm.ShipInfo.GetStats().Item1;
            if (info == null)
                return null;

            apiParameters.ShipBuild = vm.ShipInfo.GetSelf();        
            apiParameters.BaseMass = info.BaseMass;
            apiParameters.FuelMultiplier = info.Fuelmul;
            apiParameters.FuelPower = info.FuelPower;
            apiParameters.InternalTankSize = info.FuelReserve;
            apiParameters.MaxFuelPerJump = info.MaxfuelPerJump;
            apiParameters.OptimalMass = info.OptimalMass;
            apiParameters.RangeBoost = info.GFSDRangeBoost;
            apiParameters.SuperchargeMultiplier = info.SuperChargeMultipler;
            apiParameters.TankSize = info.TankSize;

            apiParameters.SourceSystem = SourceSystem.SystemName;
            apiParameters.DestinationSystem = TargetSystem?.SystemName ?? string.Empty;

            return apiParameters;
        }

        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            var ret = new List<ApiRouteSystemVM>()
            {
               SourceSystem
            };

            if (destinationSystem != null)
            {
                ret.Add(destinationSystem);
            }
            return ret;
        }

        internal override void LoadDefaults()
        {
            apiParameters.ReserveSize = 10;
            apiParameters.UseSupercharge = true;
            apiParameters.MaxTime = 60;
            apiParameters.Algorithm = SpanshRoutingAlgorithm.optimistic;
        }

        internal override bool WarnAboutScoop => apiParameters.UseSupercharge;
        internal override bool WarnAboutAFMU => apiParameters.UseSupercharge;

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiGalaxyRouteParameters>(saveData.Json);

            if (parameters == null)
                return;

            sourceSystem = new(saveData.SourceSystem);
            destinationSystem = new(saveData.TargetSystem);
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave()
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(sourceSystem),
                Json = JsonConvert.SerializeObject(apiParameters)
            };

            if (destinationSystem != null)
            {
                ret.TargetSystem = new ApiRouteSystemSave(destinationSystem);
            }

            return ret;
        }
    }
}
 