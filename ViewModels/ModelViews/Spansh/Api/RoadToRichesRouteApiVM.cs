using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class RoadToRichesRouteApiVM : ApiBase
    {
        private ApiRoadToRichesParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM from = new();
        public override ApiRouteSystemVM SourceSystem { get => from; set { from = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? to = new();
        public override ApiRouteSystemVM? TargetSystem { get => to; set { to = value; OnPropertyChanged(nameof(TargetSystem)); } }
        public double Range { get => apiParameters.Range; set { apiParameters.Range = value; OnPropertyChanged(nameof(Range)); } }
        public double Radius { get => apiParameters.Radius; set { apiParameters.Radius = value; OnPropertyChanged(nameof(Radius)); } }
        public int MaxResults { get => apiParameters.MaxResults; set { apiParameters.MaxResults = value; OnPropertyChanged(nameof(MaxResults)); } }
        public long MinValue { get => apiParameters.MinValue; set { apiParameters.MinValue = value; OnPropertyChanged(nameof(MinValue)); } }
        public long MaxDistance { get => apiParameters.MaxDistance; set { apiParameters.MaxDistance = value; OnPropertyChanged(nameof(MaxDistance)); } }
        public bool UseMappingValue { get => apiParameters.UseMappingValue; set { apiParameters.UseMappingValue = value; OnPropertyChanged(nameof(UseMappingValue)); } }
        public bool Loop { get => apiParameters.Loop; set { apiParameters.Loop = value; OnPropertyChanged(nameof(Loop)); } }
        public bool AvoidThargoids { get => apiParameters.AvoidThargoids; set { apiParameters.AvoidThargoids = value; OnPropertyChanged(nameof(AvoidThargoids)); } }
        internal override bool CanGenerate()
        {
            return from.HasValue &&
                Range > 0 &&
                Radius > 0 &&
                MaxResults > 0;
        }
    
        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            if (!from.HasValue)
                return [];

            var ret = new List<ApiRouteSystemVM>
            {
                from
            };

            if (to?.HasValue is bool v && v)
                ret.Add(to);

            return ret;
        }

        internal override void LoadDefaults()
        {
            apiParameters.MinValue = 100000;
            apiParameters.MaxDistance = 50000;
            apiParameters.MaxResults = 100;
            apiParameters.Radius = 25;
            apiParameters.Loop = true;
            apiParameters.AvoidThargoids = true;
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.From = SourceSystem.SystemName;
            apiParameters.To = Loop ? null : TargetSystem?.SystemName ?? string.Empty;

            return apiParameters;
        }
        internal override bool WarnAboutDSS => true;
        public override bool CanImportStats => true;
        internal override void ImportStats(ShipInfoVM vm)
        {
            Range = vm.JumpRangeLadenValue;
        }

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiRoadToRichesParameters>(saveData.Json);

            if (parameters == null)
                return;

            from = new(saveData.SourceSystem);
            to = new(saveData.TargetSystem);
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave()
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(from),
                Json = JsonConvert.SerializeObject(apiParameters)
            };

            if (to != null)
            {
                ret.TargetSystem = new ApiRouteSystemSave(to);
            }

            return ret;
        }
    }
}
