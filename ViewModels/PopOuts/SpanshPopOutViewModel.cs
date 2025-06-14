using Newtonsoft.Json.Linq;
using NLog.Targets;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Spansh;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public sealed class SpanshPopOutViewModel : PopOutViewModel
    {
        public SpanshPopOutViewModel(SpanshCsvStore csvStore, SettingsStore settings, NotificationService notificationService) 
        {
            this.csvStore = csvStore;
            this.settingsStore = settings;
            this.notificationService = notificationService;

            this.csvStore.StoreLive += OnCSVStoreLive;
            this.csvStore.OnCurrentTargetChanged += OnCurrentTargetChanged;
            this.csvStore.OnCurrentContainerChanged += CsvStore_OnCurrentContainerChanged;

            var currentContainer = this.csvStore.GetCurrentContainer(this.settingsStore.SpanshCSVSettings[this.settingsStore.SelectedCommanderID]);
            CsvStore_OnCurrentContainerChanged(null, currentContainer);

            PreviousTargetCommand = new ODRelayCommand(SelectPreviousTarget, (_) => CurrentIndex > 0);
            NextTargetCommand = new ODRelayCommand(SelectNextTarget, (_) => NextTarget != null);
            SetCurrentCSVType = new ODRelayCommand<CsvType>(SetCSVType, (csvType) => CurrentType != csvType);
            CopyToClipboard = new ODRelayCommand<ExplorationTargetViewModel>(CopySystemNameToClipboard, (targetVM) => targetVM != null);
            ImportCSV = new ODRelayCommand(OnImportCSV);
            DeleteCSVCommand = new ODRelayCommand(OnDeleteCSV, (_) => CurrentTarget != null);

            if (this.csvStore.IsLive)
                OnCSVStoreLive(this.csvStore, true);
        }

        protected override void Dispose()
        {
            this.csvStore.StoreLive -= OnCSVStoreLive;
            this.csvStore.OnCurrentTargetChanged -= OnCurrentTargetChanged;
            this.csvStore.OnCurrentContainerChanged -= CsvStore_OnCurrentContainerChanged;
        }

        private readonly SpanshCsvStore csvStore;
        private readonly SettingsStore settingsStore;
        private readonly NotificationService notificationService;

        public override string Name => "Spansh Overlay";

        public override bool IsLive => csvStore.IsLive;

        public override Uri TitleBarIcon => new("/Assets/Icons/spanshbtn.png", UriKind.Relative);

        public event EventHandler<SpanshCsvErrorEventArgs>? OnErrorProcessingCSV;

        public CsvTypeComboBox CurrentTypeBox
        {
            get
            {
                if (Enum.TryParse<CsvTypeComboBox>(CurrentType.ToString(), out var result))
                {
                    return result;
                }
                return CsvTypeComboBox.RoadToRiches;
            }
            set
            {
                if (Enum.TryParse<CsvType>(value.ToString(), out var result))
                {
                    CurrentType = result;
                }
                
                OnPropertyChanged(nameof(CurrentTypeBox));
            }
        }

        public CsvType CurrentType
        {
            get => settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID];
            set
            {
                settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID] = value;
                //CsvStore_OnCurrentContainerChanged(null, csvStore.GetCurrentContainer(value));
                OnPropertyChanged(nameof(CurrentType));
            }
        }
        public ObservableCollection<ExplorationTargetViewModel> Targets { get; private set; } = [];

        private ExplorationTargetViewModel? currentTarget;
        public ExplorationTargetViewModel? CurrentTarget
        {
            get => currentTarget;
            private set
            {
                currentTarget = value;
                OnPropertyChanged(nameof(CurrentTarget));
            }
        }

        private ExplorationTargetViewModel? nextTarget;
        public ExplorationTargetViewModel? NextTarget
        {
            get => nextTarget;
            private set
            {
                nextTarget = value;
                OnPropertyChanged(nameof(NextTarget));
            }
        }

        public int CurrentIndex => csvStore.CurrentIndex;
        public int RemainingCount => Targets.Count > 0 ? Targets.Count - CurrentIndex - 1 : 0;
        #region Commands
        public ICommand PreviousTargetCommand { get; }
        public ICommand NextTargetCommand { get; }
        public ICommand SetCurrentCSVType { get; }
        public ICommand CopyToClipboard { get; }
        public ICommand ImportCSV { get; }
        public ICommand DeleteCSVCommand { get; }

        private void SelectNextTarget(object? obj)
        {
            csvStore.CurrentIndex++;
        }

        private void SelectPreviousTarget(object? obj)
        {
            csvStore.CurrentIndex--;
        }

        private void SetCSVType(CsvType type)
        {
            CurrentType = type;
        }

        private void OnDeleteCSV(object? obj)
        {
            csvStore.DeleteContainer(CurrentType);
        }

        private void CopySystemNameToClipboard(ExplorationTargetViewModel model)
        {
            if (model == null)
            {
                return;
            }

            notificationService.SetClipboard(model.SystemName);
        }

        private void OnImportCSV(object? obj)
        {
            var filename = ODDialogService.OpenFileDialog("Browse csv files", "csv", "csv files (*.csv)|*.csv", 2);

            if (string.IsNullOrEmpty(filename))
                return;

            ParseCSV(filename);
        }
        #endregion

        private void OnCSVStoreLive(object? sender, bool e)
        {
            OnModelLive();
        }

        private void CsvStore_OnCurrentContainerChanged(object? sender, SpanshCsvContainer? e)
        {
            CurrentTarget = null;
            NextTarget = null;
            Targets.ClearCollection();

            if (csvStore.CurrentContainer is null)
            {
                return;
            }

            Targets.AddRange(csvStore.CurrentContainer?.Targets.Select(x => new ExplorationTargetViewModel(x)) ?? []);
            OnPropertyChanged(nameof(Targets));
            OnCurrentTargetChanged(null, csvStore.CurrentTarget);
        }

        private void OnCurrentTargetChanged(object? sender, ExplorationTarget? e)
        {
            if (CurrentIndex < 0)
            {
                currentTarget = null;
                nextTarget = null;
                OnPropertyChanged(nameof(CurrentTarget));
                OnPropertyChanged(nameof(NextTarget));
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(RemainingCount));
                OnPropertyChanged(nameof(CurrentType));
                OnPropertyChanged(nameof(CurrentTypeBox));
                return;
            }
            currentTarget = CurrentIndex < Targets.Count - 1 ? Targets[CurrentIndex]
                : Targets.FirstOrDefault(x => x.SystemName == csvStore.CurrentTarget?.SystemName);

            nextTarget = Targets.FirstOrDefault(x => x.SystemName == csvStore.NextTarget?.SystemName);
            OnPropertyChanged(nameof(CurrentTarget));
            OnPropertyChanged(nameof(NextTarget));
            OnPropertyChanged(nameof(CurrentIndex));
            OnPropertyChanged(nameof(RemainingCount));
            OnPropertyChanged(nameof(CurrentType));
            OnPropertyChanged(nameof(CurrentTypeBox));
        }

        public void ParseCSV(string fileName)
        {
            var csv = csvStore.ParseCSV(fileName);

            if (csv)
            {
                OnPropertyChanged(nameof(CurrentType));
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.Parse));
        }

        public void ForceParseCSV(string fileName, CsvType csvType)
        {
            var csv = csvStore.ForceParseCSV(fileName, csvType);

            if (csv)
            {
                OnPropertyChanged(nameof(CurrentType));
                return;
            }

            OnErrorProcessingCSV?.Invoke(this, new(fileName, SpanshCSVError.ForcePass));
        }

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 900, 500);
        }
    }
}
