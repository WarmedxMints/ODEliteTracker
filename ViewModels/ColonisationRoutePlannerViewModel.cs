using Newtonsoft.Json;
using ODEliteTracker.Helpers;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Colonisation;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Position = ODEliteTracker.Models.Galaxy.Position;

namespace ODEliteTracker.ViewModels
{
    public enum AddSystemToRouteEnum
    {
        AtStart,
        BeforeSelected,
        AfterSelected,
        AtEnd
    }
    public sealed class ColonisationRoutePlannerViewModel : ODObservableObject, IDisposable
    {
        private readonly ColonisationStore store;
        private readonly SharedDataStore sharedData;
        private readonly NotificationService notification;
        private readonly SettingsStore settings;
        private readonly PopOutService popOutService;
        private readonly SpanshClient spanshClient;
        private readonly JournalManager journalManager;
        private bool isBusy = true;
        public bool IsBusy { get => isBusy; set { isBusy = value; OnPropertyChanged(); } }
        public ColonisationRoutePlannerViewModel(ColonisationStore store,
                                                 SharedDataStore sharedData,
                                                 NotificationService notification,
                                                 SettingsStore settings,
                                                 PopOutService popOutService,
                                                 SpanshClient spanshClient,
                                                 IManageJournalEvents journalManager)
        {
            this.store = store;
            this.sharedData = sharedData;
            this.notification = notification;
            this.settings = settings;
            this.popOutService = popOutService;
            this.spanshClient = spanshClient;
            this.journalManager = (JournalManager)journalManager;
            store.StoreLive += OnStoreLive;      

            sharedData.CurrentSystemChanged += OnCurrentSystemChanged;
            sharedData.SystemDSSScaned += OnSystemDSSScanned;
            settings.ColonisationSettings.StraightLineStarPosChanged += ColonisationSettings_StraightLineStarPosChanged;
            settings.ColonisationSettings.StraightLineEndPosChanged += ColonisationSettings_StraightLineEndPosChanged;
          
            RemoveAfterSelectedCommand = new ODRelayCommand(RemoveAfterSelected, (_) => SelectedSystem != null);
            ToggleSelectedHubCommand = new ODRelayCommand(ToggleSelectedHub, (_) => SelectedSystem != null);
            ToggleSelectedBuiltCommand = new ODRelayCommand(ToggleSelectedBuilt, (_) => SelectedSystem != null);
            DeleteSelectedSystemCommand = new ODRelayCommand(DeleteSelectedSystem, (_) => SelectedSystem != null);
            SetClipboardCommand = new ODRelayCommand<string>(SetClipboard, (_) => SelectedSystem != null);
            SaveRouteCommand = new ODRelayCommand(SaveRoute, (_) => DataChanged);
            ReloadFromDatabaseCommand = new ODAsyncRelayCommand(LoadFromDatabase);
            SetSelctedAsTargetCommand = new ODRelayCommand(SetSelectedAsTarget, (_) => SelectedSystem != null && SelectedSystem.SystemAddress != targetSystem?.SystemAddress);

            SetStartAsSelectedCommand = new ODRelayCommand(SetStartAsSelected, (_) => SelectedSystem != null);
            SetStartAsCurrentCommand = new ODRelayCommand(SetStartAsCurrent, (_) => sharedData.CurrentSystem != null);
            SetEndAsSelectedCommand = new ODRelayCommand(SetEndAsSelected, (_) => SelectedSystem != null);
            SetEndAsCurrentCommand = new ODRelayCommand(SetEndAsCurrent, (_) => sharedData.CurrentSystem != null);
            SetEndAsTargetCommand = new ODRelayCommand(SetEndAsTargetSystem, (_) => TargetSystem != null);
            AddCurrentSystemToRouteCommand = new ODRelayCommand<AddSystemToRouteEnum>(AddCurrentToRoute, (x) => CanAddCurrentSystem(x));
            SelectCurrentSystemCommand = new ODRelayCommand(SelectCurrentSystem, (x) => RouteSystems.Any(x => x.SystemAddress == sharedData.CurrentSystem?.Address));
            ImportCommandersRouteCommand = new ODRelayCommand<JournalCommanderVM>(ImportCmdrsRoute, (x) => x != null);
            OpenSystemWebPageCommand = new ODRelayCommand<SystemWebSite>(OpenSystemWebPage, (_) => SelectedSystem != null);
            OpenPopOut = new ODRelayCommand<Type>(OnOpenPopOut);

            if (store.IsLive)
            {
                _ = OnStoreLive(null, true);
            }
        }

