using Newtonsoft.Json;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Shared;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{

    public sealed class SpanshAPIController : ODObservableObject, IDisposable
    {
        #region Store references
        private readonly SpanshRouteStore routeStore;
        private readonly SharedDataStore sharedDataStore;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly SettingsStore settingsStore;
        public SharedDataStore Shared => sharedDataStore;
        #endregion

        #region Api Properties
        private readonly Dictionary<SpanshRouteTypeComboBox, ApiArgsVM> apiArgsVM = [];
        public ApiArgsVM CurrentApiArgs
        {
            get
            {
                if (apiArgsVM.TryGetValue(CurrentTypeBox, out var routeSystems))
                {
                    if (routeSystems.IntProperty1 <= 0)
                        UpdateCarrierDetails(routeSystems);
                    return routeSystems;
                }
                ApiArgsVM newArgs = new(CurrentTypeBox, CurrentSystem);
                apiArgsVM.Add(CurrentTypeBox, newArgs);
                UpdateCarrierDetails(newArgs);
                return newArgs;
            }
        }
        public bool CanGenerate => CurrentApiArgs.CanGenerate();
        private bool isGnerating;
        public bool IsGenerating { get => isGnerating; set { isGnerating = value; OnPropertyChanged(nameof(IsGenerating)); } }
        public SpanshRouteTypeComboBox CurrentTypeBox
        {
            get => settingsStore.SpanshCSVSettings.GetApiSelection(settingsStore.SelectedCommanderID);
            set
            {
                settingsStore.SpanshCSVSettings.SetApiSelection(settingsStore.SelectedCommanderID, value);
                OnPropertyChanged(nameof(CurrentTypeBox));
                OnPropertyChanged(nameof(CurrentApiArgs));
                OnPropertyChanged(nameof(CanGenerate));
                OnPropertyChanged(nameof(ShowWarning));
                OnPropertyChanged(nameof(WarnAboutScoop));
                OnPropertyChanged(nameof(WarnAboutAFMU));
                OnPropertyChanged(nameof(WarnAboutDSS));
            }
        }        

        private string busyText = "Generating Route";
        public string BusyText
        {
            get => busyText;
            set
            {
                busyText = value;
                OnPropertyChanged(nameof(BusyText));
            }
        }
        #endregion

        #region Ship Properties
        private readonly ObservableCollection<ShipInfoVM> ships = [];
        public IEnumerable<ShipInfoVM> Ships => ships.OrderBy(x => x.HasScoop).ThenBy(x => x.Name);
        private ShipInfoVM? selectedShip;
        public ShipInfoVM? SelectedShip
        {
            get => selectedShip;
            set
            {              
                routeStore.SelectedShipName = string.Empty;
                selectedShip = value;
                if (selectedShip != null)
                {
                    SelectedShipId = selectedShip.ShipId;
                    routeStore.SelectedShipName = selectedShip.Name;
                }
                OnPropertyChanged(nameof(SelectedShip));
                OnPropertyChanged(nameof(ShowWarning));
                OnPropertyChanged(nameof(WarnAboutScoop));
                OnPropertyChanged(nameof(WarnAboutAFMU));
                OnPropertyChanged(nameof(WarnAboutDSS));
            }
        }
        private int SelectedShipId
        {
            get => settingsStore.SpanshCSVSettings.GetSelectedShipID(settingsStore.SelectedCommanderID);
            set => settingsStore.SpanshCSVSettings.SetSelectedShipID(settingsStore.SelectedCommanderID, value);
        }
        public bool ShowWarning => WarnAboutScoop || WarnAboutAFMU || WarnAboutDSS;
        public bool WarnAboutScoop
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasScoopBool)
                    return false;

                return CurrentApiArgs.RouteType switch
                {
                    SpanshRouteTypeComboBox.RoadToRiches or
                    SpanshRouteTypeComboBox.NeutronRoute or
                    SpanshRouteTypeComboBox.WorldTypeRoute or
                    SpanshRouteTypeComboBox.TouristRoute or
                    SpanshRouteTypeComboBox.Exobiology => true,
                    SpanshRouteTypeComboBox.GalaxyPlotter => CurrentApiArgs.BoolProperty1 || CurrentApiArgs.BoolProperty2 || CurrentApiArgs.BoolProperty6,
                    _ => false,
                };
            }
        }
        public bool WarnAboutAFMU
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasAFMUBool)
                    return false;

                return CurrentApiArgs.RouteType switch
                {
                    SpanshRouteTypeComboBox.NeutronRoute => true,
                    SpanshRouteTypeComboBox.GalaxyPlotter => CurrentApiArgs.BoolProperty1 || CurrentApiArgs.BoolProperty2,
                    _ => false,
                };
            }
        }
        public bool WarnAboutDSS
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasDSSBool)
                    return false;

                return CurrentApiArgs.RouteType switch
                {
                    SpanshRouteTypeComboBox.RoadToRiches or
                    SpanshRouteTypeComboBox.WorldTypeRoute or
                    SpanshRouteTypeComboBox.Exobiology => true,
                    _ => false,
                };
            }
        }
        #endregion

        #region Game System Properties
        public string? CurrentSystem => sharedDataStore.CurrentSystem?.Name;
        public ObservableCollection<SystemInRouteVM> GameRouteSystems { get; } = [];
        public SystemInRouteVM? LastGameRouteSystem => GameRouteSystems.LastOrDefault();
        private SystemInRouteVM? selectedGameRouteSystem;
        public SystemInRouteVM? SelectedGameRouteSystem
        {
            get => selectedGameRouteSystem;
            set
            {
                selectedGameRouteSystem = value;
                OnPropertyChanged(nameof(SelectedGameRouteSystem));
            }
        }
        #endregion

        #region Commands
        public ICommand GenerateRouteCommand { get; }
        public ICommand SetCurrentSystemCommand { get; }
        public ICommand SetLastGameSystemAsTargetCommand { get; }
        public ICommand SetSelectedGameSystemAsSourceCommand { get; }
        public ICommand SetSelectedGameSystemAsTargetCommand { get; }
        public ICommand AddAdditionalSystemCommand { get; }
        public ICommand DeleteAdditionalSystemCommand { get; }
        public ICommand ReverseRouteCommand { get; }
        public ICommand ImportDestinationsCommand { get; }
        public ICommand ChangeCarrierTypeCommand { get; }
        public ICommand ImportShipStatsCommand { get; }
        public ICommand ClearAdditionDestinationsCommand { get; }
        public ICommand SelectCurrentShipCommand { get; }
        public ICommand OpenShipyardCommand { get; }
        #endregion

        #region ctor
        public SpanshAPIController(SpanshRouteStore routeStore,
                                   SharedDataStore sharedDataStore,
                                   FleetCarrierDataStore fcDataStore,
                                   SettingsStore settingsStore)
        {
            this.routeStore = routeStore;
            this.sharedDataStore = sharedDataStore;
            this.fcDataStore = fcDataStore;
            this.settingsStore = settingsStore;

            LoadApiSave(settingsStore.SpanshCSVSettings.GetApiSave(settingsStore.SelectedCommanderID));

            ReverseRouteCommand = new ODRelayCommand(ReverseRoute, (_) => CurrentApiArgs.SourceSystem.HasValue && CurrentApiArgs.TargetSystem.HasValue);
            GenerateRouteCommand = new ODRelayCommand(GenerateRoute, (_) => CanGenerate);
            SetCurrentSystemCommand = new ODRelayCommand(SetCurrentSystem, (_) =>
            {
                if (CurrentApiArgs.RouteType == SpanshRouteTypeComboBox.FleetCarrier)
                {
                    var carrierLocation = CurrentApiArgs.BoolProperty1 ? fcDataStore.SquadCarrierData?.StarSystem : fcDataStore.PersonalCarrierData?.StarSystem;
                    return !string.Equals(carrierLocation, CurrentApiArgs.SourceSystem.SystemName);
                }
                return !string.Equals(CurrentSystem, CurrentApiArgs.SourceSystem.SystemName);
            });
            SetSelectedGameSystemAsSourceCommand = new ODRelayCommand(SetSelectedGameSystemAsSource, (_) => SelectedGameRouteSystem != null);
            SetSelectedGameSystemAsTargetCommand = new ODRelayCommand(SetSelectedGameSystemAsTarget, (_) => SelectedGameRouteSystem != null);
            SetLastGameSystemAsTargetCommand = new ODRelayCommand(SetLastGameSystemAsTarget, (_) => LastGameRouteSystem != null);
            AddAdditionalSystemCommand = new ODRelayCommand(AddAdditionalSystem, (_) => CurrentApiArgs.CanAddAdditional);
            DeleteAdditionalSystemCommand = new ODRelayCommand<ApiRouteSystemVM>(DeleteAdditionalSystem, (vm) => vm != null);
            ImportDestinationsCommand = new ODRelayCommand<IEnumerable<string>>(ImportDestinations, (_) => CurrentApiArgs.CanImportDestinations);
            ClearAdditionDestinationsCommand = new ODRelayCommand(ClearAdditionalSystems, (_) => CurrentApiArgs.CanImportDestinations);
            ChangeCarrierTypeCommand = new ODRelayCommand(ChangeCarrierType);
            ImportShipStatsCommand = new ODRelayCommand(ImportShipStats, (_) => SelectedShip != null && CurrentApiArgs.CanAddShipStats);
            SelectCurrentShipCommand = new ODRelayCommand(SelectCurrentShip, (_) => sharedDataStore.CurrentShipInfo?.ShipId != SelectedShip?.ShipId);
            OpenShipyardCommand = new ODRelayCommand<ShipyardWebSite>(OpenShipyard, (_) => SelectedShip != null);

            this.routeStore.StoreLive += SpanshStoreLive;

            this.sharedDataStore.CurrentRouteChanged += OnCurrentRouteChanged;
            this.sharedDataStore.CurrentSystemChanged += CurrentSystemChanged;
            this.sharedDataStore.ShipChangedEvent += ShipChanged;
            this.sharedDataStore.ShipCargoUpdatedEvent += ShipCargoUpdated;
            this.sharedDataStore.ShipStatsUpdated += ShipStatsUpdated;

            if (routeStore.IsLive)
            {
                SpanshStoreLive(null, true);
            }
        }        
        #endregion

        #region Game Route Systems Methods
        private void SetSelectedGameSystemAsSource(object? obj)
        {
            if (SelectedGameRouteSystem == null)
                return;

            CurrentApiArgs.SourceSystem = new(SelectedGameRouteSystem);
        }

        private void SetSelectedGameSystemAsTarget(object? obj)
        {
            if (SelectedGameRouteSystem == null)
                return;

            CurrentApiArgs.TargetSystem = new(SelectedGameRouteSystem);
        }

        private void SetLastGameSystemAsTarget(object? obj)
        {
            if (LastGameRouteSystem == null)
                return;

            CurrentApiArgs.TargetSystem = new(LastGameRouteSystem);
        }

        private void OnCurrentRouteChanged(object? sender, List<SystemInRoute> e)
        {
            GameRouteSystems.ClearCollection();

            SystemInRouteVM? previous = null;
            List<SystemInRouteVM> systemsToAdd = [];
            foreach (var system in e)
            {
                var newSystem = new SystemInRouteVM(system, previous);
                systemsToAdd.Add(newSystem);
                previous = newSystem;
            }

            GameRouteSystems.AddRange(systemsToAdd);
            SelectedGameRouteSystem = GameRouteSystems.FirstOrDefault();
            OnPropertyChanged(nameof(LastGameRouteSystem));
        }
        #endregion

        #region Current Ship Methods
        private void SelectCurrentShip(object? obj)
        {
            var ship = ships.FirstOrDefault(x => x.ShipId == sharedDataStore.CurrentShipInfo?.ShipId);

            if (ship == null)
                return;

            SelectedShip = ship;
        }
        private void OpenShipyard(ShipyardWebSite site)
        {
            if (SelectedShip == null)
                return;

            SelectedShip.ShipInfo.OpenShipyard(site);
        }
        private void ShipStatsUpdated(object? sender, ShipInfo? e)
        {
            if (e == null)
                return;

            var ship = ships.FirstOrDefault(x => x.ShipInfo == e);

            if (ship == null)
                return;

            ship.UpdateStats();
        }

        private void ShipCargoUpdated(object? sender, ShipInfo? e)
        {
            if (e == null)
                return;

            var ship = ships.FirstOrDefault(x => x.ShipInfo == e);

            if (ship == null)
                return;

            ship.OnCargoUpdated(e.Cargo);
        }

        private void ImportShipStats(object? obj)
        {
            if (SelectedShip == null)
                return;

            CurrentApiArgs.ImportShipStats(SelectedShip);
        }

        private void ShipChanged(object? sender, ShipInfo? e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var ships = sharedDataStore.Ships.Select(x => new ShipInfoVM(x));

                this.ships.ClearCollection();
                this.ships.AddRange(ships);

                SelectedShip = this.ships.FirstOrDefault(x => x.ShipId == SelectedShipId);
                SelectedShip ??= this.ships.FirstOrDefault(x => x.ShipId == sharedDataStore.CurrentShipInfo?.ShipId);
                SelectedShip ??= this.ships.FirstOrDefault();

                OnPropertyChanged(nameof(Ships));
            });
        }
        #endregion

        #region Api Systems Methods
        private void ClearAdditionalSystems(object? obj)
        {
            CurrentApiArgs.AdditionalSystems.ClearCollection();
        }

        private void DeleteAdditionalSystem(ApiRouteSystemVM vM)
        {
            CurrentApiArgs.AdditionalSystems.Remove(vM);
        }

        private void ImportDestinations(IEnumerable<string> systems)
        {
            foreach (string s in systems)
            {
                if (CurrentApiArgs.AdditionalSystems.FirstOrDefault(x => string.Equals(x.SystemName, s, StringComparison.OrdinalIgnoreCase)) == null)
                    CurrentApiArgs.AdditionalSystems.Add(new() { SystemName = s.Trim() });
            }
        }

        private void ReverseRoute(object? obj)
        {
            if (!CurrentApiArgs.SourceSystem.HasValue || !CurrentApiArgs.TargetSystem.HasValue)
                return;

            var source = CurrentApiArgs.SourceSystem;
            var target = CurrentApiArgs.TargetSystem;
            CurrentApiArgs.SourceSystem = target;
            CurrentApiArgs.TargetSystem = source;
        }

        private void AddAdditionalSystem(object? obj)
        {
            CurrentApiArgs.AddAdditonalSystem();
        }

        private void ApiArgVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShowWarning));
            OnPropertyChanged(nameof(WarnAboutScoop));
            OnPropertyChanged(nameof(WarnAboutDSS));
            OnPropertyChanged(nameof(WarnAboutAFMU));
        }
        #endregion

        #region Carrier Methods
        private void ChangeCarrierType(object? obj)
        {
            if (CurrentApiArgs.RouteType != SpanshRouteTypeComboBox.FleetCarrier)
                return;

            CurrentApiArgs.BoolProperty1 = !CurrentApiArgs.BoolProperty1;

            UpdateCarrierDetails(CurrentApiArgs);
        }

        private void UpdateCarrierDetails(ApiArgsVM routeSystem)
        {
            if (routeSystem.RouteType != SpanshRouteTypeComboBox.FleetCarrier)
                return;
            var fcData = routeSystem.BoolProperty1 ? fcDataStore.SquadCarrierData : fcDataStore.PersonalCarrierData;

            if (fcData == null)
            {
                routeSystem.IntProperty1 = 0;
                return;
            }
            routeSystem.SourceSystem.SystemName = fcData.StarSystem;
            routeSystem.IntProperty1 = fcData.UsedCapacity;
            routeSystem.IntProperty2 = fcData.FuelLevel;
            return;
        }
        #endregion

        #region Store Listeners
        private Task SpanshStoreLive(object? sender, bool e)
        {
            if (e)
            {
                if (string.IsNullOrEmpty(CurrentApiArgs.SourceSystem.SystemName) && CurrentSystem != null)
                {
                    CurrentApiArgs.SourceSystem.SystemName = CurrentSystem;
                }
                CurrentApiArgs.PropertyChanged += ApiArgVMPropertyChanged;
                ShipChanged(null, null);
            }
            return Task.CompletedTask;
        }

        private void CurrentSystemChanged(object? sender, StarSystem? e)
        {
            var systemOnRoute = GameRouteSystems?.FirstOrDefault(x => e?.Address == x.Address);

            if (systemOnRoute != null)
            {
                SelectedGameRouteSystem = systemOnRoute;
            }
            
            OnPropertyChanged(nameof(CurrentSystem));
        }
        #endregion

        #region Api UI Methods
        private void SetCurrentSystem(object? obj)
        {
            if (CurrentApiArgs.RouteType == SpanshRouteTypeComboBox.FleetCarrier)
            {
                var carrierLocation = CurrentApiArgs.BoolProperty1 ? fcDataStore.SquadCarrierData?.StarSystem : fcDataStore.PersonalCarrierData?.StarSystem;
                CurrentApiArgs.SourceSystem.SystemName = carrierLocation ?? string.Empty;
                return;
            }
            if (CurrentSystem != null)
            {
                CurrentApiArgs.SourceSystem.SystemName = CurrentSystem;
            }
        }        

        private void ResetBusyText()
        {
            IsGenerating = false;
            UpdateBusyText("Generating Route");
        }

        private async Task ErrorBusyTextMessage(string text)
        {
            UpdateBusyText(text);
            await Task.Delay(10000);
        }

        private void UpdateBusyText(string text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                BusyText = text;
            });
        }
        #endregion

        #region Route Generation
        private void GenerateRoute(object? obj)
        {
            Task.Run(async () =>
            {
                try
                {
                    IsGenerating = true;

                    var args = CurrentApiArgs.GetApiArgs(SelectedShip?.ShipInfo);

                    if (args.Item1 == null)
                    {
                        UpdateBusyText($"Error while creating args for api call\nMessage : {args.Item2}");
                        await Task.Delay(4000);
                        ResetBusyText();
                        return;
                    }

                    UpdateBusyText("Checking System Names");
                    await Task.Delay(500);
                    var results = await routeStore.CheckSystemNamesWithSpansh(args.Item1);

                    if (results.Count > 0)
                    {
                        UpdateBusyText($"System Name check failed. Those in red were not found");
                        await Task.Delay(4000);
                        ResetBusyText();
                        return;
                    }
                    UpdateBusyText("Generating Route");

                    var route = await routeStore.GenerateRoute(args.Item1, UpdateBusyText);

                    if (route.Item1 == false)
                    {
                        if (route.Item2 == null)
                        {
                            //Just incase we had an id that is no longer valid
                            CurrentApiArgs.JobID = null;
                            await ErrorBusyTextMessage("Failed to get route");
                            ResetBusyText();
                            return;
                        }
                        if (string.IsNullOrEmpty(route.Item2.Job))
                        {
                            //Just incase we had an id that is no longer valid
                            CurrentApiArgs.JobID = null;
                            await ErrorBusyTextMessage("Failed to get route");
                            await Task.Delay(4000);
                            ResetBusyText();
                            return;
                        }
                        CurrentApiArgs.JobID = route.Item2.Job;
                        await ErrorBusyTextMessage($"Failed to get route but we have a job number.\nWait a couple of minutes and click\ngerenate route again to check again.\n\nStatus : {route.Item2.Status}\nState : {route.Item2.State}");
                        await Task.Delay(10000);
                    }
                    if (route.Item2 != null)
                        CurrentApiArgs.JobID = route.Item2.Job;
                    if (route.Item2 is ApiGalaxyPlotterRoute galaxy && !string.IsNullOrEmpty(galaxy.Result.Reason))
                    {
                        UpdateBusyText($"Response from Spansh\n{galaxy.Result.Reason}");
                        await Task.Delay(10000);
                    }
                    UpdateBusyText("Route Generated");
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    UpdateBusyText($"Error : {ex.Message}");
                    await Task.Delay(10000);
                }
                finally
                {
                    ResetBusyText();
                }
            });
        }

        internal (bool, string) ImportJsonRoute(SpanshRouteType routeType, string filename)
        {
            try
            {
                var json = File.ReadAllText(filename);

                SpanshResponseBase? responseBase = null;

                switch (routeType)
                {
                    case SpanshRouteType.None:
                        return (false, "Route type cannot be none.");
                    case SpanshRouteType.RoadToRiches:
                        responseBase = JsonConvert.DeserializeObject<ApiRoadToRichesRoute>(json);
                        break;
                    case SpanshRouteType.FleetCarrier:
                        responseBase = JsonConvert.DeserializeObject<ApiFleetCarrierRoute>(json);
                        break;
                    case SpanshRouteType.NeutronRoute:
                        responseBase = JsonConvert.DeserializeObject<ApiNeutronRoute>(json);
                        break;
                    case SpanshRouteType.GalaxyPlotter:
                    case SpanshRouteType.GalaxyPlotterOld:
                        responseBase = JsonConvert.DeserializeObject<ApiGalaxyPlotterRoute>(json);
                        break;
                    case SpanshRouteType.WorldTypeRoute:
                        responseBase = JsonConvert.DeserializeObject<ApiWorldTypeRoute>(json);
                        break;
                    case SpanshRouteType.TouristRoute:
                        responseBase = JsonConvert.DeserializeObject<ApiTouristRoute>(json);
                        break;
                    case SpanshRouteType.Exobiology:
                    case SpanshRouteType.ExobiologyOld:
                        responseBase = JsonConvert.DeserializeObject<ApiExoviologyRoute>(json);
                        break;
                    case SpanshRouteType.Colonisation:
                        responseBase = JsonConvert.DeserializeObject<ApiColonisationRoute>(json);
                        break;
                }

                if (responseBase != null && SpanshRouteStore.CheckSpanshResponse(responseBase))
                {
                    if (routeStore.GenerateRoute(responseBase))
                        return (true, "Route Processed");
                    return (false, "Route has zero target systems");
                }

                return (false, "Error processing json.  Base is null");
            }
            catch (JsonSerializationException)
            {
                return (false, $"Unable to deserialise json.{Environment.NewLine}{Path.GetFileName(filename)}{Environment.NewLine}does not appear to contain valid {routeType.GetEnumDescription()} data.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        #endregion

        #region Persistence & IDisposal
        internal void LoadApiSave(List<SpashAPISave> savedata)
        {
            apiArgsVM.Clear();
            foreach (var args in savedata)
            {
                var vm = new ApiArgsVM();
                vm.LoadFronSave(args);
                if (!apiArgsVM.TryAdd(args.RouteType, vm))
                {
                    continue;
                }
            }

            // We listen to the property changed for the galaxy plotter to update warnings
            ApiArgsVM? galaxyArgs = null;

            if (apiArgsVM.TryGetValue(SpanshRouteTypeComboBox.GalaxyPlotter, out var routeSystems))
            {
                galaxyArgs = routeSystems;
            }
            galaxyArgs ??= new(SpanshRouteTypeComboBox.GalaxyPlotter, CurrentSystem);
            galaxyArgs.PropertyChanged += ApiArgVMPropertyChanged;

            OnPropertyChanged(nameof(CurrentApiArgs));
        }

        internal void SaveAPIData()
        {
            var ret = new List<SpashAPISave>();

            foreach (var args in apiArgsVM)
            {
                var save = args.Value.GetSave();
                ret.Add(save);
            }

            settingsStore.SpanshCSVSettings.SaveAPiData(settingsStore.SelectedCommanderID, ret);
        }

        public void Dispose()
        {
            this.sharedDataStore.CurrentRouteChanged -= OnCurrentRouteChanged;
            this.sharedDataStore.CurrentSystemChanged -= CurrentSystemChanged;
            this.sharedDataStore.ShipChangedEvent -= ShipChanged;
            this.sharedDataStore.ShipCargoUpdatedEvent -= ShipCargoUpdated;
            this.sharedDataStore.ShipStatsUpdated -= ShipStatsUpdated;
            this.routeStore.StoreLive -= SpanshStoreLive;
            SaveAPIData();
        }
        #endregion
    }
}
