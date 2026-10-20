using ODEliteTracker.Extensions;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.ViewModels.ModelViews.Spansh;

namespace ODEliteTracker.Models.Spansh
{
    public record ApiArgs
    {
        public SpanshRouteTypeComboBox RouteType { get; }
        public ApiRouteSystemVM SourceSystem { get; }
        public ApiRouteSystemVM TargetSystem { get; }
        public List<ApiRouteSystemVM> AdditionSystems { get; } = [];
        public List<long> IntProperties { get; } = [];
        public List<double> DoubleProperties { get; } = [];
        public List<bool> BoolProperties { get; } = [];
        public List<string>? StringArray { get; }
        public string? JobId { get; set; }
        public string? ShipBuild { get; set; }
        public ApiArgs(SpanshRouteTypeComboBox RouteType,
                  ApiRouteSystemVM SourceSystem,
                  ApiRouteSystemVM TargetSystem,
                  List<ApiRouteSystemVM> AdditionSystems,
                  List<long> IntProperties,
                  List<double> DoubleProperties,
                  List<bool> BoolProperties,
                  string? JobId)
        {
            this.RouteType = RouteType;
            this.SourceSystem = SourceSystem;
            this.TargetSystem = TargetSystem;
            this.AdditionSystems = AdditionSystems;
            this.IntProperties = IntProperties;
            this.DoubleProperties = DoubleProperties;
            this.BoolProperties = BoolProperties;
            this.JobId = JobId;
        }

        public ApiArgs(SpanshRouteTypeComboBox RouteType,
                  ApiRouteSystemVM SourceSystem,
                  ApiRouteSystemVM TargetSystem,
                  string? JobId)
        {
            this.RouteType = RouteType;
            this.SourceSystem = SourceSystem;
            this.TargetSystem = TargetSystem;
            this.JobId = JobId;
        }

        public ApiArgs(SpanshRouteTypeComboBox RouteType,
                 ApiRouteSystemVM SourceSystem,
                 ApiRouteSystemVM TargetSystem,
                 string ShipBuild,
                 string? JobId)
        {
            this.RouteType = RouteType;
            this.SourceSystem = SourceSystem;
            this.TargetSystem = TargetSystem;
            this.ShipBuild = ShipBuild;
            this.JobId = JobId;
        }
        //Galaxy plotter route
        public ApiArgs(ShipStatsInfo ship, ApiArgsVM vm)
        {
            RouteType = SpanshRouteTypeComboBox.GalaxyPlotter;
            SourceSystem = vm.SourceSystem;
            TargetSystem = vm.TargetSystem;
            ShipBuild = ship.ShipBuild;
            DoubleProperties =
            [
                ship.BaseMass,
                ship.Fuelmul,
                ship.FuelPower,
                ship.FuelReserve,
                ship.MaxfuelPerJump,
                ship.OptimalMass,
                ship.GFSDRangeBoost,
                ship.SuperChargeMultipler,
                ship.TankSize,               
            ];
            IntProperties =
            [
                vm.IntProperty1, //Cargo
                vm.IntProperty2, //Calculation Time
                vm.IntProperty3, //Reserved Fuel
                (int)vm.Algorithm     //Routing Algorithm
            ];
            BoolProperties =
            [
                vm.BoolProperty1, //Already Supercharged
                vm.BoolProperty2, //Use Supercharge
                vm.BoolProperty3, //Use FSD Injections Always
                vm.BoolProperty4, //Use FSD Injections When Required
                vm.BoolProperty5, //Exclude Secondary Stars
                vm.BoolProperty6  //Refuel Every Scoopable
            ];
        }
        //world type route
        public ApiArgs(SpanshRouteTypeComboBox routeType,
            ApiRouteSystemVM sourceSystem,
            ApiRouteSystemVM targetSystem,
            string? JobId,
            List<string> bodyTypes,
            ApiArgsVM vm) : this(routeType, sourceSystem, targetSystem, JobId)
        {
            StringArray = bodyTypes;
            DoubleProperties = [vm.DoubleProperty1, vm.DoubleProperty2];
            IntProperties = [vm.IntProperty1, vm.IntProperty2, vm.IntProperty3];
            BoolProperties = [vm.BoolProperty5, vm.BoolProperty6];
        }

