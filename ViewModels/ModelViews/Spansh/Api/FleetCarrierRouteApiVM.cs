using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class FleetCarrierRouteApiVM : ApiBase
    {
        private ApiFleetCarrierRouteParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM sourceSystem = new();
        public override ApiRouteSystemVM SourceSystem { get => sourceSystem; set { sourceSystem = value; OnPropertyChanged(nameof(SourceSystem)); } }
        public bool SquadCarrier { get => apiParameters.Capacity == 60000; set { apiParameters.Capacity = value ? 60000 : 25000; OnPropertyChanged(nameof(SquadCarrier)); } }
        public bool DetermineTritRequirements { get => apiParameters.CalculateStartingFuel; set { apiParameters.CalculateStartingFuel = value; OnPropertyChanged(nameof(DetermineTritRequirements)); } }
        public long CapicityUsed { get => apiParameters.CapacityUsed; set { apiParameters.CapacityUsed = value; OnPropertyChanged(nameof(CapicityUsed)); } }
        public int CurrentFuel { get => apiParameters.CurrentFuel; set { apiParameters.CurrentFuel = value; OnPropertyChanged(nameof(CurrentFuel)); } }
        public long TritInMarket { get => apiParameters.TritiumAmount; set { apiParameters.TritiumAmount = value; OnPropertyChanged(nameof(TritInMarket)); } }

        internal override bool CanGenerate()
        {
            return sourceSystem.HasValue && additionalSystems.Count > 0;
        }

        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            var ret = new List<ApiRouteSystemVM>
            {
                sourceSystem
            };
            ret.AddRange(additionalSystems);
            return ret;
        }

        internal override void LoadDefaults()
        {
            apiParameters.Capacity = 25000;
            apiParameters.CalculateStartingFuel = true;
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.Mass = SquadCarrier ? 15000 : 25000;
            apiParameters.DestinationSystems = [];
            apiParameters.SourceSystem = SourceSystem.SystemName;
            if (additionalSystems.Count != 0)
            {
                apiParameters.DestinationSystems = [.. additionalSystems.Select(x => x.SystemName)];
            }

            return apiParameters;
        }

        public override bool CanImportDestinations => true;
        public override bool CanImportStats => true;
        internal override void ImportStats(Models.FleetCarrier.FleetCarrier? vm)
        {
            if (vm == null)
                return;

            TritInMarket = vm.Stock.FirstOrDefault(x => x.FdevName == "$tritium_name;")?.StockCount ?? 0;
            CurrentFuel = vm.FuelLevel;
            CapicityUsed = vm.UsedCapacity;
        }
        internal override bool CanAddAdditional(List<string> systemNames)
        {
            var count = 0;

            foreach (var name in systemNames)
            {
                if (additionalSystems.FirstOrDefault(x => string.Equals(x.SystemName, name, StringComparison.OrdinalIgnoreCase)) != null)
                    continue;
                count++;
            }
            return additionalSystems.Count + count <= 10;
        }
        internal override bool CanReverseRoute => false;
        public override string ButtonText => "Apply FC Stats";

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiFleetCarrierRouteParameters>(saveData.Json);

            if (parameters == null)
                return;

            sourceSystem = new(saveData.SourceSystem);
            additionalSystems = [.. saveData.AdditionalSystems.Select(x => new ApiRouteSystemVM(x)) ];
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            return new SpanshAPISave
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(sourceSystem),
                AdditionalSystems = [.. additionalSystems.Select(x => new ApiRouteSystemSave(x))],
                Json = JsonConvert.SerializeObject(apiParameters)
            };
        }
    }
}
