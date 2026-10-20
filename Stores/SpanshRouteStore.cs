using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Database;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Services;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Stores
{
    public sealed class SpanshRouteStore : LogProcessorBase
    {
        public SpanshRouteStore(IManageJournalEvents parserStore,
                              IODDatabaseProvider odToolsDatabase,
                              SettingsStore settingsStore,
                              NotificationService notificationService,
                              SharedDataStore sharedData,
                              SpanshClient spanshClient)
        {
            this.parserStore = parserStore;
            this.settingsStore = settingsStore;
            this.notificationService = notificationService;
            this.sharedData = sharedData;
            this.spanshClient = spanshClient;
            databaseProvider = (ODEliteTrackerDatabaseProvider)odToolsDatabase;

            parserStore.RegisterLogProcessor(this);
        }

        private readonly IManageJournalEvents parserStore;
        private readonly SettingsStore settingsStore;
        private readonly NotificationService notificationService;
        private readonly SharedDataStore sharedData;
        private readonly SpanshClient spanshClient;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private List<SpanshRouteContainer> containers = [];
        private SpanshRouteType currentContainerType;
        public event EventHandler<SpanshTargetSystem?>? OnCurrentTargetChanged;
        public event EventHandler<SpanshRouteContainer?>? OnCurrentContainerChanged;

        public List<SpanshRouteContainer> Containers => containers;
        public SpanshRouteContainer? CurrentContainer { get; private set; }

        private SpanshTargetSystem? currentTarget;
        public SpanshTargetSystem? CurrentTarget
        {
            get => currentTarget;

            private set
            {
                if (currentTarget != value)
                {
                    currentTarget = value;
                    OnCurrentTargetChanged?.Invoke(this, currentTarget);
                }
            }
        }

        public SpanshTargetSystem? NextTarget
        {
            get
            {
                if (CurrentContainer is null || CurrentContainer.Targets.Count == 0 || CurrentContainer.CurrentIndex > CurrentContainer.Targets.Count - 2)
                {
                    return null;
                }

                return CurrentContainer.Targets[CurrentContainer.CurrentIndex + 1];
            }
        }
        public int CurrentIndex
        {
            get
            {
                if (CurrentContainer == null)
                    return -1;

                return CurrentContainer.CurrentIndex;
            }
            set
            {
                if (CurrentContainer == null)
                {

                    return;
                }

                var targets = CurrentContainer.Targets;

                if (targets == null || targets.Count == 0)
                {
                    CurrentContainer.CurrentIndex = 0;
                    return;
                }

                CurrentContainer.CurrentIndex = Math.Clamp(value, 0, targets.Count - 1);

                CurrentTarget = targets[CurrentContainer.CurrentIndex];
            }
        }

        public string SelectedShipName { get; set; } = string.Empty;
        private void SetCurrentContainer(SpanshRouteContainer container)
        {
            currentContainerType = container.RouteType;
            container.LastUsed = DateTime.UtcNow;
            CurrentTarget = null;
            CurrentContainer = Containers.FirstOrDefault(x => x == container);
            OnCurrentContainerChanged?.Invoke(this, CurrentContainer);
        }

        public SpanshRouteContainer? DeleteContainer(SpanshRouteContainer container)
        {
            containers.Remove(container);
            databaseProvider.DeleteSpanshRoute(container, settingsStore.SelectedCommanderID);
            CurrentContainer = Containers.OrderByDescending(x => x.LastUsed).FirstOrDefault();
            return CurrentContainer;
        }

        public SpanshRouteContainer? GetCurrentContainer(SpanshRouteType routeType)
        {
            if (CurrentContainer != null && CurrentContainer.RouteType == routeType)
                return CurrentContainer;

            var container = Containers.Where(x => x.RouteType == routeType).OrderByDescending(x => x.LastUsed).FirstOrDefault();

            if (container != null)
            {
                CurrentContainer = container;
                CurrentContainer.LastUsed = DateTime.UtcNow;
                return CurrentContainer;
            }

            CurrentContainer = null;
            CurrentIndex = -1;
            return null;
        }

        public void SaveContainers()
        {
            databaseProvider.SaveSpanshRoutes(Containers, settingsStore.SelectedCommanderID);
        }

        #region IProcessJournalLogs Implementation

        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.Fileheader, true },
                { JournalTypeEnum.Location, false },
                { JournalTypeEnum.CarrierLocation, false },
                { JournalTypeEnum.FSDJump, false},
                { JournalTypeEnum.CarrierJump, false},
            };
        }

        public override string StoreName => "Spansh Route Store";

        public override void ClearData()
        {
            IsLive = false;
            containers.Clear();
            CurrentContainer = null;
            CurrentTarget = null;
        }

        public override void Dispose()
        {
            parserStore.UnregisterLogProcessor(this);
        }

        public override void SaveData()
        {
            SaveContainers();
        }

        public override DateTime GetJournalAge(DateTime defaultAge)
        {
            return DateTime.UtcNow;
        }

        public override Task ParseHistoryStream(JournalEntry entry)
        {
            ParseJournalEvent(entry);
            return Task.CompletedTask;
        }

        public override void ParseJournalEvent(JournalEntry evt)
        {
            if (EventsToParse.ContainsKey(evt.EventType) == false)
                return;

            switch (evt.EventData)
            {
                case LocationEvent.LocationEventArgs location:
                    OnCurrentSystemChanged(location.StarSystem);
                    break;
                case CarrierLocationEvent.CarrierLocationEventArgs carrierLocation:
                    //Carrier location event is fired when logging in so check that isn't the case
                    if (carrierLocation.Timestamp - sharedData.LastLoginTime < TimeSpan.FromSeconds(10))
                        break;
                    OnCurrentSystemChanged(carrierLocation.StarSystem);
                    break;
                case FSDJumpEvent.FSDJumpEventArgs fsdJump:
                    OnCurrentSystemChanged(fsdJump.StarSystem);
                    break;
                case CarrierJumpEvent.CarrierJumpEventArgs carrierJump:
                    OnCurrentSystemChanged(carrierJump.StarSystem);
                    break;
            }
        }

        public override void RunAfterParsingHistory()
        {
            var databaseContainers = databaseProvider.GetSpanshRoutes(settingsStore.SelectedCommanderID);
            containers = databaseContainers;

            CurrentContainer = Containers.OrderByDescending(x => x.LastUsed).FirstOrDefault();
            base.RunAfterParsingHistory();
        }
        #endregion

        private void OnCurrentSystemChanged(string? system)
        {
            if (parserStore.IsLive == false || string.IsNullOrEmpty(system) || CurrentContainer is null || CurrentContainer.Targets.Count < 1)
            {
                return;
            }

            if (NextTarget is not null && NextTarget.SystemName.Equals(system, StringComparison.OrdinalIgnoreCase))
            {
                int index = CurrentContainer.Targets.IndexOf(NextTarget);

                if (index > CurrentIndex)
                {
                    CurrentIndex = index;

                    if (currentContainerType == SpanshRouteType.GalaxyPlotter
                    && string.IsNullOrEmpty(CurrentTarget?.Property3) == false
                    && CurrentTarget.Property3.Contains("Yes", StringComparison.OrdinalIgnoreCase))
                    {
                        notificationService.ShowBasicNotification(new Notifications.NotificationArgs(NextTarget.SystemName, ["Refuel System"], Models.Settings.NotificationOptions.SpanshRefuel));
                    }

                    if (settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard && string.IsNullOrEmpty(NextTarget?.SystemName) == false)
                    {
                        notificationService.SetClipboard(NextTarget.SystemName);
                    }
                }

                return;
            }

            if (settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard == false)
                return;

            if (NextTarget is not null && CurrentTarget is not null && CurrentTarget.SystemName.Equals(system, StringComparison.OrdinalIgnoreCase))
            {
                notificationService.SetClipboard(NextTarget.SystemName);
            }
        }

        public SpanshRouteContainer? ParseCSV(string filename)
        {
            var csv = SpanshCSVParser.ParseCsv(filename);

            if (csv is null)
            {
                return null;
            }

            return ParseCSV(csv);
        }

        public SpanshRouteContainer? ForceParseCSV(string filename, SpanshRouteType csvType)
        {
            var csv = SpanshCSVParser.ForceParse(filename, csvType);

            if (csv is null)
            {
                return null;
            }

            return ParseCSV(csv);
        }

        private SpanshRouteContainer? ParseCSV(CsvParserReturn csv)
        {
            var known = containers.FirstOrDefault(x => x.RouteType == csv.CsvType && x.CreatedDate == csv.FileCreationDate);

            if (known != null)
            {
                known.LastUsed = DateTime.UtcNow;
                SetCurrentContainer(known);
                return known;
            }

            var container = new SpanshRouteContainer(csv, SelectedShipName);

            containers.Add(container);
            SetCurrentContainer(container);
            return container;
        }

        public async Task<List<ApiRouteSystemVM>> CheckSystemNamesWithSpansh(ApiArgs args)
        {
            List<ApiRouteSystemVM> systems = GetSystemNames(args);
            //Store a list of all the names we get in each in case one matches something later in the list
            HashSet<string> spanshResults = [];

            List<ApiRouteSystemVM> systemsNotFound = [];
            foreach (var system in systems)
            {
                if (system.Status != NameCheckStatus.Unchecked)
                    continue;
                if (spanshResults.Contains(system.SystemName, StringComparer.InvariantCultureIgnoreCase))
                {
                    system.Status = NameCheckStatus.Good;
                    continue;
                }

                var results = await spanshClient.GetSystemNames(system.SystemName);

                if (results == null)
                {
                    if (systemsNotFound.Contains(system))
                        continue;
                    system.Status = NameCheckStatus.Bad;
                    systemsNotFound.Add(system);
                    continue;
                }
                foreach (var result in results.Results)
                {
                    if (spanshResults.Contains(result.Name, StringComparer.InvariantCultureIgnoreCase) == false)
                        spanshResults.Add(result.Name);

                    if (result.Name.Contains(system.SystemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        system.Id64 = result.Id64;
                        system.Status = NameCheckStatus.Good;
                        break;
                    }
                }
                if (system.Status == NameCheckStatus.Good)
                    continue;
                if (systemsNotFound.Contains(system))
                    continue;
                system.Status = NameCheckStatus.Bad;
                systemsNotFound.Add(system);
            }
            return systemsNotFound;
        }

        private static List<ApiRouteSystemVM> GetSystemNames(ApiArgs args)
        {
            List<ApiRouteSystemVM> ret = [];

            if (args.SourceSystem.HasValue)
                ret.Add(args.SourceSystem);

            if (args.TargetSystem.HasValue)
                ret.Add(args.TargetSystem);

            if (args.AdditionSystems.Any(x => x.HasValue))
                ret.AddRange(args.AdditionSystems.Where(x => x.HasValue));
            //TODO
            switch (args.RouteType)
            {
                case SpanshRouteTypeComboBox.RoadToRiches:
                    break;
                case SpanshRouteTypeComboBox.FleetCarrier:
                    break;
                case SpanshRouteTypeComboBox.NeutronRoute:
                    break;
                case SpanshRouteTypeComboBox.GalaxyPlotter:
                    break;
                case SpanshRouteTypeComboBox.WorldTypeRoute:
                    break;
                case SpanshRouteTypeComboBox.TouristRoute:
                    break;
                case SpanshRouteTypeComboBox.Exobiology:
                    break;
                case SpanshRouteTypeComboBox.Colonisation:
                    break;
            }
            return ret;
        }

        public async Task<(bool, SpanshResponseBase?)> GenerateRoute(ApiArgs args, Action<string>? textUpdate)
        {
            switch (args.RouteType)
            {
                case SpanshRouteTypeComboBox.RoadToRiches:
                    if (args.GetApiParamters() is not ApiRoadToRichesParameters r2rParams)
                        return (false, null);
                    var r2rRoute = await spanshClient.GetApiRoute(r2rParams, textUpdate) as ApiRoadToRichesRoute;
                    if (r2rRoute != null && CheckSpanshResponse(r2rRoute))
                    {
                        GenerateRoute(r2rRoute);
                        return (true, r2rRoute);
                    }
                    return (false, r2rRoute);
                case SpanshRouteTypeComboBox.FleetCarrier:
                    if (args.GetApiParamters() is not ApiFleetCarrierRouteParameters fcParams)
                        return (false, null);
                    var fcRoute = await spanshClient.GetApiRoute(fcParams, textUpdate) as ApiFleetCarrierRoute;
                    if (fcRoute != null && CheckSpanshResponse(fcRoute))
                    {
                        GenerateRoute(fcRoute);
                        return (true, fcRoute);
                    }
                    break;
                case SpanshRouteTypeComboBox.NeutronRoute:
                   if (args.GetApiParamters() is not ApiNeutronParameters neutronParams)
                        return (false, null);
                    var neutronRoute = await spanshClient.GetApiRoute(neutronParams, textUpdate) as ApiNeutronRoute;
                    if (neutronRoute != null && CheckSpanshResponse(neutronRoute))
                    {
                        GenerateRoute(neutronRoute);
                        return (true, neutronRoute);
                    }
                    return (false, neutronRoute);
                case SpanshRouteTypeComboBox.GalaxyPlotter:
                    if (args.GetApiParamters() is not ApiGalaxyRouteParameters galaxyParams)
                        return (false, null);
                    var galaxyRoute = await spanshClient.GetApiRoute(galaxyParams, textUpdate) as ApiGalaxyPlotterRoute;
                    if (galaxyRoute != null && CheckSpanshResponse(galaxyRoute))
                    {
                        GenerateRoute(galaxyRoute);
                        return (true, galaxyRoute);
                    }
                    break;
                case SpanshRouteTypeComboBox.WorldTypeRoute:
                    if (args.GetApiParamters() is not ApiWorldTypeParameters worldParams)
                        return (false, null);
                    var worldRoute = await spanshClient.GetApiRoute(worldParams, textUpdate) as ApiWorldTypeRoute;
                    if (worldRoute != null && CheckSpanshResponse(worldRoute))
                    {
                        GenerateRoute(worldRoute);
                        return (true, worldRoute);
                    }
                    break;
                case SpanshRouteTypeComboBox.TouristRoute:
                    if (args.GetApiParamters() is not ApiTouristRouteApiParameters touristParams)
                        return (false, null);
                    var tourist = await spanshClient.GetApiRoute(touristParams, textUpdate) as ApiTouristRoute;

                    if (tourist != null && CheckSpanshResponse(tourist))
                    {
                        GenerateRoute(tourist);
                        return (true, tourist);
                    }
                    break;
                case SpanshRouteTypeComboBox.Exobiology:
                    if (args.GetApiParamters() is not ApiExobiologyRouteParameters exoParams)
                        return (false, null);
                    var exuRoute = await spanshClient.GetApiRoute(exoParams, textUpdate) as ApiExoviologyRoute;
                    if (exuRoute != null && CheckSpanshResponse(exuRoute))
                    {
                        GenerateRoute(exuRoute);
                        return (true, exuRoute);
                    }
                    break;
                case SpanshRouteTypeComboBox.Colonisation:
                    if (args.GetApiParamters() is not ApiColonisationParameters colPamars)
                        return (false, null);
                    var colonisationRoute = await spanshClient.GetApiRoute(colPamars, textUpdate) as ApiColonisationRoute;

                    if (colonisationRoute != null && CheckSpanshResponse(colonisationRoute))
                    {
                        GenerateRoute(colonisationRoute);
                        return (true, colonisationRoute);
                    }
                    return (false, colonisationRoute);
            }
            return (false, null);
        }

        internal static bool CheckSpanshResponse(SpanshResponseBase? response)
        {
            if (response == null || response.State != "completed" || response.Status != "ok")
                return false;
            return true;
        }
        internal bool GenerateRoute(SpanshResponseBase route)
        {
            if (route == null)
                return false;

            var container = SpanshRouteContainer.CreateFromApi(route, SelectedShipName);
            if (container == null || !container.HasValue)
                return false;
            Containers.Add(container);
            SetCurrentContainer(container);
            return true;
        }

        internal void SetSelectedSystem(SpanshTargetSystem target)
        {
            var index = CurrentContainer?.Targets.IndexOf(target);

            if (index != null && index is int i && i >= 0)
            {
                CurrentIndex = i;
            }
        }

        internal void LoadContainer(SpanshRouteContainer routeContainer)
        {
            SetCurrentContainer(routeContainer);
        }
    }
}
