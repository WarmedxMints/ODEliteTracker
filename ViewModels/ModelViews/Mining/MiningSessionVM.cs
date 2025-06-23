using ODEliteTracker.Models.Mining;
using ODMVVM.ViewModels;
using System.Windows;

namespace ODEliteTracker.ViewModels.ModelViews.Mining
{
    public sealed class MiningSessionVM(MiningSession session) : ODObservableObject
    {
        public MiningSession Session { get; } = session;

        public string Location { get; } = string.IsNullOrEmpty(session.Location) ? session.StarSystem : session.Location;
        public string SessionTime
        {
            get
            {
                if(Session.HasData == false)
                {
                    return "00:00:00 │ 0 t/hr";
                }

                var time = Session.TimeFinished < DateTime.MaxValue ? Session.TimeFinished - Session.TimeStarted : DateTime.UtcNow - Session.TimeStarted;

                var refined = Items.Sum(x => x.MiningItem.RefinedCount);

                var rate = refined / time.TotalHours;

                return $"{time:hh}:{time:mm}:{time:ss} │ {rate:N1} t/hr";
            }
        }

        public List<MiningItemVM> Items { get; } = [.. session.Items.Select(x => new MiningItemVM(x, session.AsteroidsProspected))];

        private IEnumerable<MiningProspectorVM> prospectors = [.. session.Prospectors.Select(x => new MiningProspectorVM(x))];
        public IEnumerable<MiningProspectorVM> Prospectors
        {
            get => prospectors;
            set
            {
                prospectors = value;
                OnPropertyChanged(nameof(Prospectors));
            }
        }
        public IEnumerable<MiningItemVM> SortedItems
        {
            get
            {
                return Items.OrderByDescending(x => x.Type == MiningItemType.Ore)
                    .ThenByDescending(x => x.MiningItem.RefinedCount)
                    .ThenByDescending(x => x.MiningItem.CollectedCount);
            }
        }
        public string AsteroidsProspected => Session.AsteroidsProspected.ToString("N0");
        public string AsteroidsCracked => Session.AsteroidsCracked.ToString("N0");
        public string ProspectorsFired => Session.ProspectorsFired.ToString("N0");
        public string CollectorsDeployed => Session.CollectorsDeployed.ToString("N0");
        public string AsteroidMaterialContents => $"Material Contents (L | M | H): {Session.LowContent:N0} │ {Session.MedContent:N0} │ {Session.HighContent:N0}";  
        public string InfoText
        {
            get
            {
                return $"{SessionTime} │ {Items.Where(x => x.Type == MiningItemType.Ore).Sum(x => x.MiningItem.RefinedCount)}t Refined";
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        public void UpdateSessionTime()
        {
            OnPropertyChanged(nameof(SessionTime));
            OnPropertyChanged(nameof(InfoText));
        }

        public void SessionUpdated(MiningSession miningSession)
        {
            if (miningSession != Session)
                return;

            OnPropertyChanged(nameof(AsteroidsProspected));
            OnPropertyChanged(nameof(AsteroidsCracked));
            OnPropertyChanged(nameof(ProspectorsFired));
            OnPropertyChanged(nameof(CollectorsDeployed));
            OnPropertyChanged(nameof(AsteroidMaterialContents));

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                prospectors = [.. Session.Prospectors.Select(x => new MiningProspectorVM(x))];

                OnPropertyChanged(nameof(Prospectors));
            }));
            foreach (var item in miningSession.Items)
            {
                var known = Items.FirstOrDefault(x => x.MiningItem == item);

                if(known != null)
                {
                    known.Update(miningSession.AsteroidsProspected);
                    continue;
                }
                Items.Add(new(item, miningSession.AsteroidsProspected));
            }

            OnPropertyChanged(nameof(SortedItems));
        }
    }
}
