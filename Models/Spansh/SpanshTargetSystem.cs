namespace ODEliteTracker.Models.Spansh
{
    public sealed class SpanshTargetSystem
    {
        public string SystemName { get; set; } = "No Data";
        public string? Property1 { get; set; }
        public string? Property2 { get; set; }
        public string? Property3 { get; set; }
        public string? Property4 { get; set; }
        public List<BodiesInfo>? BodiesInfo { get; set; }
        public ApiTradePlannerResult? TradeRestult { get; set; }
        public static SpanshTargetSystem CreateFromApi(SpanshRouteType type, ISpanshApiSystem system)
        {
            var ret = new SpanshTargetSystem();

            switch (type)
            {
                case SpanshRouteType.RoadToRiches:
                    var r2r = (ApiRoadToRichesReult)system;
                    ret.SystemName = r2r.Name;
                    var bodies = r2r.Bodies?.Select(x => new BodiesInfo(x)).ToList();
                    ret.BodiesInfo = bodies ?? [];
                    break;
                case SpanshRouteType.FleetCarrier:
                    var fc = (ApiFleetCarrierRouteJump)system;
                    ret.SystemName = fc.Name;
                    ret.Property1 = $"{fc.Distance:N2}";
                    ret.Property2 = $"{fc.DistanceToDestination:N2}";
                    ret.Property3 = fc.HasIcyRing ? "Yes" : "No";
                    break;
                case SpanshRouteType.NeutronRoute:
                    var neutron = (ApiNeutronSystemJump)system;
                    ret.SystemName = neutron.System;
                    ret.Property1 = $"{neutron.DistanceJumped:N0}";
                    ret.Property2 = $"{neutron.DistanceLeft:N0}";
                    ret.Property3 = $"{neutron.Jumps}";
                    ret.Property4 = neutron.NeutronStar ? "Yes" : "No";
                    break;
                case SpanshRouteType.GalaxyPlotter:
                    var galaxy = (ApiGalaxyPlotterRouteJump)system;
                    ret.SystemName = galaxy.Name;
                    ret.Property1 = $"{galaxy.Distance:N2}";
                    ret.Property2 = $"{galaxy.DistanceToDestination:N2}";
                    ret.Property3 = galaxy.MustRefuel ? "Yes" : "No";
                    ret.Property4 = galaxy.HasNeutron ? "Yes" : "No";
                    break;
                case SpanshRouteType.WorldTypeRoute:
                    var world = (ApiWorldTypeResult)system;
                    ret.SystemName = world.Name;
                    if (world.Bodies == null || world.Bodies.Count == 0)
                    {
                        ret.BodiesInfo = [];
                        break;
                    }
                    ret.BodiesInfo = [.. world.Bodies.Select(x => new BodiesInfo(x, world.Jumps)) ];
                    break;
                case SpanshRouteType.TouristRoute:
                    var tourist = (ApiTouristRouteSystemJump)system;
                    ret.SystemName = tourist.System;
                    ret.Property1 = $"{tourist.Jumps:N0}";
                    break;
                case SpanshRouteType.Exobiology:
                    var exo = (ApiRoadToRichesReult)system;
                    ret.SystemName = exo.Name;
                    List<BodiesInfo> bodiesInfos = [];
                    foreach (var body in exo.Bodies.OrderBy(x => x.Name))
                    {
                        var landmarks = body.Landmarks.OrderBy(x => x.Subtype, StringComparer.InvariantCultureIgnoreCase).ToList();
                        body.Name = GetBodyName(body.Name, exo.Name);
                        for (var i = 0; i < landmarks.Count; i++)
                        {
                            bodiesInfos.Add(new BodiesInfo(body, landmarks[i], i == 0));
                        }
                    }
                    ret.BodiesInfo = bodiesInfos;
                    break;
                case SpanshRouteType.Colonisation:
                    var coloniation = (ApiColonisationSystemJump)system;
                    ret.SystemName = coloniation.Name;
                    ret.Property1 = $"{coloniation.Distance:N2}";
                    ret.Property2 = $"{coloniation.DistanceToDestination:N2}";
                    break;
                case SpanshRouteType.TradePlanner:
                    var trade = ((ApiTradePlannerResult)system).Copy();
                    ret.SystemName = trade.Source.System;
                    ret.TradeRestult = trade;
                    break;
                default:
                    break;
            }
            return ret;
        }

        private static string GetBodyName(string bodyName, string systemName)
        {
            if (bodyName.Length > systemName.Length)
            {
                return bodyName.Remove(0, systemName.Length);
            }

            return bodyName;
        }
    }
}
