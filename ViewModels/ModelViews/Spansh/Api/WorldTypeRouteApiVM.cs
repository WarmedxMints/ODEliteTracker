using Newtonsoft.Json;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh.Api
{
    public sealed class WorldTypeRouteApiVM : ApiBase
    {
        private const string ELW = "Earth-like world";
        private const string AmmoniaWorld = "Ammonia world";
        private const string RockyWorld = "Rocky Body";
        private const string HMCWorld = "High metal content world";

        private ApiWorldTypeParameters apiParameters = new();
        public override SpanshApiParameters ApiParameters => apiParameters;
        private ApiRouteSystemVM from = new();
        public override ApiRouteSystemVM SourceSystem { get => from; set { from = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM? to = new();
        public override ApiRouteSystemVM? TargetSystem { get => to; set { to = value; OnPropertyChanged(nameof(TargetSystem)); } }
        public double Range { get => apiParameters.Range; set { apiParameters.Range = value; OnPropertyChanged(nameof(Range)); } }
        public double Radius { get => apiParameters.Radius; set { apiParameters.Radius = value; OnPropertyChanged(nameof(Radius)); } }
        public int MaxResults { get => apiParameters.MaxResults; set { apiParameters.MaxResults = value; OnPropertyChanged(nameof(MaxResults)); } }
        public long MaxDistance { get => apiParameters.MaxDistance; set { apiParameters.MaxDistance = value; OnPropertyChanged(nameof(MaxDistance)); } }
        public bool Loop { get => apiParameters.Loop; set { apiParameters.Loop = value; OnPropertyChanged(nameof(Loop)); } }
        public bool AvoidThargoids { get => apiParameters.AvoidThargoids; set { apiParameters.AvoidThargoids = value; OnPropertyChanged(nameof(AvoidThargoids)); } }
        public bool EarthLike
        {
            get => apiParameters.BodyTypes?.Contains(ELW) ?? false;
            set
            {
                if (value)
                {
                    apiParameters.BodyTypes ??= [];
                    if (!apiParameters.BodyTypes.Contains(ELW))
                    {
                        apiParameters.BodyTypes.Add(ELW);
                    }
                    OnPropertyChanged(nameof(EarthLike));
                    return;
                }
                apiParameters.BodyTypes?.Remove(ELW);
                OnPropertyChanged(nameof(EarthLike));
            }
        }
        public bool Ammonia
        {
            get => apiParameters.BodyTypes?.Contains(AmmoniaWorld) ?? false;
            set
            {
                if (value)
                {
                    apiParameters.BodyTypes ??= [];
                    if (!apiParameters.BodyTypes.Contains(AmmoniaWorld))
                    {
                        apiParameters.BodyTypes.Add(AmmoniaWorld);
                    }
                    OnPropertyChanged(nameof(Ammonia));
                    return;
                }
                apiParameters.BodyTypes?.Remove(AmmoniaWorld);
                OnPropertyChanged(nameof(Ammonia));
            }
        }
        public bool Rocky
        {
            get => apiParameters.BodyTypes?.Contains(RockyWorld) ?? false;
            set
            {
                if (value)
                {
                    apiParameters.BodyTypes ??= [];
                    if (!apiParameters.BodyTypes.Contains(RockyWorld))
                    {
                        apiParameters.BodyTypes.Add(RockyWorld);
                    }
                    OnPropertyChanged(nameof(Rocky));
                    return;
                }
                apiParameters.BodyTypes?.Remove(RockyWorld);
                OnPropertyChanged(nameof(Rocky));
            }
        }
        public bool HMC
        {
            get => apiParameters.BodyTypes?.Contains(HMCWorld) ?? false;
            set
            {
                if (value)
                {
                    apiParameters.BodyTypes ??= [];
                    if (!apiParameters.BodyTypes.Contains(HMCWorld))
                    {
                        apiParameters.BodyTypes.Add(HMCWorld);
                    }
                    OnPropertyChanged(nameof(HMC));
                    return;
                }
                apiParameters.BodyTypes?.Remove(HMCWorld);
                OnPropertyChanged(nameof(HMC));
            }
        }
        internal override bool CanGenerate()
        {
            return from.HasValue &&
                Radius > 0 &&
                Range > 0 &&
                MaxResults > 0 &&
                (EarthLike || Ammonia || Rocky || HMC); 
        }
        public override bool CanImportStats => true;
        internal override void ImportStats(ShipInfoVM vm)
        {
            Range = vm.JumpRangeLadenValue;
        }
        internal override bool WarnAboutDSS => true;
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
            apiParameters.MaxResults = 100;
            apiParameters.Radius = 25;
            apiParameters.Loop = true;
            apiParameters.AvoidThargoids = true;
            apiParameters.BodyTypes = [ELW];
            apiParameters.MaxDistance = 50_000;
        }

        internal override SpanshApiParameters? GetRouteParameters(ShipInfoVM? vm)
        {
            apiParameters.Source = SourceSystem.SystemName;
            apiParameters.To = Loop ? null : TargetSystem?.SystemName ?? string.Empty;
            apiParameters.MinValue = 1;
            return apiParameters;
        }

        internal override void LoadFromSave(SpanshAPISave saveData)
        {
            if (saveData.Json == null)
                return;

            var parameters = JsonConvert.DeserializeObject<ApiWorldTypeParameters>(saveData.Json);

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
