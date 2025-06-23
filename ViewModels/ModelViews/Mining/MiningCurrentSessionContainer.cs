using ODEliteTracker.Stores;
using ODMVVM.ViewModels;

namespace ODEliteTracker.ViewModels.ModelViews.Mining
{
    public class MiningCurrentSessionContainer : ODObservableObject, IDisposable
    {
        private readonly MiningDataStore miningData;
        private readonly Timer sessionTimeUpdateTimer;

        public MiningCurrentSessionContainer(MiningDataStore miningData)
        {
            this.miningData = miningData;
            this.miningData.CurrentSessionUpdated += OnCurrentSessionUpdated;

            sessionTimeUpdateTimer = new Timer(OnUpdateTime, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            miningData.CurrentSessionUpdated -= OnCurrentSessionUpdated;
        }

        public EventHandler? SessionUpdated;

        private MiningSessionVM? currentSession;
        public MiningSessionVM? CurrentSession
        {
            get => currentSession;
            set
            {
                if (currentSession != null)
                    currentSession.IsSelected = false;
                currentSession = value;

                OnPropertyChanged(nameof(CurrentSession));
                SessionUpdated?.Invoke(this, EventArgs.Empty);
                if(currentSession == null)
                {
                    sessionTimeUpdateTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                    return;
                }
                sessionTimeUpdateTimer.Change(TimeSpan.Zero, new TimeSpan(0,0,1));
            }
        }

        public void OnCurrentSessionUpdated(object? sender, EventArgs e)
        {
            if (currentSession == null)
            {
                if (miningData.CurrentSession != null)
                {
                    CurrentSession = new(miningData.CurrentSession);
                    return;
                }               
                return;
            }

            if (miningData.CurrentSession == null)
            {
                if(CurrentSession != null)
                    CurrentSession.IsSelected = false;
                CurrentSession = null;
                return;
            }

            if (currentSession.Session == miningData.CurrentSession)
            {
                currentSession.SessionUpdated(miningData.CurrentSession);
                return;
            }

            CurrentSession = new(miningData.CurrentSession);
        }

        private void OnUpdateTime(object? state)
        {
            if (currentSession == null)
                return;

            currentSession.UpdateSessionTime();
        }
    }
}
