using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODJournalDatabase.JournalManagement;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Colonisation
{
    public sealed class ColonisationRouteSystemVM : ODObservableObject
    {
        private long systemAddress;
        private int hopNumber = -100;
        private int databaseHopNumber;
        private string systemName;
        private SystemPositionVM position;
        private string starClass;
        private bool built;
        private bool hub;
        private string notes;
        private double distance;
        private double totalDistance;
        private double totalHopDistance;
        private double totalDistanceFromStart;
        private string distanceFromLinetText = string.Empty;
        private readonly ColonisationRoutePlannerViewModel vm;

        public ColonisationRouteSystemVM(ColonisationRouteSystem system, ColonisationRoutePlannerViewModel vm)
        {
            systemAddress = system.SystemAddress;
            databaseHopNumber = system.HopNumber;
            systemName = system.SystemName;
            position = new(system.Position);
            starClass = system.StarClass;
            built = system.Built;
            hub = system.Hub;
            notes = system.Notes;
            this.vm = vm;
        }

        public ColonisationRouteSystemVM(SystemInRoute system, ColonisationRoutePlannerViewModel vm)
        {
            systemAddress = system.SystemAddress;
            systemName = system.StarSystem;
            position = new(system.StarPos);
            starClass = system.StarClass.ToString();
            notes = string.Empty;
            this.vm = vm;
        }

        public ColonisationRouteSystemVM(Jump system, ColonisationRoutePlannerViewModel vm)
        {
            systemAddress = system.Id64;
            systemName = system.Name;
            position = new(system);
            starClass = "?";
            notes = string.Empty;
            this.vm = vm;
        }

        public ColonisationRouteSystemVM(ColonisationRouteSystemVM routeSystem)
        {
            this.vm = routeSystem.vm;
            this.systemAddress = routeSystem.systemAddress;
            this.systemName = routeSystem.systemName;
            this.position = routeSystem.position;
            this.starClass = routeSystem.starClass;
            this.notes = routeSystem.notes;
        }

        public long SystemAddress { get => systemAddress; set { systemAddress = value; OnPropertyChanged(); } }      
        public int HopNumber { get => hopNumber; set { hopNumber = value; OnPropertyChanged(); } }
        public int DataBaseHopNumber => databaseHopNumber;
        public string SystemName { get => systemName; set { systemName = value; OnPropertyChanged(); } }        
        public SystemPositionVM Position { get => position; set { position = value; OnPropertyChanged(); } }
        public string StarClass { get => starClass; set { starClass = value; OnPropertyChanged(); } }
        public bool Built { get => built; set { built = value; OnPropertyChanged(); } }
        public bool Hub { get => hub; set { hub = value; OnPropertyChanged(); } }
        public string Notes { get => notes; set { notes = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasNotes)); vm.SystemDataChanged(); } }
        public double Distance { get => distance; set { distance = value; OnPropertyChanged(); OnPropertyChanged(nameof(GreaterThan15ly)); } }
        public double TotalDistance { get => totalDistance; set { totalDistance = value; OnPropertyChanged(); } }
        public double TotalHopDistance { get => totalHopDistance; set { totalHopDistance = value; OnPropertyChanged(); } }
        public double TotalDistanceFromStart { get => totalDistanceFromStart; set { totalDistanceFromStart = value; OnPropertyChanged(); } }
        public bool GreaterThan15ly => distance > 15d;
        public bool HasNotes => string.IsNullOrEmpty(Notes) == false;
        public string DistanceFromLineText { get => distanceFromLinetText; set { distanceFromLinetText = value; OnPropertyChanged(); } }
        internal void UpdateSystemDetails(SystemInRoute system)
        {
            Position = new(system.StarPos);
            StarClass = system.StarClass.ToString();
        }

        internal void UpdateSystemDetails(Jump system)
        {
            Position = new(system);
        }

        internal ColonisationRouteSystemVM Clone()
        {           
            return new(this);
        }

        internal void Selected(ColonisationRouteSystemVM? colonisationRouteSystemVM, ColonisationRouteSystemVM? targetSystem)
        {
            if (colonisationRouteSystemVM == null || targetSystem == null)
            {
                DistanceFromLineText = string.Empty;
                return;
            }

            var nearestPointOnLine = SystemPositionVM.GetClosestPointOnLineSegment(colonisationRouteSystemVM.Position.Position, targetSystem.Position.Position, Position.Position);

            var distance = nearestPointOnLine.DistanceFrom(position.Position);

            DistanceFromLineText = $"{distance:N2} ly from a direct line between start and target";
        }
    }
}
