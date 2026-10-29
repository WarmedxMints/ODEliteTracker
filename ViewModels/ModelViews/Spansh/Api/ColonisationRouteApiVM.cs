using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class ColonisationRouteApiVM : ApiBase
    {
        private ApiColonisationParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM source = new();
        public override ApiRouteSystemVM SourceSystem { get => source; set { source = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? destination = new();
        public override ApiRouteSystemVM? TargetSystem { get => destination; set { destination = value; OnPropertyChanged(nameof(TargetSystem)); } }

        internal override bool CanGenerate()
        {
            return source.HasValue && (destination?.HasValue ?? false);
        }

        internal override List<ApiRouteSystemVM> GetSystemNamesToCheck()
        {
            var ret = new List<ApiRouteSystemVM>()
            {
                source
            };

            if (destination != null)
                ret.Add(destination);
            return ret;
        }

        internal override void LoadDefaults()
        {
            source = new();
            destination = new();
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.SourceSystem = SourceSystem.SystemName;
            apiParameters.DestinationSystem = TargetSystem?.SystemName ?? string.Empty;
            return apiParameters;
        }

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiColonisationParameters>(saveData.Json);

            if (parameters == null)
                return;

            source = new(saveData.SourceSystem);
            destination = new(saveData.TargetSystem);
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(source),
                Json = JsonConvert.SerializeObject(apiParameters)
            };

            if (destination != null)
            {
                ret.TargetSystem = new(destination);
            }
            return ret;
        }
    }
}
