using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class NeutronRouteApiVM : ApiBase
    {
        private ApiNeutronParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM from = new();
        public override ApiRouteSystemVM SourceSystem { get => from; set { from = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? to = new();
        public override ApiRouteSystemVM? TargetSystem { get => to; set { to = value; OnPropertyChanged(nameof(TargetSystem)); } }
        public double Range { get => apiParameters.Range; set { apiParameters.Range = value; OnPropertyChanged(nameof(Range)); } }
        public bool OverChargedSuperCharge { get => apiParameters.SuperchargeMultiplier == 6; set { apiParameters.SuperchargeMultiplier = value ? 6 : 4; OnPropertyChanged(nameof(OverChargedSuperCharge)); } }
        public int Efficiency { get => apiParameters.Efficiency; set { apiParameters.Efficiency = value; OnPropertyChanged(nameof(Efficiency)); } }

        internal override bool CanGenerate()
        {
            return from.HasValue &&
                (to?.HasValue ?? false) &&
                Range > 0 &&
                Efficiency > 0;
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

            if (additionalSystems.Count > 0)
            {
                ret.AddRange([.. additionalSystems.Where(x => !string.IsNullOrEmpty(x.SystemName))]);
            }

            return ret;
        }

        internal override void LoadDefaults()
        {
            apiParameters.SuperchargeMultiplier = 4;
            apiParameters.Efficiency = 60;
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.From = SourceSystem.SystemName;
            apiParameters.To = TargetSystem?.SystemName ?? string.Empty;

            apiParameters.Via = null;
            if (additionalSystems.Count != 0)
            {
                apiParameters.Via = [.. additionalSystems.Select(x => x.SystemName)];
            }

            return apiParameters;
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
        public override bool CanImportStats => true;
        internal override void ImportStats(ShipInfoVM vm)
        {
            Range = vm.JumpRangeLadenValue;
        }
        internal override bool WarnAboutScoop => true;

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiNeutronParameters>(saveData.Json);

            if (parameters == null)
                return;

            from = new(saveData.SourceSystem);
            to = new(saveData.TargetSystem);
            additionalSystems = [.. saveData.AdditionalSystems.Select(x => new ApiRouteSystemVM(x))];
            apiParameters = parameters;
        }

        internal override SpanshAPISave GetSave()
        {
            var ret = new SpanshAPISave()
            {
                RouteType = RouteTypeBox(),
                SourceSystem = new ApiRouteSystemSave(from),
                AdditionalSystems = [.. additionalSystems.Select(x => new ApiRouteSystemSave(x))],
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
