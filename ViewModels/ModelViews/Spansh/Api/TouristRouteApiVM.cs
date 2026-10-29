using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class TouristRouteApiVM : ApiBase
    {
        private ApiTouristRouteApiParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;

        private ApiRouteSystemVM source = new();
        public override ApiRouteSystemVM SourceSystem { get => source; set { source = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? finalDestination = new();
        public override ApiRouteSystemVM? TargetSystem { get => finalDestination; set { finalDestination = value; OnPropertyChanged(nameof(TargetSystem)); } }
        public double Range { get => apiParameters.Range; set { apiParameters.Range = value; OnPropertyChanged(nameof(Range)); } }
        public bool Loop { get => apiParameters.Loop; set { apiParameters.Loop = value; OnPropertyChanged(nameof(Loop)); } }

        internal override bool CanGenerate()
        {
            return source.HasValue &&
                Range > 0 &&
                additionalSystems.Count > 0;
        }

        public override bool CanImportDestinations => true;
        internal override bool CanAddAdditional(List<string> names)
        {
            return true;
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.Source = SourceSystem.SystemName;
            apiParameters.FinalDestination = Loop ? null : TargetSystem?.SystemName ?? string.Empty;

            apiParameters.Destinations = [];
            if (additionalSystems.Count != 0)
            {
                apiParameters.Destinations = [.. additionalSystems.Select(x => x.SystemName)];
            }

            return apiParameters;
        }

        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            var ret = new List<ApiRouteSystemVM>()
            {
                SourceSystem
            };

            if (finalDestination != null)
            {
                ret.Add(finalDestination);
            }
           
            ret.AddRange(additionalSystems);

            //remove any systems with empty systemname
            var blanks = ret.Where(x => string.IsNullOrEmpty(x.SystemName)).ToList();
            foreach (var system in blanks)
            {
                ret.Remove(system);
            }
            return ret;
        }

        public override bool CanImportStats => true;
        internal override void ImportStats(ShipInfoVM vm)
        {
            Range = vm.JumpRangeFullTank;
        }
        internal override void LoadDefaults()
        {
            apiParameters.Loop = true;
        }

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiTouristRouteApiParameters>(saveData.Json);

            if (parameters == null)
                return;

            source = new(saveData.SourceSystem);
            if (!string.IsNullOrEmpty(saveData.TargetSystem?.SystemName))
            {
                finalDestination = new ApiRouteSystemVM(saveData.TargetSystem);
            }
            additionalSystems = [.. saveData.AdditionalSystems.Select(x => new ApiRouteSystemVM(x))];
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(source),
                AdditionalSystems = [.. additionalSystems.Select(x => new ApiRouteSystemSave(x))],
                Json = JsonConvert.SerializeObject(apiParameters)
            };

            if (finalDestination != null)
            {
                ret.TargetSystem = new ApiRouteSystemSave(finalDestination);
            }

            return ret;
        }
    }
}
