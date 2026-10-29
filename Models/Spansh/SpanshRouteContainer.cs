using Newtonsoft.Json;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Services;

namespace ODEliteTracker.Models.Spansh
{
    public record SpanshRouteContainerSave(string? Job, string? State, string? Status, List<SpanshTargetSystem> Targets, int CurrentIndex, string ShipName);
    public sealed class SpanshRouteContainer
    {
        public SpanshRouteContainer(SpanshResponseBase route, string shipName)
        {
            CreatedDate = LastUsed = route.CreatedAt.ToUniversalTime();
            Format = SpanshRouteFormat.Api;
            Job = route.Job;
            State = route.State;
            Status = route.Status;
            ShipName = shipName;
        }

        internal SpanshRouteContainer(SpanshRouteDTO dto)
        {
            CreatedDate = dto.CreatedDate;
            LastUsed = dto.LastUsed;
            Format = dto.Format;
            RouteType = dto.RouteType;

            var saveData = JsonConvert.DeserializeObject<SpanshRouteContainerSave>(dto.Json);

            if (saveData == null)
                return;

            Job = saveData.Job;
            State = saveData.State;
            Status = saveData.Status;
            Targets = saveData.Targets;
            CurrentTarget = Targets.FirstOrDefault();
            CurrentIndex = saveData.CurrentIndex;
            ShipName = saveData.ShipName;
        }

        internal SpanshRouteContainer(CsvParserReturn csv, string shipName)
        {
            CreatedDate = csv.FileCreationDate;
            LastUsed = DateTime.UtcNow;
            Format = SpanshRouteFormat.CSV;
            RouteType = csv.CsvType;
            Targets = csv.Targets;
            CurrentTarget = Targets.FirstOrDefault();
            CurrentIndex = 0;
            ShipName = shipName;
        }

        public string? Job { get; set; } 
        public string? State { get; set; }
        public string? Status { get; set; }
        public string ShipName { get; set; } = string.Empty;
        public string ShipNameGrid => RouteType == SpanshRouteType.FleetCarrier ? string.Empty : ShipName;
        public DateTime CreatedDate { get; set; }
        public DateTime LastUsed { get; set; }
        public SpanshRouteFormat Format { get; set; }
        public SpanshRouteType RouteType { get; set; }
        public List<SpanshTargetSystem> Targets { get; set; } = [];
        public SpanshTargetSystem? CurrentTarget { get; set; }
        public int CurrentIndex { get; set; }
        public bool HasValue => Targets.Count > 0;

        internal SpanshRouteDTO GetDTO(int commanderId)
        {
            return new SpanshRouteDTO(commanderId, CreatedDate, LastUsed, Format, RouteType, GetSaveString());
        }

        private string GetSaveString()
        {
            var save = new SpanshRouteContainerSave(Job, State, Status, Targets, CurrentIndex, ShipName);

            return JsonConvert.SerializeObject(save);
        }

        public static SpanshRouteContainer? CreateFromApi(SpanshResponseBase responseBase, string shipName)
        {
            var ret = new SpanshRouteContainer(responseBase, shipName);

            switch (responseBase)
            {
                case ApiRoadToRichesRoute r2r:
                    ret.RouteType = r2r.Parameters.GetRouteType();
                    ret.Targets = [.. r2r.Result.Select(x => SpanshTargetSystem.CreateFromApi(r2r.Parameters.GetRouteType(), x))];                    
                    break;
                case ApiNeutronRoute neutron:
                    if (neutron.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.NeutronRoute;
                    ret.Targets = [.. neutron.Result.SystemJumps.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.NeutronRoute, x))];
                    break;
                case ApiColonisationRoute colonisation:
                    if (colonisation.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.Colonisation;
                    ret.Targets = [.. colonisation.Result.Jumps.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.Colonisation, x))];
                    break;
                case ApiFleetCarrierRoute fleetCarrier:
                    if (fleetCarrier.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.FleetCarrier;
                    ret.Targets = [.. fleetCarrier.Result.Jumps.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.FleetCarrier, x))];
                    break;
                case ApiGalaxyPlotterRoute galaxy:
                    if (galaxy.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.GalaxyPlotter;
                    ret.Targets = [.. galaxy.Result.Jumps.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.GalaxyPlotter, x))];
                    break;
                case ApiWorldTypeRoute worldType:
                    if (worldType.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.WorldTypeRoute;
                    ret.Targets = [.. worldType.Result.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.WorldTypeRoute, x))];
                    break;
                case ApiExoviologyRoute exo:
                    ret.RouteType = exo.Parameters.GetRouteType();
                    ret.Targets = [.. exo.Result.Select(x => SpanshTargetSystem.CreateFromApi(exo.Parameters.GetRouteType(), x))];
                    break;
                case ApiTouristRoute touristRoute:
                    if (touristRoute.Result == null)
                        return null;
                    ret.RouteType = SpanshRouteType.TouristRoute;
                    ret.Targets = [.. touristRoute.Result.SystemJumps.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.TouristRoute, x))];
                    break;
                    case ApiTradePlannerRoute tradePlannerRoute:
                    ret.RouteType = SpanshRouteType.TradePlanner;
                    ret.Targets = [.. tradePlannerRoute.Result.Select(x => SpanshTargetSystem.CreateFromApi(SpanshRouteType.TradePlanner, x))];
                    break;

            }
            ret.CurrentTarget = ret.Targets.FirstOrDefault();
            return ret;
        }
    }
}
