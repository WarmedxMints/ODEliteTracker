
using System.ComponentModel;

namespace ODEliteTracker.Models.Spansh
{
    public enum SpanshRouteFormat
    {
        CSV,
        Json,
        Api,
    }

    public enum SpanshRouteType
    {
        [Description("Show All")]
        None = -1,
        [Description("Road To Riches")]
        RoadToRiches = 0,
        [Description("Fleet Carrier Route")]
        FleetCarrier,
        [Description("Neutron Plotter")]
        NeutronRoute,
        [Description("Galaxy Plotter")]
        GalaxyPlotter,
        [Description("World Type Route")]
        WorldTypeRoute,
        [Description("Tourist Route")]
        TouristRoute,
        [Description("Exobiology Route")]
        Exobiology,
        [Description("Exobiology Route (Old Format)")]
        ExobiologyOld,
        [Description("Colonisation Plotter")]
        Colonisation,
        [Description("Galaxy Plotter Old")]
        GalaxyPlotterOld,
        [Description("Trade Planner")]
        TradePlanner
    }

    public enum SpanshRouteFilterEnum
    {
        [Description("Show All")]
        ShowAll = -1,
        [Description("Road To Riches")]
        RoadToRiches = 0,
        [Description("Fleet Carrier Route")]
        FleetCarrier,
        [Description("Neutron Plotter")]
        NeutronRoute,
        [Description("Galaxy Plotter")]
        GalaxyPlotter,
        [Description("World Type Route")]
        WorldTypeRoute,
        [Description("Tourist Route")]
        TouristRoute,
        [Description("Exobiology Route")]
        Exobiology,
        [Description("Colonisation Plotter")]
        Colonisation = 8,
        [Description("Trade Planner")]
        TradePlanner = 10
    }
}
