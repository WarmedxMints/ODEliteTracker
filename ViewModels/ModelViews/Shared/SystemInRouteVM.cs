using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.ViewModels.ModelViews.Shared
{
    public sealed class SystemInRouteVM(SystemInRoute routeSystem, SystemInRouteVM? previous)
    {
        private readonly SystemInRoute routeSystem = routeSystem;
        private readonly SystemInRouteVM? previous = previous;

        public ulong Address = routeSystem.SystemAddress;
        public string StarClass => routeSystem.StarClass.ToString();
        public string Name => routeSystem.StarSystem;
        internal SystemPositionVM StarPos { get; } = new(routeSystem.StarPos);
        public string Distance => previous == null ? string.Empty : $"{previous.StarPos.DistanceFrom(StarPos):N2} ly";

    }
}
