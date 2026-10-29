using Newtonsoft.Json;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Models.Ship;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Shared;
using ODEliteTracker.ViewModels.ModelViews.Spansh.Api;
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
    public record AdditionalSystemAddedEventArgs(SpanshRouteType RouteType, bool Added);
    public sealed class SpanshAPIController : ODObservableObject, IDisposable
    {
        #region Store references
        private readonly SpanshRouteStore routeStore;
        private readonly SharedDataStore sharedDataStore;
        private readonly FleetCarrierDataStore fcDataStore;
        private readonly SettingsStore settingsStore;
        public SharedDataStore Shared => sharedDataStore;
        #endregion

        #region Events
        public EventHandler<AdditionalSystemAddedEventArgs>? AdditionalSystemAdded;
        #endregion

        #region Api Properties
        private readonly Dictionary<SpanshRouteTypeComboBox, ApiBase> apiArgsVM = [];
        public ApiBase CurrentApiArgs
        {
            get
            {
                if (apiArgsVM.TryGetValue(CurrentTypeBox, out var routeSystems))
                {
                    return routeSystems;
                }

                ApiBase apiBase = GetApiBase(CurrentTypeBox);
                apiArgsVM.TryAdd(CurrentTypeBox, apiBase);
                return apiBase;
            }
        }

        private ApiBase GetApiBase(SpanshRouteTypeComboBox routeType)
        {
            ApiBase apiBase = routeType switch
            {
                SpanshRouteTypeComboBox.FleetCarrier => new FleetCarrierRouteApiVM(),
                SpanshRouteTypeComboBox.NeutronRoute => new NeutronRouteApiVM(),
                SpanshRouteTypeComboBox.GalaxyPlotter => new GalaxyRouteApiVM(),
                SpanshRouteTypeComboBox.WorldTypeRoute => new WorldTypeRouteApiVM(),
                SpanshRouteTypeComboBox.TouristRoute => new TouristRouteApiVM(),
                SpanshRouteTypeComboBox.Exobiology => new ExobiologyRouteApiVM(),
                SpanshRouteTypeComboBox.Colonisation => new ColonisationRouteApiVM(),
                SpanshRouteTypeComboBox.TradePlanner => new TradePlannerApiVM(),
                _ => new RoadToRichesRouteApiVM(),
            };
            apiBase.LoadDefaults();
            if (!string.IsNullOrEmpty(CurrentSystem))
                apiBase.SourceSystem = new ApiRouteSystemVM(CurrentSystem);
            return apiBase;
        }

        public bool CanGenerate => CurrentApiArgs.CanGenerate();
        private bool isGnerating;
        public bool IsGenerating { get => isGnerating; set { isGnerating = value; OnPropertyChanged(nameof(IsGenerating)); } }
        public SpanshRouteTypeComboBox CurrentTypeBox
        {
            get => settingsStore.SpanshRouteSettings.GetApiSelection(settingsStore.SelectedCommanderID);
            set
            {
                settingsStore.SpanshRouteSettings.SetApiSelection(settingsStore.SelectedCommanderID, value);
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
            get => settingsStore.SpanshRouteSettings.GetSelectedShipID(settingsStore.SelectedCommanderID);
            set => settingsStore.SpanshRouteSettings.SetSelectedShipID(settingsStore.SelectedCommanderID, value);
        }
        public bool ShowWarning => WarnAboutScoop || WarnAboutAFMU || WarnAboutDSS;
        public bool WarnAboutScoop
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasScoopBool)
                    return false;

                return CurrentApiArgs.WarnAboutScoop;
            }
        }
        public bool WarnAboutAFMU
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasAFMUBool)
                    return false;

                return CurrentApiArgs.WarnAboutAFMU;
            }
        }
        public bool WarnAboutDSS
        {
            get
            {
                if (SelectedShip == null || SelectedShip.HasDSSBool)
                    return false;

                return CurrentApiArgs.WarnAboutDSS;
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
        public ICommand ImportStatsCommand { get; }
        public ICommand ClearAdditionDestinationsCommand { get; }
        public ICommand SelectCurrentShipCommand { get; }
        public ICommand OpenShipyardCommand { get; }
        public ICommand GetTradeStationsCommand { get; }
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

            LoadApiSave(settingsStore.SpanshRouteSettings.GetApiSave(settingsStore.SelectedCommanderID));

            ReverseRouteCommand = new ODRelayCommand(ReverseRoute, (_) => CurrentApiArgs.CanReverseRoute);
            GenerateRouteCommand = new ODRelayCommand(GenerateRoute, (_) => CanGenerate);
            SetCurrentSystemCommand = new ODRelayCommand(SetCurrentSystem, (_) =>
            {
                if (CurrentApiArgs?.RouteType == SpanshRouteType.FleetCarrier)
                {
                    var carrierLocation = ((FleetCarrierRouteApiVM)CurrentApiArgs).SquadCarrier ? fcDataStore.SquadCarrierData?.StarSystem : fcDataStore.PersonalCarrierData?.StarSystem;
                    return !string.Equals(carrierLocation, CurrentApiArgs.SourceSystem.SystemName);
                }
                return !string.Equals(CurrentSystem, CurrentApiArgs?.SourceSystem.SystemName);
            });
            SetSelectedGameSystemAsSourceCommand = new ODRelayCommand(SetSelectedGameSystemAsSource, (_) => SelectedGameRouteSystem != null);
            SetSelectedGameSystemAsTargetCommand = new ODRelayCommand(SetSelectedGameSystemAsTarget, (_) => SelectedGameRouteSystem != null);
            SetLastGameSystemAsTargetCommand = new ODRelayCommand(SetLastGameSystemAsTarget, (_) => LastGameRouteSystem != null);
            AddAdditionalSystemCommand = new ODRelayCommand<string>(AddAdditionalSystem, (name) =>
            {
                if (string.IsNullOrEmpty(name))
                    return false;
                return CurrentApiArgs.CanAddAdditional([name]);
            });
            DeleteAdditionalSystemCommand = new ODRelayCommand<ApiRouteSystemVM>(DeleteAdditionalSystem, (vm) => vm != null);
            ImportDestinationsCommand = new ODRelayCommand<IEnumerable<string>>(ImportDestinations, (_) => CurrentApiArgs.CanImportDestinations);
            ClearAdditionDestinationsCommand = new ODRelayCommand(ClearAdditionalSystems, (_) => CurrentApiArgs.CanImportDestinations);
            ChangeCarrierTypeCommand = new ODRelayCommand(ChangeCarrierType);
            ImportStatsCommand = new ODRelayCommand(ImportStats, (_) =>
            {
                if (!CurrentApiArgs.CanImportStats)
                    return false;

                if (CurrentApiArgs is FleetCarrierRouteApiVM vm)
                {
                    return vm.SquadCarrier ? fcDataStore.SquadCarrierData != null : fcDataStore.PersonalCarrierData != null;
                }
                return SelectedShip != null;
            });
            SelectCurrentShipCommand = new ODRelayCommand(SelectCurrentShip, (_) => sharedDataStore.CurrentShipInfo?.ShipId != SelectedShip?.ShipId);
            OpenShipyardCommand = new ODRelayCommand<ShipyardWebSite>(OpenShipyard, (_) => SelectedShip != null);
            GetTradeStationsCommand = new ODAsyncRelayCommand(GetTradeStations, () => IsGenerating == false);

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

        private void ImportStats(object? obj)
        {
            if (CurrentApiArgs is FleetCarrierRouteApiVM vm)
            {
                CurrentApiArgs.ImportStats(vm.SquadCarrier ? fcDataStore.SquadCarrierData : fcDataStore.PersonalCarrierData);
                return;
            }

            if (SelectedShip == null)
                return;

            CurrentApiArgs.ImportStats(SelectedShip);
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
            if (!CurrentApiArgs.SourceSystem.HasValue || !(CurrentApiArgs.TargetSystem?.HasValue ?? false))
                return;

            var source = CurrentApiArgs.SourceSystem;
            var target = CurrentApiArgs.TargetSystem;
            CurrentApiArgs.SourceSystem = target;
            CurrentApiArgs.TargetSystem = source;
        }

        private void AddAdditionalSystem(string systemName)
        {
            var added = CurrentApiArgs.AddAdditonalSystem(systemName);

            AdditionalSystemAdded?.Invoke(this, new(CurrentApiArgs.RouteType, added));
        }

        private void ApiArgVMPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShowWarning));
            OnPropertyChanged(nameof(WarnAboutScoop));
            OnPropertyChanged(nameof(WarnAboutDSS));
            OnPropertyChanged(nameof(WarnAboutAFMU));
        }

        private async Task GetTradeStations()
        {
            if (CurrentApiArgs is not TradePlannerApiVM vm)
                return;
            IsGenerating = true;
            UpdateBusyText("Checking Source System");
            await Task.Delay(1000);
            

            if (vm.SourceSystem.Id64 == 0)
            {
                await routeStore.CheckSystemNamesWithSpansh([vm.SourceSystem]);
            }

            if (vm.SourceSystem.Id64 == 0)
            {
                UpdateBusyText("Couldn't get system Id64");
                await Task.Delay(1000);
                IsGenerating = false;
                return;
            }

            UpdateBusyText("Getting Stations from Spansh");
            await Task.Delay(1000);

            var dump = await routeStore.GetSystemDump(vm.SourceSystem.Id64);

            if (dump == null || dump.System == null)
            {
                UpdateBusyText("No stations Found");
                await Task.Delay(2000);
                IsGenerating = false;
                return;
            }

            var stations = new List<SpanshStation>();

            if (dump.System.Stations.Count != 0)
                stations.AddRange(dump.System.Stations.Where(IsValidStation));

            foreach (var body in dump.System.Bodies)
            {
                if (body.Stations.Count == 0)
                    continue;

                stations.AddRange(body.Stations.Where(IsValidStation));
            }

            stations.Sort((x, y) =>
            {
                return x.Name?.CompareTo(y.Name) ?? 0;
            });

            vm.Stations.ClearCollection();
            vm.Stations.AddRange(stations.Select(station => new ApiTradePlannerStationVM(dump.System, station)));
            vm.SelectedStation = vm.Stations.FirstOrDefault();
            UpdateBusyText($"Found {stations.Count} stations");
            await Task.Delay(1000);
            IsGenerating = false;
        }

        private bool IsValidStation(SpanshStation station)
        {
            if (station.Market == null)
                return false;

            if (station.Type == null || station.Type.Contains("Carrier"))
                return false;

            if (string.Equals(station.Government, "Megaconstruction", StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
        #endregion

        #region Carrier Methods
        private void ChangeCarrierType(object? obj)
        {
            if (CurrentApiArgs is not FleetCarrierRouteApiVM vm)
                return;

            vm.SquadCarrier = !vm.SquadCarrier;

            UpdateCarrierDetails(vm);
        }

        private void UpdateCarrierDetails(FleetCarrierRouteApiVM vm)
        {
            var fcData = vm.SquadCarrier ? fcDataStore.SquadCarrierData : fcDataStore.PersonalCarrierData;

            if (fcData == null)
            {
                vm.CapicityUsed = 0;
                vm.CurrentFuel = 0;
                vm.TritInMarket = 0;
                return;
            }
            vm.SourceSystem.SystemName = fcData.StarSystem;
            vm.CapicityUsed = fcData.UsedCapacity;
            vm.CurrentFuel = fcData.FuelLevel;
            vm.TritInMarket = fcData.Stock.FirstOrDefault(x => x.FdevName == "$tritium_name;")?.StockCount ?? 0;
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
            if (CurrentApiArgs is FleetCarrierRouteApiVM vm)
            {
                var carrierData = vm.SquadCarrier ? fcDataStore.SquadCarrierData : fcDataStore.PersonalCarrierData;
                if (carrierData != null)
                {
                    CurrentApiArgs.SourceSystem = new(carrierData);
                }
                return;
            }
            if (sharedDataStore.CurrentSystem != null)
            {
                CurrentApiArgs.SourceSystem = new(sharedDataStore.CurrentSystem);
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

                    UpdateBusyText("Checking System Names");
                    await Task.Delay(500);
                    var results = await routeStore.CheckSystemNamesWithSpansh(CurrentApiArgs.GetSystemNamesToCheck());

                    if (results.Count > 0)
                    {
                        UpdateBusyText($"System Name check failed. Those in red were not found");
                        await Task.Delay(4000);
                        ResetBusyText();
                        return;
                    }                 

                    var parameters = CurrentApiArgs.GetRouteParameters(selectedShip);
                    if (parameters == null)
                    {
                        UpdateBusyText("Unable To Get Route Parameters");
                        await Task.Delay(4000);
                        ResetBusyText();
                        return;
                    }

                    UpdateBusyText("Generating Route");
                    var route = await routeStore.GenerateRoute(parameters, UpdateBusyText);
                    if (route.Item1 == false)
                    {
                        if (route.Item2 == null)
                        {
                            //Just incase we had an id that is no longer valid
                            CurrentApiArgs.ApiParameters.JobId = null;
                            await ErrorBusyTextMessage("Failed to get route");
                            ResetBusyText();
                            return;
                        }
                        if (string.IsNullOrEmpty(route.Item2.Job))
                        {
                            //Just incase we had an id that is no longer valid
                            CurrentApiArgs.ApiParameters.JobId = null;
                            await ErrorBusyTextMessage("Failed to get route");
                            await Task.Delay(4000);
                            ResetBusyText();
                            return;
                        }
                        CurrentApiArgs.ApiParameters.JobId = route.Item2.Job;
                        await ErrorBusyTextMessage($"Failed to get route but we have a job number.\nWait a couple of minutes and click\ngerenate route again to check again.\n\nStatus : {route.Item2.Status}\nState : {route.Item2.State}");
                        await Task.Delay(10000);
                    }
                    if (route.Item2 != null)
                        CurrentApiArgs.ApiParameters.JobId = route.Item2.Job;
                    if (route.Item2 is ApiGalaxyPlotterRoute galaxy && !string.IsNullOrEmpty(galaxy.Result.Reason))
                    {
                        UpdateBusyText($"Response from Spansh\n{galaxy.Result.Reason}");
                        await Task.Delay(10000);
                        ResetBusyText();
                        return;
                    }
                    if (route.Item2 is ApiTradePlannerRoute trade && trade.Result.Count == 0)
                    {
                        UpdateBusyText("Spansh returned an empty route\n\nTry increasing the hop distance");
                        await Task.Delay(10000);
                        ResetBusyText();
                        return;

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
                    case SpanshRouteType.TradePlanner:
                        responseBase = JsonConvert.DeserializeObject<ApiTradePlannerRoute>(json);
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
        internal void LoadApiSave(List<SpanshAPISave> savedata)
        {
            apiArgsVM.Clear();
            foreach (var args in savedata)
            {
                var vm = GetApiBase(args.RouteType);
                vm.LoadFromSave(args);
                if (!apiArgsVM.TryAdd(args.RouteType, vm))
                {
                    continue;
                }
            }

            ApiBase? galaxyArgs = null;

            if (apiArgsVM.TryGetValue(SpanshRouteTypeComboBox.GalaxyPlotter, out var routeSystems))
            {
                galaxyArgs = routeSystems;
            }
            galaxyArgs ??= GetApiBase(SpanshRouteTypeComboBox.GalaxyPlotter);
            galaxyArgs.PropertyChanged += ApiArgVMPropertyChanged;

            OnPropertyChanged(nameof(CurrentApiArgs));
        }

        internal void SaveAPIData()
        {
            var saveData = new List<SpanshAPISave>();

            foreach (var args in apiArgsVM)
            {
                var save = args.Value.GetSave();
                saveData.Add(save);
            }

            settingsStore.SpanshRouteSettings.SaveAPiData(settingsStore.SelectedCommanderID, saveData);
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
