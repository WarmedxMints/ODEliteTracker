using ODEliteTracker.Models.Ship;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews;
using ODEliteTracker.ViewModels.ModelViews.Mining;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class MiningViewModel : ODViewModel
    {
        public MiningViewModel(MiningDataStore miningData,SharedDataStore sharedData, PopOutService popOutService, SettingsStore settings) 
        {
            this.miningData = miningData;
            this.sharedData = sharedData;
            this.popOutService = popOutService;
            this.settings = settings;
            CurrentContainer = new(miningData);
            CurrentContainer.SessionUpdated += OnCurrentSessionUpdated;
            ProspectorContainer = new(miningData);

            SelectSession = new ODRelayCommand<MiningSessionVM>(OnSelectSession, 
                (obj) =>
                {
                    if (obj is MiningSessionVM vm)
                    {
                        return vm != null;
                    }
                    return false;
                });
            OpenPopOut = new ODRelayCommand<Type>(OnOpenPopOut);

            this.miningData.StoreLive += OnStoreLive;
            this.miningData.SessionsUpdated += OnPreviousSessionsUpdated;

            this.sharedData.ShipChangedEvent += OnShipChanged;
            this.sharedData.ShipCargoUpdatedEvent += OnCargoUpdated;

            if (this.miningData.IsLive)
                OnStoreLive(this.miningData, true);
        }

        public override void Dispose()
        {
            miningData.StoreLive -= OnStoreLive;
            miningData.SessionsUpdated -= OnPreviousSessionsUpdated;
            CurrentContainer.SessionUpdated -= OnCurrentSessionUpdated;
            CurrentContainer.Dispose();
            ProspectorContainer.Dispose();
        }

        private readonly MiningDataStore miningData;
        private readonly SharedDataStore sharedData;
        private readonly PopOutService popOutService;
        private readonly SettingsStore settings;

        public override bool IsLive => miningData.IsLive;

        public MiningCurrentSessionContainer CurrentContainer { get; }
        public MiningProspectorContainer ProspectorContainer { get; }

        private MiningSessionVM? selectedSession;
        public MiningSessionVM? SelectedSession
        {
            get => selectedSession;
            set
            {
                if (selectedSession != null)
                    selectedSession.IsSelected = false;
                selectedSession = value;
                if (selectedSession != null)
                    selectedSession.IsSelected = true;
                if (value != null)
                    selectedSession?.SessionUpdated(value.Session);
                OnPropertyChanged(nameof(SelectedSession));
            }
        }

        private ObservableCollection<MiningSessionVM> previousSessions = [];
        public ObservableCollection<MiningSessionVM> PreviousSessions
        {
            get => previousSessions;
            set
            {
                previousSessions = value;
                OnPropertyChanged(nameof(PreviousSessions));
            }
        }

        private ShipInfoVM? currentShip;
        public ShipInfoVM? CurrentShip
        {
            get => currentShip;
            set
            {
                currentShip = value;
                OnPropertyChanged(nameof(CurrentShip));
            }
        }

        #region Commands
        public ICommand SelectSession { get; }
        public ICommand OpenPopOut { get; }

        private void OnSelectSession(MiningSessionVM vM)
        {
            SelectedSession = vM;
        }
        private void OnOpenPopOut(Type type)
        {
            popOutService.OpenPopOut(type, settings.SelectedCommanderID);
        }
        #endregion

        private void OnStoreLive(object? sender, bool e)
        {
            if(e)
            {
                CurrentContainer.OnCurrentSessionUpdated(miningData, EventArgs.Empty);
                ProspectorContainer.OnProspectorUpdated(miningData, EventArgs.Empty);
                OnPreviousSessionsUpdated(miningData, EventArgs.Empty);
                OnShipChanged(sharedData, sharedData.CurrentShipInfo);
                OnCargoUpdated(sharedData, sharedData.CurrentShipCargo);
                OnCurrentSessionUpdated(null, EventArgs.Empty);
            }
            OnModelLive(e);
        }

        private void OnShipChanged(object? sender, ShipInfo? e)
        {
            CurrentShip = e == null ? null : new(e);
        }

        private void OnCargoUpdated(object? sender, IEnumerable<ShipCargo>? e)
        {
            CurrentShip?.OnCargoUpdated(e, "Limpet");
        }

        private void OnPreviousSessionsUpdated(object? sender, EventArgs e)
        {
            PreviousSessions.ClearCollection();

            if (miningData.Sessions.Count == 0)
                return;

            var orderedSessions = miningData.Sessions.OrderByDescending(x => x.TimeStarted).Select(x => new MiningSessionVM(x));

            PreviousSessions.AddRange(orderedSessions);

            if (CurrentContainer.CurrentSession == null)
            {
                SelectedSession = PreviousSessions.FirstOrDefault();
            }
        }

        private void OnCurrentSessionUpdated(object? sender, EventArgs e)
        {
            if(CurrentContainer.CurrentSession == null)
            {
                SelectedSession = PreviousSessions.FirstOrDefault();
                return;
            }

            SelectedSession = CurrentContainer.CurrentSession;
        }
    }
}
