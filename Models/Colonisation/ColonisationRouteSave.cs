namespace ODEliteTracker.Models.Colonisation
{
    internal sealed class ColonisationRouteSave
    {
        public List<ColonisationRouteSystem> RouteSystems { get; set; } = [];
        public ColonisationRouteSystem? TargetSystem { get; set; }
    }
}
