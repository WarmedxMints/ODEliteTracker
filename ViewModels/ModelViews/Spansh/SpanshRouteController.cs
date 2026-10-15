using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class SpanshRouteController : ODObservableObject, IDisposable
    {
        private readonly SpanshRouteStore spanshStore;
        private readonly NotificationService notificationService;
        private readonly SettingsStore settingsStore;
        private readonly SharedDataStore sharedData;

        public event EventHandler<SpanshCsvErrorEventArgs>? OnErrorProcessingCSV;

        #region Public Properties
        public ObservableCollection<SpanshRouteContainerVM> RouteContainers { get; } = [];   
        private SpanshRouteFilterEnum contianerFilter = SpanshRouteFilterEnum.ShowAll;
        public SpanshRouteFilterEnum ContainerFilter
        {
            get => contianerFilter;
            set
            {
                contianerFilter = value;
                OnPropertyChanged(nameof(ContainerFilter));
                OnPropertyChanged(nameof(FilteredContainers));
            }
        }
        public IEnumerable<SpanshRouteContainerVM> FilteredContainers
        {
            get
            {
                if (contianerFilter == SpanshRouteFilterEnum.ShowAll)
                    return RouteContainers.OrderBy(x => x.RouteType.GetEnumDescription()).ThenBy(x => x.From);

                return RouteContainers.Where(x => x.RouteType == (SpanshRouteType)contianerFilter)
                    .OrderBy(x => x.From);
            }
        }
        private SpanshRouteContainerVM? currentContainer;
        public SpanshRouteContainerVM? CurrentContainer
        {
            get => currentContainer;
            set
            {
                currentContainer = value;
                SelectedContainer = value;
                OnPropertyChanged(nameof(CurrentContainer));
                OnPropertyChanged(nameof(SelectedContainer));
                OnPropertyChanged(nameof(CurrentType));
            }
        }
        private SpanshRouteContainerVM? selectedContainer;
        public SpanshRouteContainerVM? SelectedContainer { get => selectedContainer; set { selectedContainer = value; OnPropertyChanged(nameof(SelectedContainer)); } }
        private SpanshRouteTargetViewModel? currentTarget;
        public SpanshRouteTargetViewModel? CurrentTarget
        {
            get => currentTarget;
            private set
            {
                currentTarget = value;
                OnPropertyChanged(nameof(CurrentTarget));
            }
        }

        private SpanshRouteTargetViewModel? nextTarget;
        public SpanshRouteTargetViewModel? NextTarget
        {
            get => nextTarget;
            private set
            {
                nextTarget = value;
                OnPropertyChanged(nameof(NextTarget));
            }
        }

        public int CurrentIndex => spanshStore.CurrentIndex;
        public int RemainingCount
        {
            get
            {
                if (CurrentContainer == null)
                    return 0;
                return CurrentContainer.Targets.Count > 0 ? CurrentContainer.Targets.Count - CurrentIndex - 1 : 0;
            }
        }

        public bool AutoCopyNextSystem
        {
            get => settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard;
            set
            {
                settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard = value;
                OnPropertyChanged(nameof(AutoCopyNextSystem));
            }
        }

        private SpanshRouteType currentType = SpanshRouteType.RoadToRiches;
        public SpanshRouteType CurrentType
        {
            get
            {
                if (CurrentContainer == null)
                    return currentType;
                return CurrentContainer.RouteType;
            }
        }
        public string TimeRemaining
        {
            get
            {
                if (CurrentType != SpanshRouteType.FleetCarrier)
                    return string.Empty;

                var duration = TimeSpan.FromMinutes(20) * RemainingCount;
                return $"{duration.Hours} h {duration.Minutes} m";
            }
        }

        public int SelectedTab
        {
            get => settingsStore.SpanshCSVSettings.RouteControllerSelectedTab;
            set
            {
                settingsStore.SpanshCSVSettings.RouteControllerSelectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
            }
        }
        #endregion
        public SpanshRouteController(SpanshRouteStore spanshStore,
                                     NotificationService notificationService,
                                     SettingsStore settingsStore,
                                     SharedDataStore sharedData)
        {
            this.spanshStore = spanshStore;
            this.notificationService = notificationService;
            this.settingsStore = settingsStore;
            this.sharedData = sharedData;
            
            this.spanshStore.StoreLive += OnSpanshStoreLive;
            this.spanshStore.OnCurrentTargetChanged += (s, e) => OnCurrentTargetChanged(s, e);
            this.spanshStore.OnCurrentContainerChanged += (s,e) => OnCurrentContainerChanged(s,e);

            PreviousTargetCommand = new ODRelayCommand(SelectPreviousTarget, (_) => CurrentIndex > 0);
            NextTargetCommand = new ODRelayCommand(SelectNextTarget, (_) => NextTarget != null);
            SetCurrentRouteTypeCommand = new ODRelayCommand<SpanshRouteType>(SetRouteType, (routeType) => CurrentType != routeType);
            CopyToClipboardCommand = new ODRelayCommand<SpanshRouteTargetViewModel>(CopySystemNameToClipboard, (targetVM) => targetVM != null);
            DeleteRouteCommand = new ODRelayCommand<SpanshRouteContainerVM>(DeleteRoute, (vm) => vm != null);
            LoadSavedRouteCommand = new ODRelayCommand(LoadSavedRoute, (_) => SelectedContainer != null);
            RouteContainers.CollectionChanged += RouteContainers_CollectionChanged;
            if (spanshStore.IsLive)
                OnSpanshStoreLive(null, true);
        }

        private void LoadSavedRoute(object? obj)
        {
            if (SelectedContainer == null)
                return;

            spanshStore.LoadContainer(SelectedContainer.RouteContainer);
        }

        private void RouteContainers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FilteredContainers));
        }

        #region Spansh Store Event Listeners
        private Task OnSpanshStoreLive(object? sender, bool e)
        {
            RouteContainers.ClearCollection();

            if (spanshStore.Containers.Count > 0)
            {
                RouteContainers.AddRange(spanshStore.Containers.Select(x => new SpanshRouteContainerVM(x)));
            }
    
            OnCurrentContainerChanged(null, spanshStore.CurrentContainer);
            return Task.CompletedTask;
        }

        private void OnCurrentTargetChanged(object? sender, SpanshTargetSystem? e, bool notification = false)
        {
            if (CurrentContainer == null || CurrentIndex < 0)
            {
                currentTarget = null;
                nextTarget = null;
                OnPropertyChanged(nameof(CurrentTarget));
                OnPropertyChanged(nameof(NextTarget));
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(RemainingCount));
                OnPropertyChanged(nameof(TimeRemaining));
                OnPropertyChanged(nameof(CurrentType));
                OnPropertyChanged(nameof(CurrentContainer));
                return;
            }
            currentTarget = CurrentIndex < CurrentContainer.Targets.Count - 1 ? CurrentContainer.Targets[CurrentIndex]
                : CurrentContainer.Targets.FirstOrDefault(x => x.SystemName == spanshStore.CurrentTarget?.SystemName);

            nextTarget = CurrentContainer.Targets.FirstOrDefault(x => x.SystemName == spanshStore.NextTarget?.SystemName);

            if (notification && nextTarget != null)
            {
                notificationService.SetClipboard($"{nextTarget.SystemName}");
            }
            OnPropertyChanged(nameof(CurrentTarget));
            OnPropertyChanged(nameof(NextTarget));
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(RemainingCount));
            OnPropertyChanged(nameof(TimeRemaining));
            OnPropertyChanged(nameof(CurrentType));
            OnPropertyChanged(nameof(CurrentContainer));
        }

        private void OnCurrentContainerChanged(object? sender, SpanshRouteContainer? e, bool notification = false)
        {
            CurrentTarget = null;
            NextTarget = null;
            CurrentContainer = null;
            if (e == null)
            {
                OnPropertyChanged(nameof(CurrentTarget));
                OnPropertyChanged(nameof(NextTarget));
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(RemainingCount));
                OnPropertyChanged(nameof(TimeRemaining));
                OnPropertyChanged(nameof(CurrentType));
                OnPropertyChanged(nameof(CurrentContainer));
                return;
            }

            CurrentContainer = RouteContainers.FirstOrDefault(x => x.RouteContainer == e);
            //Must be a new one
            if (CurrentContainer == null)
            {
                var newComtainer = new SpanshRouteContainerVM(e);
                RouteContainers.AddItem(newComtainer);
                CurrentContainer = newComtainer;
                notification = true;
                //SelectedTab = 0;
            }
            OnCurrentTargetChanged(null, spanshStore.CurrentTarget, notification);
        }
        #endregion

        #region Commands
        public ICommand PreviousTargetCommand { get; }
        public ICommand NextTargetCommand { get; }
        public ICommand SetCurrentRouteTypeCommand { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand DeleteRouteCommand { get; }
        public ICommand LoadSavedRouteCommand { get; }

        private void SelectNextTarget(object? obj)
        {
            spanshStore.CurrentIndex++;
        }

        private void SelectPreviousTarget(object? obj)
        {
            spanshStore.CurrentIndex--;
        }

        private void SetRouteType(SpanshRouteType type)
        {
            currentType = type;
            var container = spanshStore.GetCurrentContainer(type);
            OnCurrentContainerChanged(null, container);
        }

        private void DeleteRoute(SpanshRouteContainerVM? vm)
        {
            if (vm == null)
                return;

            var contianer = spanshStore.DeleteContainer(vm.RouteContainer);
            RouteContainers.Remove(vm);
            OnCurrentContainerChanged(null, contianer);
        }

        private void CopySystemNameToClipboard(SpanshRouteTargetViewModel model)
        {
            if (model == null)
            {
                return;
            }

            notificationService.SetClipboard(model.SystemName);
        }

        internal void ImportCSV(string? filename)
        {
            if (string.IsNullOrEmpty(filename))
                return;

            ParseCSV(filename);
        }
        #endregion

        #region CSV Parsing
        public void ParseCSV(string fileName)
        {
            var newContainer = spanshStore.ParseCSV(fileName);

            if (newContainer != null)
            {
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.Parse));
        }

        public void ForceParseCSV(string fileName, SpanshRouteType csvType)
        {
            var newContainer = spanshStore.ForceParseCSV(fileName, csvType);

            if (newContainer != null)
            {
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.ForcePass));
        }
        #endregion

        public void Dispose()
        {
            this.spanshStore.StoreLive -= OnSpanshStoreLive;
            this.spanshStore.OnCurrentTargetChanged -= (s,e) => OnCurrentTargetChanged(s, e);
            this.spanshStore.OnCurrentContainerChanged -= (s, e) => OnCurrentContainerChanged(s, e);
        }

        internal void MoveToSystem(SpanshRouteTargetViewModel target)
        {
            spanshStore.SetSelectedSystem(target.Target);
        }
    }
}