        public SpanshApiParameters GetApiParamters()
        {
            return RouteType switch
            {
                SpanshRouteTypeComboBox.RoadToRiches => new ApiRoadToRichesParameters()
                {
                    From = SourceSystem.SystemName,
                    To = TargetSystem.HasValue ? TargetSystem.SystemName : null,
                    Range = DoubleProperties[0],
                    Radius = DoubleProperties[1],
                    MaxResults = IntProperties[0],
                    MinValue = IntProperties[1],
                    MaxDistance = IntProperties[2],
                    UseMappingValue = BoolProperties[0].ToInt(),
                    Loop = BoolProperties[1].ToInt(),
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.NeutronRoute => new ApiNeutronParameters()
                {
                    From = SourceSystem.SystemName,
                    To = TargetSystem.SystemName,
                    Range = DoubleProperties[0],
                    SuperchargeMultiplier = BoolProperties[0].ToInt(6, 4),
                    Efficiency = IntProperties[0],
                    Via = [.. AdditionSystems.Select(x => x.SystemName)],
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.Colonisation => new ApiColonisationParameters()
                {
                    SourceSystem = SourceSystem.SystemName,
                    DestinationSystem = TargetSystem.SystemName,
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.FleetCarrier => new ApiFleetCarrierRouteParameters()
                {
                    SourceSystem = SourceSystem.SystemName,
                    DestinationSystems = [.. AdditionSystems.Select(x => x.SystemName)],
                    Mass = BoolProperties[0] ? 60000 : 25000,
                    CapacityUsed = IntProperties[0],
                    CurrentFuel = IntProperties[1],
                    Capacity = BoolProperties[0] ? 60000 : 25000,
                    CalculateStartingFuel = BoolProperties[1].ToInt(),
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.GalaxyPlotter => new ApiGalaxyRouteParameters()
                {
                    SourceSystem = SourceSystem.SystemName,
                    DestinationSystem = TargetSystem.SystemName,
                    ShipBuild = ShipBuild ?? string.Empty,
                    BaseMass = DoubleProperties[0],
                    FuelMultiplier = DoubleProperties[1],
                    FuelPower = DoubleProperties[2],
                    InternalTankSize = DoubleProperties[3],
                    MaxFuelPerJump = DoubleProperties[4],
                    OptimalMass = DoubleProperties[5],
                    RangeBoost = DoubleProperties[6],
                    SuperchargeMultiplier = (int)DoubleProperties[7],
                    TankSize = (int)DoubleProperties[8],
                    IsSupercharged =BoolProperties[0].ToInt(),
                    UseSupercharge = BoolProperties[1].ToInt(),
                    UseInjections = BoolProperties[2].ToInt(),
                    UseInjectionsWhenRequired = BoolProperties[3].ToInt(),
                    ExcludeSecondary = BoolProperties[4].ToInt(),
                    RefuelEveryScoopable = BoolProperties[5].ToInt(),
                    Cargo = IntProperties[0],
                    MaxTime = (int)IntProperties[1],
                    ReserveSize = IntProperties[2],                    
                    Algorithm = (SpanshRoutingAlgorithm)IntProperties[3],
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.WorldTypeRoute => new ApiWorldTypeParameters()
                {
                    From = SourceSystem.SystemName,
                    To = TargetSystem.HasValue ? TargetSystem.SystemName : null,
                    AvoidThargoids = BoolProperties[0].ToInt(),
                    BodyTypes = StringArray ?? [],
                    Loop = BoolProperties[1].ToInt(),
                    MaxResults = IntProperties[0],
                    MinValue = 1,
                    Radius = DoubleProperties[1],
                    Range = DoubleProperties[0],
                    MaxDistance = IntProperties[2],
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.Exobiology => new ApiExobiologyRouteParameters()
                {
                    From = SourceSystem.SystemName,
                    To = TargetSystem.HasValue ? TargetSystem.SystemName : null,
                    Range = DoubleProperties[0],
                    Radius = DoubleProperties[1],
                    MaxResults = IntProperties[0],
                    UseMappingValue = 1,
                    MinValue = IntProperties[2],
                    MaxDistance = IntProperties[1],
                    Loop = BoolProperties[1].ToInt(),
                    JobId = JobId
                },
                SpanshRouteTypeComboBox.TouristRoute => new ApiTouristRouteApiParameters()
                {
                    Source = SourceSystem.SystemName,
                    FinalDestination = TargetSystem.HasValue ? TargetSystem.SystemName : null,
                    Destinations = [.. AdditionSystems.Select(x => x.SystemName)],
                    Range = DoubleProperties[0],
                    Loop = BoolProperties[0].ToInt(),
                    JobId = JobId
                },
                _ => throw new NotImplementedException("Spansh Route Type Not Implement in ApiArgs")
            };
        }
    }
}
