using ODEliteTracker.Models;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Models.Spansh;
using ODMVVM.Extensions;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class ApiArgsVM : ODObservableObject
    {
        private readonly Dictionary<string, bool> boolValues = [];
        private readonly Dictionary<string, long> intValues = [];
        private readonly Dictionary<string, double> doubleValues = [];
        public string? JobID { get; set; }
        public SpanshRouteTypeComboBox RouteType { get; private set; }
        private ApiRouteSystemVM sourceSystem = new();
        public ApiRouteSystemVM SourceSystem { get => sourceSystem; set { sourceSystem = value; OnPropertyChanged(nameof(SourceSystem)); } }
        private ApiRouteSystemVM targetSystem = new();
        public ApiRouteSystemVM TargetSystem { get => targetSystem; set { targetSystem = value; OnPropertyChanged(nameof(TargetSystem)); } }
        private ObservableCollection<ApiRouteSystemVM> additionalSystems = [];
        /// <summary>
        /// Neutron Plotter = Via System List
        /// R2R = Not Used
        /// Colonisation = Not Used
        /// Fleet Carrier = Destinations List
        /// </summary>
        public ObservableCollection<ApiRouteSystemVM> AdditionalSystems { get => additionalSystems; set { additionalSystems = value; OnPropertyChanged(nameof(AdditionalSystems)); } }
        private ApiRouteSystemVM? selectAdditional;
        public ApiRouteSystemVM? SelectedAdditional
        {
            get => selectAdditional;
            set
            {
                selectAdditional = value;
                if (selectAdditional != null)
                    AdditionSystem = selectAdditional;
                OnPropertyChanged(nameof(SelectedAdditional));
            }
        }
        /// <summary>
        /// Neutron Plotter = Effceincy
        /// R2R = Maximum Systems
        /// Colonisation = Not Used
        /// Fleet Carrier = Used Capacity
        /// </summary>
        public long IntProperty1 { get => GetInt(); set { SetInt(value); OnPropertyChanged(); } }
        /// <summary>
        /// Neutron Plotter = Not Used
        /// R2R = Not Used
        /// Colonisation = Not Used
        /// Fleet Carrier = Fuel Tank Level
        /// </summary>
        public long IntProperty2 { get => GetInt(); set { SetInt(value); OnPropertyChanged(); } }
        public long IntProperty3 { get => GetInt(); set { SetInt(value); OnPropertyChanged(); } }
        /// <summary>
        /// Neutron Plotter = Range
        /// R2R = Range
        /// Colonisation = Not Used
        /// Fleet Carrier = Not Used
        /// </summary>
        public double DoubleProperty1 { get => GetDouble(); set { SetDouble(value); OnPropertyChanged(); OnPropertyChanged(nameof(DoubleProperty1Text)); } }
        /// <summary>
        /// Neutron Plotter = Not Used
        /// R2R = Radius
        /// Colonisation = Not Used
        /// Fleet Carrier = Not Used
        /// </summary>
        public double DoubleProperty2 { get => GetDouble(); set { SetDouble(value); OnPropertyChanged(); OnPropertyChanged(nameof(DoubleProperty2Text)); } }
        /// <summary>
        /// Neutron Plotter = Supercharge
        /// R2R = Use Mapping Value
        /// Colonisation = Not Used
        /// Fleet Carrier = Carrier Type
        /// </summary>
        //public bool BoolProperty1 { get => boolPropety1; set { boolPropety1 = value; OnPropertyChanged(nameof(BoolProperty1)); } }
        public bool BoolProperty1 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }
        /// <summary>
        /// Neutron Plotter = Not Used
        /// R2R = Loop
        /// Colonisation = Not Used
        /// Fleet Carrier = Determine Tritium Requirements
        /// </summary>
        public bool BoolProperty2 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }
        public bool BoolProperty3 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }
        public bool BoolProperty4 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }
        public bool BoolProperty5 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }
        public bool BoolProperty6 { get => GetBool(); set { Setbool(value); OnPropertyChanged(); } }

        private string doubleProperty1Text = string.Empty;
        public string DoubleProperty1Text
        {
            get
            {
                if (string.IsNullOrEmpty(doubleProperty1Text))
                {
                    return $"{DoubleProperty1}";
                }
                return doubleProperty1Text;
            }
            set
            {
                if (double.TryParse(value, out var result))
                {
                    DoubleProperty1 = result;
                }
                doubleProperty1Text = value;
                OnPropertyChanged(nameof(DoubleProperty1Text));
            }
        }

        private string doubleProperty2Text = string.Empty;
        public string DoubleProperty2Text
        {
            get
            {
                if (string.IsNullOrEmpty(doubleProperty2Text))
                {
                    return $"{DoubleProperty2}";
                }
                return doubleProperty2Text;
            }
            set
            {
                if (double.TryParse(value, out var result))
                {
                    DoubleProperty2 = result;
                }
                doubleProperty2Text = value;
                OnPropertyChanged(nameof(DoubleProperty2Text));
            }
        }

        private ApiRouteSystemVM additionalSystem = new();
        public ApiRouteSystemVM AdditionSystem { get => additionalSystem; set { additionalSystem = value; OnPropertyChanged(nameof(AdditionSystem)); OnPropertyChanged(nameof(CanAddAdditional)); } }

        public SpanshRoutingAlgorithm Algorithm { get => (SpanshRoutingAlgorithm)GetInt(); set { SetInt((int)value); OnPropertyChanged(); } }

        private bool _isLoaded;
        public ApiArgsVM()
        {
            PropertyChanged += ApiArgsVM_PropertyChanged;
            additionalSystems.CollectionChanged += AdditionalSystems_CollectionChanged;
        }

        public ApiArgsVM(SpanshRouteTypeComboBox routeType, string? currentSystem)
        {
            RouteType = routeType;

            if (!string.IsNullOrEmpty(currentSystem))
            {
                SourceSystem = new ApiRouteSystemVM() { SystemName = currentSystem };
            }
            LoadDefaults();

            PropertyChanged += ApiArgsVM_PropertyChanged;
            additionalSystems.CollectionChanged += AdditionalSystems_CollectionChanged;
        }

        internal void LoadDefaults()
        {
            //Default Values of each type
            switch (RouteType)
            {
                case SpanshRouteTypeComboBox.Exobiology:
                case SpanshRouteTypeComboBox.RoadToRiches:
                    IntProperty1 = 100;
                    DoubleProperty2 = 25;
                    BoolProperty1 = true;
                    BoolProperty2 = true;
                    break;
                case SpanshRouteTypeComboBox.FleetCarrier:
                    BoolProperty2 = true;
                    break;
                case SpanshRouteTypeComboBox.NeutronRoute:
                    IntProperty1 = 60;
                    DoubleProperty2 = 25;
                    break;
                case SpanshRouteTypeComboBox.GalaxyPlotter:
                    DoubleProperty1Text = "10";
                    IntProperty2 = 60;
                    IntProperty3 = 10;
                    BoolProperty2 = true;
                    BoolProperty6 = true;
                    Algorithm = SpanshRoutingAlgorithm.optimistic;
                    break;
                case SpanshRouteTypeComboBox.WorldTypeRoute:
                    DoubleProperty2 = 25;
                    IntProperty1 = 100;
                    BoolProperty1 = true;
                    BoolProperty5 = true;
                    BoolProperty6 = true;
                    IntProperty3 = 50000;
                    break;
                case SpanshRouteTypeComboBox.TouristRoute:
                    BoolProperty1 = true;
                    break;
                case SpanshRouteTypeComboBox.Colonisation:
                    break;
            }
        }

        private void ApiArgsVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!_isLoaded)
                return;
            //If any property is changed then we clear the job id as it would be a different query
            JobID = null;
        }

        private void AdditionalSystems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_isLoaded)
                return;
            //If any items are add/removed from the collection then we clear the job id as it would be a different query
            JobID = null;
        }

        public (ApiArgs?, string) GetApiArgs(ShipInfo? ship)
        {
            var response = $"{RouteType.GetEnumDescription()} Args Created";
            return RouteType switch
            {
                SpanshRouteTypeComboBox.RoadToRiches => (new ApiArgs(RouteType,
                   SourceSystem,
                   TargetSystem,
                   [],
                   [IntProperty1, IntProperty2, IntProperty3],
                   [DoubleProperty1, DoubleProperty2],
                   [BoolProperty1, BoolProperty2], JobID), response),
                SpanshRouteTypeComboBox.NeutronRoute => (new ApiArgs(RouteType,
                    SourceSystem,
                    TargetSystem,
                    [.. AdditionalSystems],
                    [IntProperty1],
                    [DoubleProperty1],
                    [BoolProperty1], JobID), response),
                SpanshRouteTypeComboBox.Colonisation => (new ApiArgs(RouteType, SourceSystem, TargetSystem, JobID), response),
                SpanshRouteTypeComboBox.FleetCarrier => (new ApiArgs(RouteType,
                    SourceSystem,
                    new(),
                    [.. AdditionalSystems],
                    [IntProperty1, IntProperty2],
                    [],
                    [BoolProperty1, BoolProperty2], JobID), response),
                SpanshRouteTypeComboBox.GalaxyPlotter => GetGalaxyPlotterArgs(ship),
                SpanshRouteTypeComboBox.WorldTypeRoute => GetWorldTypeArgs(),
                SpanshRouteTypeComboBox.TouristRoute => (new ApiArgs(
                    RouteType,
                    SourceSystem,
                    TargetSystem,
                    [.. AdditionalSystems],
                    [],
                    [DoubleProperty1],
                    [BoolProperty1],
                    JobID), response),
                SpanshRouteTypeComboBox.Exobiology => (new ApiArgs(RouteType,
                    SourceSystem,
                    TargetSystem,
                    [],
                    [IntProperty1, IntProperty2, IntProperty3],
                    [DoubleProperty1, DoubleProperty2],
                    [BoolProperty1, BoolProperty2],
                    JobID), response),
                _ => (null, "Route Not Implemented")
            };

        }

        private (ApiArgs?, string) GetWorldTypeArgs()
        {
            List<string> bodyTypes = [];

            if (BoolProperty1)
                bodyTypes.Add("Earth-like world");
            if (BoolProperty2)
                bodyTypes.Add("Ammonia world");
            if (BoolProperty3)
                bodyTypes.Add("Rocky body");
            if (BoolProperty4)
                bodyTypes.Add("High metal content world");

            var ret = new ApiArgs(RouteType, SourceSystem, TargetSystem, JobID, bodyTypes, this);
            return (ret, string.Empty);

        }

        private (ApiArgs?, string) GetGalaxyPlotterArgs(ShipInfo? ship)
        {
            if (ship == null)
                return (null, "Ship Data Missing");
            var data = ship.GetStats();

            if (data.Item1 == null)
                return (null, "Ship Stats Missing"); ;
            var ret = new ApiArgs(data.Item1, this);

            return (ret, string.Empty);
        }

        internal bool CanGenerate()
        {
            return RouteType switch
            {
                SpanshRouteTypeComboBox.RoadToRiches => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && DoubleProperty1 > 0 && DoubleProperty2 > 0 && IntProperty1 > 0,
                SpanshRouteTypeComboBox.NeutronRoute => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && !string.IsNullOrEmpty(TargetSystem.SystemName)
                                                        && !AdditionalSystems.Any(x => string.IsNullOrEmpty(x.SystemName))
                                                        && DoubleProperty1 > 0 && IntProperty1 > 0,
                SpanshRouteTypeComboBox.Colonisation => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && !string.IsNullOrEmpty(TargetSystem.SystemName),
                SpanshRouteTypeComboBox.FleetCarrier => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && AdditionalSystems.Count > 0,
                SpanshRouteTypeComboBox.GalaxyPlotter => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && !string.IsNullOrEmpty(TargetSystem.SystemName),
                SpanshRouteTypeComboBox.WorldTypeRoute => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && DoubleProperty1 > 0
                                                        && DoubleProperty2 > 0
                                                        && IntProperty1 > 0
                                                        && (BoolProperty1 || BoolProperty2 || BoolProperty3 || BoolProperty4),
                SpanshRouteTypeComboBox.Exobiology => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && DoubleProperty1 > 0 && DoubleProperty2 > 0 && IntProperty1 > 0,
                SpanshRouteTypeComboBox.TouristRoute => !string.IsNullOrEmpty(SourceSystem.SystemName)
                                                        && DoubleProperty1 > 0
                                                        && (!string.IsNullOrEmpty(TargetSystem.SystemName) || AdditionalSystems.Count > 0),

                _ => false

            };
        }

        internal bool CanAddAdditional
        {
            get
            {
                if (string.IsNullOrEmpty(additionalSystem.SystemName))
                {
                    return false;
                }

                if (RouteType == SpanshRouteTypeComboBox.TouristRoute)
                    return true;

                if (AdditionalSystems.Contains(AdditionSystem))
                    return true;

                return AdditionalSystems.Count < 10;
            }
        }

        public bool CanImportDestinations => RouteType switch
        {
            SpanshRouteTypeComboBox.NeutronRoute or
            SpanshRouteTypeComboBox.FleetCarrier or
            SpanshRouteTypeComboBox.TouristRoute => true,
            _ => false
        };

        public bool CanAddShipStats => RouteType switch
        {
            SpanshRouteTypeComboBox.RoadToRiches or
            SpanshRouteTypeComboBox.NeutronRoute or
            SpanshRouteTypeComboBox.GalaxyPlotter or
            SpanshRouteTypeComboBox.WorldTypeRoute or
            SpanshRouteTypeComboBox.TouristRoute or
            SpanshRouteTypeComboBox.Exobiology => true,
            _ => false
        };

        internal void AddAdditonalSystem()
        {
            if (string.IsNullOrEmpty(additionalSystem.SystemName))
                return;

            if (AdditionalSystems.Contains(AdditionSystem))
            {
                AdditionSystem = new();
                return;
            }

            AdditionalSystems.AddItem(AdditionSystem);
            AdditionSystem = new();
        }

        internal void ImportShipStats(ShipInfoVM vm)
        {
            switch (RouteType)
            {
                //Check each stat isn't already the same so the user doesn't 
                //clear the job id if things haven't changed
                case SpanshRouteTypeComboBox.WorldTypeRoute:
                case SpanshRouteTypeComboBox.RoadToRiches:
                case SpanshRouteTypeComboBox.NeutronRoute:
                case SpanshRouteTypeComboBox.TouristRoute:
                case SpanshRouteTypeComboBox.Exobiology:
                    if (DoubleProperty1 != vm.JumpRangeFullTank)
                        DoubleProperty1 = vm.JumpRangeFullTank;
                    break;
                case SpanshRouteTypeComboBox.GalaxyPlotter:
                    if (IntProperty1 != vm.CargoCapacity)
                        IntProperty1 = vm.CargoCapacity;
                    break;
                case SpanshRouteTypeComboBox.Colonisation:
                case SpanshRouteTypeComboBox.FleetCarrier:
                default:
                    return;
            }
        }
        private bool GetBool([CallerMemberName] string? name = null)
        {
            if (name == null)
                return false;

            if (boolValues.TryGetValue(name, out var boolValue))
                return boolValue;
            return false;
        }
        private void Setbool(bool value, [CallerMemberName] string? name = null)
        {
            if (name == null)
                return;

            if (boolValues.ContainsKey(name))
            {
                boolValues[name] = value;
                return;
            }

            boolValues.Add(name, value);
        }

        private long GetInt([CallerMemberName] string? name = null)
        {
            if (name == null)
                return 0;
            if (intValues.TryGetValue(name, out var intVlue))
                return intVlue;
            return 0;
        }
        private void SetInt(long value, [CallerMemberName] string? name = null)
        {
            if (name == null)
                return;

            if (intValues.ContainsKey(name))
            {
                intValues[name] = value;
                return;
            }

            intValues.Add(name, value);
        }

        private double GetDouble([CallerMemberName] string? name = null)
        {
            if (name == null)
                return 0;
            if (doubleValues.TryGetValue(name, out var doubleVlue))
            {
                return doubleVlue;
            }
            return 0;
        }
        private void SetDouble(double value, [CallerMemberName] string? name = null)
        {
            if (name == null)
                return;

            if (doubleValues.ContainsKey(name))
            {
                doubleValues[name] = value;
                return;
            }

            doubleValues.Add(name, value);
        }

        internal SpashAPISave GetSave()
        {
            var ret = new SpashAPISave()
            {
                BoolValues = new(this.boolValues),
                IntValues = new(this.intValues),
                DoubleValues = new(this.doubleValues),
                JobID = this.JobID,
                RouteType = this.RouteType,
                SourceSystem = new(sourceSystem),
                TargetSystem = new(targetSystem),
                AdditionalSystems = [.. additionalSystems.Select(x => new ApiRouteSystemSave(x))],
                Algorithm = this.Algorithm,
            };

            return ret;
        }

        internal void LoadFronSave(SpashAPISave save)
        {
            boolValues.Clear();
            foreach (var v in save.BoolValues)
            {
                boolValues.TryAdd(v.Key, v.Value);
            }

            intValues.Clear();
            foreach (var v in save.IntValues)
            {
                intValues.TryAdd(v.Key, v.Value);
            }

            doubleValues.Clear();
            foreach (var v in save.DoubleValues)
            {
                doubleValues.TryAdd(v.Key, v.Value);
            }

            RouteType = save.RouteType;
            SourceSystem = new(save.SourceSystem);
            TargetSystem = new(save.TargetSystem);
            additionalSystems.ClearCollection();
            additionalSystems.AddRange(save.AdditionalSystems.Select(x => new ApiRouteSystemVM(x)));
            Algorithm = save.Algorithm;
            JobID = save.JobID;

            _isLoaded = true;
        }
    }
}
