using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Database;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.Services;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using System.Windows;

namespace ODEliteTracker.Stores
{
    public sealed class SpanshCsvStore : LogProcessorBase
    {
        public SpanshCsvStore(IManageJournalEvents parserStore, IODDatabaseProvider odToolsDatabase, SettingsStore settingsStore, NotificationService notificationService)
        {
            this.parserStore = parserStore;
            this.settingsStore = settingsStore;
            this.notificationService = notificationService;

            databaseProvider = (ODEliteTrackerDatabaseProvider)odToolsDatabase;

            this.settingsStore.SpanshCSVSettings.CSVChanged += OnCSVChanged;
            parserStore.RegisterLogProcessor(this);
        }

        private readonly IManageJournalEvents parserStore;
        private readonly SettingsStore settingsStore;
        private readonly NotificationService notificationService;
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private Dictionary<CsvType, SpanshCsvContainer> containers = [];

        public event EventHandler<ExplorationTarget?>? OnCurrentTargetChanged;
        public event EventHandler<SpanshCsvContainer?>? OnCurrentContainerChanged;

        public SpanshCsvContainer? CurrentContainer { get; private set; }

        private ExplorationTarget? currentTarget;
        public ExplorationTarget? CurrentTarget
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

        public ExplorationTarget? NextTarget
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

        public SpanshCsvContainer SetCurrentCSVContainer(CsvType csvType)
        {
            if (settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID] != csvType)
            {
                settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID] = csvType;
            }
            CurrentTarget = null;
            var ret = GetCurrentContainer(csvType);
            OnCurrentContainerChanged?.Invoke(this, ret);
            return ret;
        }

        public void DeleteContainer(CsvType csvType)
        {
            if (containers.TryGetValue(csvType, out var container))
            {
                containers.Remove(csvType);
                CurrentContainer = new SpanshCsvContainer([], 0); ;
                CurrentIndex = -1;
                databaseProvider.DeleteCSV(csvType, settingsStore.SelectedCommanderID);
                OnCurrentContainerChanged?.Invoke(this, CurrentContainer);
            }
        }

        public SpanshCsvContainer GetCurrentContainer(CsvType csvType)
        {
            if (containers.TryGetValue(csvType, out var container))
            {
                CurrentContainer = container;
                CurrentIndex = CurrentContainer.CurrentIndex;
                return container;
            }

            var ret = new SpanshCsvContainer([], 0);
            CurrentContainer = ret;
            CurrentIndex = -1;
            return ret;
        }

        public void SaveCSVs()
        {
            var toSave = containers.Where(x => x.Value.HasValue).ToDictionary();

            databaseProvider.SaveCVSs(toSave, settingsStore.SelectedCommanderID);
        }

        #region IProcessJournalLogs Implementation

        public override Dictionary<JournalTypeEnum, bool> EventsToParse
        {
            get => new()
            {
                { JournalTypeEnum.Fileheader, true },
                { JournalTypeEnum.Location, false },
                { JournalTypeEnum.FSDJump, false},
                { JournalTypeEnum.CarrierJump, false},                
            };
        }

        public override string StoreName => "Spansh CSV Store";

        public override void ClearData()
        {
            IsLive = false;
            containers.Clear();
            CurrentContainer = null;
            CurrentTarget = null;
        }

        public override void Dispose()
        {
            this.settingsStore.SpanshCSVSettings.CSVChanged -= OnCSVChanged;
            parserStore.UnregisterLogProcessor(this);
        }

        public override void SaveData()
        {
            SaveCSVs();
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
            _ = Task.Factory.StartNew(() =>
            {
                containers = databaseProvider.GetSpanshCSVsDictionary(settingsStore.SelectedCommanderID);
                SetCurrentCSVContainer(settingsStore.SpanshCSVSettings[settingsStore.SelectedCommanderID]);
            });
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

                    if (settingsStore.SpanshCSVSettings.AutoCopySystemToClipboard)
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

        private void OnCSVChanged(object? sender, CsvType e)
        {
            SetCurrentCSVContainer(e);
            SaveCSVs();
        }

        public bool ParseCSV(string filename)
        {
            var csv = SpanshCSVParser.ParseCsv(filename);

            if (csv is null)
            {
                return false;
            }

            if (containers.ContainsKey(csv.CsvType))
            {
                containers[csv.CsvType] = new(csv.Targets, 0);
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            if (containers.TryAdd(csv.CsvType, new(csv.Targets, 0)))
            {
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            return false;
        }

        public bool ForceParseCSV(string filename, CsvType csvType)
        {
            var csv = SpanshCSVParser.ForceParse(filename, csvType);

            if (csv is null)
            {
                return false;
            }

            if (containers.ContainsKey(csv.CsvType))
            {
                containers[csv.CsvType] = new(csv.Targets, 0);
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            if (containers.TryAdd(csv.CsvType, new(csv.Targets, 0)))
            {
                SetCurrentCSVContainer(csv.CsvType);
                SaveCSVs();
                return true;
            }
            return false;
        } 
    }
}
