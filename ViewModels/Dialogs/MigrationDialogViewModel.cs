using ODEliteTracker.ViewModels.ModelViews;
using ODJournalDatabase.Database.Interfaces;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.Dialogs
{
    public sealed class MigrationDialogViewModel : ODObservableObject
    {
        private readonly IODDatabaseProvider databaseProvider;
        private readonly JournalCommanderVM currentCommander;

        public ICommand MigrateCommander { get; }

        public EventHandler<bool>? OnMigrationComplete;

        public MigrationDialogViewModel(IODDatabaseProvider databaseProvider,
                                        JournalCommanderVM currentCommander) 
        {
            this.databaseProvider = databaseProvider;
            this.currentCommander = currentCommander;

            MigrateCommander = new ODAsyncRelayCommand(OnMigrateCommnader);

            _ = Initialise();
        } 

        private async Task Initialise()
        {
            var commanders = await databaseProvider.GetAllJournalCommanders(true);

            var vms = commanders.Where(x => x.MigratedTo < 0 && x.Id != currentCommander.Id).Select(x => new JournalCommanderVM(x));

            JournalCommanders.ClearCollection();

            foreach (var commander in vms)
            {
                JournalCommanders.AddItem(commander);
            }

            SelectCommander = JournalCommanders.FirstOrDefault();

            OnPropertyChanged(nameof(SelectCommander));
            OnPropertyChanged(nameof(CurrentCommander));
            OnPropertyChanged(nameof(JournalCommanders));
        }

        public JournalCommanderVM CurrentCommander => currentCommander;

        private JournalCommanderVM? selectedCommander;
        public JournalCommanderVM? SelectCommander
        {
            get => selectedCommander;
            set
            {
                selectedCommander = value;
                OnPropertyChanged(nameof(SelectCommander));
            }
        }

        private ObservableCollection<JournalCommanderVM> journalCommanders = [];
        public ObservableCollection<JournalCommanderVM> JournalCommanders
        {
            get => journalCommanders;
            set
            {
                journalCommanders = value;
                OnPropertyChanged(nameof(JournalCommanders));
            }
        }

        private bool uiEnabled = true;
        public bool UiEnabled
        {
            get => uiEnabled;
            set
            {
                uiEnabled = value;
                OnPropertyChanged(nameof(UiEnabled));
            }
        }

        private string migrationMessage = "Migrate into";
        public string MigrationMessage
        {
            get => migrationMessage;
            set
            {
                migrationMessage = value;
                OnPropertyChanged(nameof(MigrationMessage));
            }
        }

        private async Task OnMigrateCommnader()
        {
            MigrationMessage = "Migrating...";
            UiEnabled = false;

            if (selectedCommander == null)
            {
                MigrationMessage = "Migration failed";
                OnMigrationComplete?.Invoke(this, false);
                return;
            }
            //EF Core seems to bloock the UI for some reason, Task.Delay does not;
            await Task.Delay(500);
            await databaseProvider.MigrateCommander(selectedCommander.Id, currentCommander.Id).ConfigureAwait(true);
            OnMigrationComplete?.Invoke(this, true);
            UiEnabled = true;
        }
    }
}
