using ODJournalDatabase.JournalManagement;
using ODMVVM.ViewModels;
using System.IO;

namespace ODEliteTracker.ViewModels.ModelViews
{
    public class JournalCommanderVM(JournalCommander cmdr) : ODObservableObject
    {
        public string Name => cmdr.Name;
        public int Id => cmdr.Id;
        public int MigratedTo => cmdr.MigratedTo;

        private string journalPath = cmdr.JournalPath ?? string.Empty;
        public string JournalPath
        {
            get => journalPath;
            set
            {
                journalPath = value;
                OnPropertyChanged(nameof(JournalPath));
            }
        }

        private string lastFile = Path.GetFileName(cmdr.LastFile ?? string.Empty);
        public string LastFile
        {
            get => lastFile;
            set
            {
                lastFile = value;
                OnPropertyChanged(nameof(LastFile));
            }
        }

        private bool isHidden = cmdr.IsHidden;
        public bool IsHidden
        {
            get => isHidden;
            set
            {
                isHidden = value;
                OnPropertyChanged(nameof(IsHidden));
            }
        }

        private bool useCAPI = cmdr.UseCAPI;
        public bool UseCAPI
        {
            get => useCAPI;
            set
            {
                useCAPI = value;
                OnPropertyChanged(nameof(UseCAPI));
            }
        }
    }
}