        private void OpenSystemWebPage(SystemWebSite site)
        {
            if (SelectedSystem == null)
                return;

            StarSystemHelpers.OpenSystemPage(site, SelectedSystem);
        }

        private void ImportCmdrsRoute(JournalCommanderVM vM)
        {
            store.ImportCommandersColonisationRoute(vM.Id);
            var routeSystems = store.RouteSystems.Where(x => x.HopNumber >= 0).Select(x => new ColonisationRouteSystemVM(x, this)).ToList();

            if (!routeSystems.Any())
                return;

            RouteSystems.ClearCollection();
            RouteSystems.AddRange(routeSystems);
            TargetSystem = store.RouteSystems.Where(x => x.HopNumber == -1).Select(x => new ColonisationRouteSystemVM(x, this)).FirstOrDefault();
            CalcDistances();
            CalcLineValues();
            DataChanged = true;
        }

        private void OnOpenPopOut(Type type)
        {
            popOutService.OpenPopOut(type, settings.SelectedCommanderID);
        }

        private void ColonisationSettings_StraightLineEndPosChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(EndPos));
            CalcLineValues();
        }

        private void ColonisationSettings_StraightLineStarPosChanged(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(StartPos));
            CalcLineValues();
        }

        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            if (e == null)
                return;
            CalcLineValues();

            var routeSystem = RouteSystems.FirstOrDefault(x => x.SystemAddress == e.Address);

            if (routeSystem == null)
                return;

            if (RouteClipboardBehavior != ColonisationRouteClipboardBehavior.DoNothing)
                CheckNextSystemOnRoute(routeSystem);

            var colonised = e.FactionCount > 1;

            if (!routeSystem.Built && colonised)
            {
                routeSystem.Built = true;
                DataChanged = true;        
            }

            if (AutoSelect)
            {
                SelectedSystem = routeSystem;
            }
        }

        private void OnSystemDSSScanned(object? sender, StarSystem e)
        {
            var routeSystem = RouteSystems.FirstOrDefault(x => x.SystemAddress == e.Address);

            if (routeSystem == null)
                return;

            Task.Run(() => UpdateBodyCountFromDSSScan(routeSystem, e));
        }

        private async Task UpdateBodyCountFromDSSScan(ColonisationRouteSystemVM routeSystem, StarSystem system)
        {
            //The game will fire the dss event before the scan events
            //We are in no rush to update the data so we will give it a second
            await Task.Delay(1000);
            Application.Current.Dispatcher.Invoke(() =>
            {
                routeSystem.BodyCount = system.BodyCount;
                routeSystem.StarCount = system.Bodies.Count(x => x.Value.StarType != EliteJournalReader.StarType.Unknown);
                DataChanged = true;
            });
        }

        private void CheckNextSystemOnRoute(ColonisationRouteSystemVM routeSystem)
        {
            var index = RouteSystems.IndexOf(routeSystem);

            if (index >= RouteSystems.Count || index < 0)
                return;

            ColonisationRouteSystemVM systemToCopy;

            switch (RouteClipboardBehavior)
            {
                case ColonisationRouteClipboardBehavior.DoNothing:
                    return;
                case ColonisationRouteClipboardBehavior.CopyIfNotBuilt:
                    systemToCopy = RouteSystems[index + 1];
                    if (systemToCopy.Built)
                        return;
                    break;
                case ColonisationRouteClipboardBehavior.CopyNext:
                    systemToCopy = RouteSystems[index + 1];
                    break;
                case ColonisationRouteClipboardBehavior.CopyPrevious:
                    if (index == 0)
                        return;
                    systemToCopy = RouteSystems[index - 1];
                    break;
                default:
                    return;
            }

            SetClipboard(systemToCopy.SystemName);
        }
        #region Route Planner
        public ICommand RemoveAfterSelectedCommand { get; }
        public ICommand ToggleSelectedHubCommand { get; }
        public ICommand ToggleSelectedBuiltCommand { get; }
        public ICommand DeleteSelectedSystemCommand { get; }
        public ICommand SetClipboardCommand { get; }
        public ICommand SaveRouteCommand { get; }
        public ICommand ReloadFromDatabaseCommand { get; }
        public ICommand SetSelctedAsTargetCommand { get; }
        public ICommand AddCurrentSystemToRouteCommand { get; }
        public ICommand SelectCurrentSystemCommand { get; }
        public ICommand OpenPopOut { get; }
        public ICommand ImportCommandersRouteCommand { get; }
        public ICommand OpenSystemWebPageCommand { get; }
        private ObservableCollection<ColonisationRouteSystemVM> routeSystems = [];
        public ObservableCollection<ColonisationRouteSystemVM> RouteSystems { get => routeSystems; set { routeSystems = value; OnPropertyChanged(); } }

        private ColonisationRouteSystemVM? targetSystem;
        public ColonisationRouteSystemVM? TargetSystem { get => targetSystem; set { targetSystem = value; OnPropertyChanged(); } }

        private ColonisationRouteSystemVM? selectedSystem;
        public ColonisationRouteSystemVM? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                selectedSystem = value;
                selectedSystem?.Selected(RouteSystems.FirstOrDefault(), targetSystem);
                if (selectedSystem != null && !IsBusy)
                {
                    settings.ColonisationSettings.SelectedPlannerAddress = selectedSystem.SystemAddress;
                    SelectedSystemChanged?.Invoke(this, selectedSystem);
                }
                OnPropertyChanged();
            }
        }
        public ObservableCollection<JournalCommanderVM> LiveCommanders { get; } = [];
        public bool HaveCommders => LiveCommanders.Count > 0;
        public bool DataChanged { get; private set; }
        public bool UseSpansh { get => settings.ColonisationSettings.CheckSpansh; set { settings.ColonisationSettings.CheckSpansh = value; OnPropertyChanged(); } }
        public bool AutoSelect { get => settings.ColonisationSettings.AutoSelect; set { settings.ColonisationSettings.AutoSelect = value; OnPropertyChanged(); } }
        public ColonisationRouteClipboardBehavior RouteClipboardBehavior { get => settings.ColonisationSettings.RouteClipboardBehavior; set { settings.ColonisationSettings.RouteClipboardBehavior = value; OnPropertyChanged(); OnPropertyChanged(nameof(ClipboardBehaviourText)); } }
        public string ClipboardBehaviourText
        {
            get
            {
                return RouteClipboardBehavior switch
                {
                    ColonisationRouteClipboardBehavior.DoNothing => "Do not copy to clipboard",
                    ColonisationRouteClipboardBehavior.CopyIfNotBuilt => "Copy next system to clipboard when arriving in the system before it if the next system is not marked as built",
                    ColonisationRouteClipboardBehavior.CopyNext => "Copy next system to clipboard when arriving in the system before it",
                    ColonisationRouteClipboardBehavior.CopyPrevious => "Copy previous system to clipboard when arriving in the system before it",
                    _ => string.Empty,
                };
            }
        }

        public string TargetSystemText => targetSystem == null ?
            string.Empty : $"{targetSystem.Distance:N2} ly away from {targetSystem.SystemName}";
        internal EventHandler? DataChangedWarning;
        internal EventHandler? OnViewModelLive;
        internal EventHandler<ColonisationRouteSystemVM>? SelectedSystemChanged;
        private async Task OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                var cmdrs = journalManager.Commanders.Where(x => !x.IsHidden && !x.IsLegacy && x.Id != journalManager.SelectedCommander?.Id)
                                             .Select(x => new JournalCommanderVM(x));

                LiveCommanders.ClearCollection();
                LiveCommanders.AddRange(cmdrs);
                OnPropertyChanged(nameof(HaveCommders));
                if (store.RouteSystems.Any() == false)
                {
                    IsBusy = false;
                    return;
                }
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await LoadFromDatabase();                    

                    CalcLineValues();
                    IsBusy = false;

                    OnViewModelLive?.Invoke(this, EventArgs.Empty);
                });        
                return;
            }
        }

        private async Task LoadFromDatabase()
        {
            RouteSystems.ClearCollection();
            var routeSystems = store.RouteSystems.Where(x => x.HopNumber >= 0).Select(x => new ColonisationRouteSystemVM(x, this)).ToList();
            RouteSystems.AddRange(routeSystems);

            Application.Current.Dispatcher.Invoke(() =>
            {
                TargetSystem = store.RouteSystems.Where(x => x.HopNumber == -1).Select(x => new ColonisationRouteSystemVM(x, this)).FirstOrDefault();
                _ = CheckAllSystemCounts(false);
                CalcDistances();
                if (settings.ColonisationSettings.SelectedPlannerAddress > 0)
                {
                    SelectedSystem = RouteSystems.FirstOrDefault(x => x.SystemAddress == settings.ColonisationSettings.SelectedPlannerAddress);
                }
                SelectedSystem ??= RouteSystems.LastOrDefault(x => x.Built);
                DataChanged = false;
            });
        }

        internal bool ShouldCheckSpansh =>  settings.ColonisationSettings.CheckSpansh;

        internal async Task<bool> CheckAllSystemCounts(bool checkSpansh)
        {
            bool updated = false;
            foreach (var routesystem in routeSystems)
            {
                if (routesystem.BodyCount > 0 && routesystem.StarCount > 0 && routesystem.Built)
                    continue;

                if (await UpdateSystemBodyCounts(routesystem, checkSpansh))
                    updated = true;
            }

            return updated;
        }
        internal async Task<bool> UpdateSystemBodyCounts(ColonisationRouteSystemVM routesystem, bool checkSpansh)
        {
            var shouldCheck = !(routesystem.BodyCount > 0 && routesystem.StarCount > 0 && routesystem.Built);
            if (!shouldCheck)
                return false;

            if (sharedData.Systems.TryGetValue(routesystem.SystemAddress, out var system) && shouldCheck)
            {
                routesystem.BodyCount = system.BodyCount;
                routesystem.StarCount = system.Bodies.Count(x => x.Value.StarType != EliteJournalReader.StarType.Unknown);
                if (!routesystem.Built)
                    routesystem.Built = system.FactionCount > 1;
                DataChanged = true;
                shouldCheck = !(routesystem.BodyCount > 0 && routesystem.StarCount > 0 && routesystem.Built);
            }

            if (!checkSpansh || !shouldCheck)
                return DataChanged;

            var spanshDump = await spanshClient.GetSystem(routesystem.SystemAddress);

            if (spanshDump == null || spanshDump.System == null)
                return false;

            routesystem.StarCount = Math.Max(spanshDump.System.Bodies.Count(x => x.Type.Contains("star", StringComparison.OrdinalIgnoreCase)), routesystem.StarCount);
            routesystem.BodyCount = Math.Max(spanshDump.System.BodyCount, routesystem.BodyCount);
            routesystem.Built = spanshDump.System.Factions?.Count > 1;
            DataChanged = true;
            return true;
        }

        private bool isDisposing;
        public void Dispose()
        {
            if (isDisposing)
            { 
                return;
            }

            isDisposing = true;
            sharedData.CurrentSystemChanged -= OnCurrentSystemChanged;
            sharedData.SystemDSSScaned -= OnSystemDSSScanned;
            settings.ColonisationSettings.StraightLineStarPosChanged -= ColonisationSettings_StraightLineStarPosChanged;
            settings.ColonisationSettings.StraightLineEndPosChanged -= ColonisationSettings_StraightLineEndPosChanged;

            if (DataChanged && !IsBusy)
            {
                DataChangedWarning?.Invoke(this, EventArgs.Empty);
            }
        }

        internal bool GetEliteNavRoute(bool addToRoute)
        {
            var route = sharedData.GetNavRoute();

            return ParseEliteNavRouteActual(route, addToRoute);
        }

        internal bool ParseEliteNavRoute(string fileName, bool addToRoute)
        {
            string json = File.ReadAllText(fileName, Encoding.UTF8);

            var route = JsonConvert.DeserializeObject<NavigationRoute>(json);

            return ParseEliteNavRouteActual(route, addToRoute);
        }
        private bool ParseEliteNavRouteActual(NavigationRoute? route, bool addToRoute)
        {
            if (route == null || route.Route.Any() == false)
                return false;

            if (addToRoute)
            {
                foreach (var system in route.Route)
                {
                    var known = RouteSystems.FirstOrDefault(x => x.SystemAddress == system.SystemAddress);

                    if (known != null)
                    {
                        known.UpdateSystemDetails(system);
                        continue;
                    }

                    RouteSystems.AddItem(new(system, this));
                }

                CalcDistances();
                return true;
            }

            TargetSystem = null;
            RouteSystems.ClearCollection();
            RouteSystems.AddRange(route.Route.Select(x => new ColonisationRouteSystemVM(x, this)));
            CalcDistances();
            return true;
        }

        internal bool ParseSpanshRoute(string fileName, bool addToRoute)
        {
            try
            {
                string json = File.ReadAllText(fileName, Encoding.UTF8);

                var route = JsonConvert.DeserializeObject<ApiColonisationRoute>(json);

                if (route == null || route.Result == null || route.Result.Jumps == null || route.Result.Jumps.Any() == false)
                    return false;

                if (addToRoute)
                {
                    foreach (var system in route.Result.Jumps)
                    {
                        var known = RouteSystems.FirstOrDefault(x => x.SystemAddress == system.Id64);

                        if (known != null)
                        {
                            known.UpdateSystemDetails(system);
                            continue;
                        }

                        RouteSystems.AddItem(new(system, this));
                    }

                    CalcDistances();
                    return true;
                }

                TargetSystem = null;
                RouteSystems.ClearCollection();
                RouteSystems.AddRange(route.Result.Jumps.Select(x => new ColonisationRouteSystemVM(x, this)));

                CalcDistances();
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal void CalcDistances()
        {
            ColonisationRouteSystemVM? previous = null;
            double totalDistance = 0;
            double totalHopDistance = 0;
            ColonisationRouteSystemVM? lastBuilt = null;
            ColonisationRouteSystemVM? hub = RouteSystems.FirstOrDefault();
            ColonisationRouteSystemVM? start = RouteSystems.FirstOrDefault();

            int hopNumber = 1;
            foreach (var system in RouteSystems)
            {
                if (previous == null)
                {
                    previous = system;
                    totalDistance = 0;
                    system.HopNumber = 0;
                    continue;
                }

                system.Distance = previous.Position.DistanceFrom(system.Position);
                totalHopDistance += system.Distance;
                if (hub != null)
                {
                    totalDistance = hub.Position.DistanceFrom(system.Position);
                }
                system.TotalHopDistance = totalHopDistance;
                if (start != null)
                {
                    system.TotalDistanceFromStart = start.Position.DistanceFrom(system.Position);
                }
                system.TotalDistance = totalDistance;
                system.HopNumber = hopNumber;
                hopNumber++;
                previous = system;

                if (system.Hub)
                {
                    hub = system;
                    totalDistance = 0;
                    totalHopDistance = 0;
                    hopNumber = 1;
                }

                if (system.Built)
                {
                    lastBuilt = system;
                }
            }

            DataChanged = true;
            if (RouteSystems.Count == 0)
            {
                return;
            }

            lastBuilt ??= RouteSystems.FirstOrDefault();

            if (lastBuilt == null)
                return;

            SelectedSystem ??= RouteSystems.FirstOrDefault();
            TargetSystem ??= RouteSystems.LastOrDefault()?.Clone();

            if (TargetSystem != null)
            {
                TargetSystem.HopNumber = -1;
                TargetSystem.Distance = TargetSystem.Position.DistanceFrom(lastBuilt.Position);
                OnPropertyChanged(nameof(TargetSystemText));
            }
        }

        private void RemoveAfterSelected(object? obj)
        {
            if (SelectedSystem == null)
                return;

            var index = RouteSystems.IndexOf(SelectedSystem);

            if (index < 0)
                return;

            for (var i = RouteSystems.Count - 1; i > index; i--)
            {
                RouteSystems.RemoveAt(i);
            }

            DataChanged = true;
        }

        private void SetSelectedAsTarget(object? obj)
        {
            if (selectedSystem == null)
                return;

            TargetSystem = new(selectedSystem)
            {
                HopNumber = -1
            };

            CalcDistances();
            selectedSystem.Selected(RouteSystems.FirstOrDefault(), TargetSystem);
        }

        private void ToggleSelectedBuilt(object? obj)
        {
            if (SelectedSystem == null)
                return;

            SelectedSystem.Built = !SelectedSystem.Built;

            DataChanged = true;

            CalcDistances();
        }

        private void ToggleSelectedHub(object? obj)
        {
            if (SelectedSystem == null)
                return;

            SelectedSystem.Hub = !SelectedSystem.Hub;

            DataChanged = true;

            CalcDistances();
        }

        internal void DeleteSelectedSystem(object? obj)
        {
            if (SelectedSystem == null)
                return;

            RouteSystems.RemoveItem(SelectedSystem);
            SelectedSystem = null;

            CalcDistances();
        }

        private void SetClipboard(string value)
        {
            notification.SetClipboard(value);
        }

        internal void SystemDataChanged()
        {
            DataChanged = true;
        }

        private void SaveRoute(object? obj)
        {
            store.SaveColonisationRotue(RouteSystems, TargetSystem);
            DataChanged = false;
        }

        private void SelectCurrentSystem(object? obj)
        {
            var inRoute = RouteSystems.FirstOrDefault(x => x.SystemAddress == sharedData.CurrentSystem?.Address);

            if (inRoute == null)
                return;

            SelectedSystem = inRoute;
        }

        internal void ClearRoute()
        {
            RouteSystems.ClearCollection();
            SelectedSystem = null;
            TargetSystem = null;
            DataChanged = true;
        }

        private bool CanAddCurrentSystem(AddSystemToRouteEnum x)
        {
            if (sharedData.CurrentSystem == null)
                return false;

            if (RouteSystems.Any(x => x.SystemAddress == sharedData.CurrentSystem.Address))
                return false;

            switch (x)
            {
                case AddSystemToRouteEnum.AtStart:
                    break;
                case AddSystemToRouteEnum.BeforeSelected:
                    if (selectedSystem == null)
                        return false;
                    break;
                case AddSystemToRouteEnum.AfterSelected:
                    if (selectedSystem == null)
                        return false;
                    break;
                case AddSystemToRouteEnum.AtEnd:
                    break;
            }

            return true;
        }

        private void AddCurrentToRoute(AddSystemToRouteEnum route)
        {
            if (sharedData.CurrentSystem == null)
                return;

            if (RouteSystems.Any(x => x.SystemAddress == sharedData.CurrentSystem.Address))
                return;

            var newSystem = new ColonisationRouteSystemVM(
                new SystemInRoute()
                {
                    StarClass = EliteJournalReader.StarType.Unknown,
                    SystemAddress = sharedData.CurrentSystem.Address,
                    StarSystem = sharedData.CurrentSystem.Name,
                    StarPos = new() { X = sharedData.CurrentSystem.Position.X, Y = sharedData.CurrentSystem.Position.Y, Z = sharedData.CurrentSystem.Position.Z }
                }, this);

            switch (route)
            {
                case AddSystemToRouteEnum.AtStart:
                    RouteSystems.Insert(0, newSystem);
                    break;
                case AddSystemToRouteEnum.BeforeSelected:
                    if (SelectedSystem == null)
                    {
                        return;
                    }
                    var index = RouteSystems.IndexOf(SelectedSystem);
                    RouteSystems.Insert(index, newSystem);
                    break;
                case AddSystemToRouteEnum.AfterSelected:
                    if (SelectedSystem == null)
                    {
                        return;
                    }
                    var idx = RouteSystems.IndexOf(SelectedSystem);
                    RouteSystems.Insert(idx + 1, newSystem);
                    break;
                case AddSystemToRouteEnum.AtEnd:
                    RouteSystems.Add(newSystem);
                    break;
            }

            CalcDistances();
            SelectedSystem = newSystem;
            SelectedSystemChanged?.Invoke(this, newSystem);
        }

        internal bool LoadFromJson(string filename, bool addToRoute)
        {
            try
            {
                var json = File.ReadAllText(filename);
                var data = JsonConvert.DeserializeObject<ColonisationRouteSave>(json);

                if (data is null || data.RouteSystems.Count == 0)
                    return false;

                if (addToRoute)
                {
                    var systemsToAdd = data.RouteSystems.Where(x => RouteSystems.Any(y => y.SystemAddress == x.SystemAddress) == false).Select(x => new ColonisationRouteSystemVM(x, this));

                    RouteSystems.AddRange(systemsToAdd);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        TargetSystem = data.TargetSystem == null ? null : new(data.TargetSystem, this);
                        CalcDistances();
                    });
                    return true;
                }

                RouteSystems.ClearCollection();
                RouteSystems.AddRange(data.RouteSystems.Where(x => x.HopNumber >= 0).Select(x => new ColonisationRouteSystemVM(x, this)));

                Application.Current.Dispatcher.Invoke(() =>
                {
                    TargetSystem = data.TargetSystem == null ? null : new(data.TargetSystem, this);
                    CalcDistances();
                });
            }
            catch
            {
                return false;
            }

            return true;
        }

        internal bool SaveJson(string fileName)
        {
            var save = new ColonisationRouteSave()
            {
                RouteSystems = [.. RouteSystems.Select(x => new ColonisationRouteSystem(x))]
            };

            if (targetSystem != null)
            {
                var targetKnown = RouteSystems.FirstOrDefault(x => x.SystemAddress == targetSystem.SystemAddress);

                if (targetKnown == null)
                    save.TargetSystem = new(targetSystem);
            }

            try
            {
                var json = JsonConvert.SerializeObject(save, Formatting.Indented);
                File.WriteAllText(fileName, json);
            }
            catch
            {
                return false;
            }

            return true;
        }

        internal bool SaveCSV(string fileName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("\"Hop Number\", \"Address\", \"SystemName\", \"X\", \"Y\", \"Z\", \"Bount Count\", \"Star Count\"\"Built\", \"Hub\", \"Hop Distance\", \"Total Hop Distance\", \"Dist From Hub\", \"Dist From Start\", \"Notes\"");

            foreach (var item in RouteSystems)
            {
                sb.AppendLine($"\"{item.DataBaseHopNumber}\", \"{item.SystemAddress}\", \"{item.SystemName}\", \"{item.Position.X}\", \"{item.Position.Y}\", \"{item.Position.Z}\", \"{item.BodyCount}\", \"{item.StarCount}\", \"{item.Built}\", \"{item.Hub}\", \"{item.Distance}\", \"{item.TotalHopDistance}\", \"{item.TotalDistance}\", \"{item.TotalDistanceFromStart}\", \"{item.Notes}\"");
            }

            try
            {
                File.WriteAllText(fileName, sb.ToString());
            }
            catch
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Straight Line Tool
        public Position? StartPos { get => settings.ColonisationSettings.LineStart; set { settings.ColonisationSettings.LineStart = value; OnPropertyChanged(); } }
        public Position? EndPos { get => settings.ColonisationSettings.LineEnd; set { settings.ColonisationSettings.LineEnd = value; OnPropertyChanged(); } }
        private Position point500ly = new();

        private string distanceFromStart = string.Empty;
        public string DistanceFromStart { get => distanceFromStart; set { distanceFromStart = value; OnPropertyChanged(); } }

        private string distanceFrom500ly = string.Empty;
        public string DistanceFrom500ly { get => distanceFrom500ly; set { distanceFrom500ly = value; OnPropertyChanged(); } }

        private string distanceFromStraightLine = string.Empty;
        public string DistanceFromStraightLine { get => distanceFromStraightLine; set { distanceFromStraightLine = value; OnPropertyChanged(); } }

        private string directionsToStraightLine = string.Empty;
        public string DirectionsToStraightLine { get => directionsToStraightLine; set { directionsToStraightLine = value; OnPropertyChanged(); } }

        public ICommand SetStartAsSelectedCommand { get; }
        public ICommand SetStartAsCurrentCommand { get; }
        public ICommand SetEndAsSelectedCommand { get; }
        public ICommand SetEndAsCurrentCommand { get; }
        public ICommand SetEndAsTargetCommand { get; }

        private void SetStartAsSelected(object? obj)
        {
            if (selectedSystem == null)
                return;

            settings.ColonisationSettings.LineStart = selectedSystem.Position.Position;
        }

        private void SetStartAsCurrent(object? obj)
        {
            if (sharedData.CurrentSystem == null)
                return;

            settings.ColonisationSettings.LineStart = sharedData.CurrentSystem.Position;
        }

        private void SetEndAsSelected(object? obj)
        {
            if (selectedSystem == null)
                return;

            settings.ColonisationSettings.LineEnd = selectedSystem.Position.Position;
        }

        private void SetEndAsCurrent(object? obj)
        {
            if (sharedData.CurrentSystem == null)
                return;

            settings.ColonisationSettings.LineEnd = sharedData.CurrentSystem.Position;
        }

        private void SetEndAsTargetSystem(object? obj)
        {
            if (TargetSystem == null)
                return;

            settings.ColonisationSettings.LineEnd = TargetSystem.Position.Position;
        }

        private void CalcLineValues()
        {
            if (StartPos == null || EndPos == null || sharedData.CurrentSystem == null)
            {
                DistanceFromStart = string.Empty;
                DistanceFrom500ly = string.Empty;
                DistanceFromStraightLine = string.Empty;
                DirectionsToStraightLine = string.Empty;
                return;
            }

            point500ly = Position.GetPointAtDistance(StartPos, EndPos, 500);
            var nearestPointOnline = Position.GetClosestPointOnLineSegment(StartPos, EndPos, sharedData.CurrentSystem.Position);

            DistanceFromStart = $"{Position.Distance(sharedData.CurrentSystem.Position, StartPos):N2} ly";
            DistanceFrom500ly = $"{Position.Distance(sharedData.CurrentSystem.Position, point500ly):N2} ly";
            DistanceFromStraightLine = $"{Position.Distance(sharedData.CurrentSystem.Position, nearestPointOnline):N2} ly";
            DirectionsToStraightLine = $"{DirectionText(nearestPointOnline.X, sharedData.CurrentSystem.Position.X, "Left", "Right")} | {DirectionText(nearestPointOnline.Y, sharedData.CurrentSystem.Position.Y, "Down", "Up")} | {DirectionText(nearestPointOnline.Z, sharedData.CurrentSystem.Position.Z, "Back", "Forward")}";
        }

        private static string DirectionText(double current, double point, string lessText, string moreText)
        {
            var dist = Math.Abs(point - current);
            if (current > point)
                return $"{moreText} {dist:N2} ly";

            if (current < point)
                return $"{lessText} {dist:N2} ly";

            return "---";
        }        
        #endregion
    }
}
