using EliteJournalReader.Events;
using Newtonsoft.Json.Linq;
using ODCompass;
using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Models.Settings;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Bookmarks;
using ODEliteTracker.ViewModels.ModelViews.Compass;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Helpers;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels.PopOuts
{
    public class CompassPopOutViewModel : PopOutViewModel, IWatchStatus
    {
        public CompassPopOutViewModel(IManageJournalEvents journalManager, SharedDataStore sharedData, BookmarkDataStore bookmarkData, EdsmApiService edsmApi, SettingsStore settings)
        {
            this.journalManager = journalManager;
            this.sharedData = sharedData;
            this.bookmarkData = bookmarkData;
            this.edsmApi = edsmApi;
            this.settings = settings;
            this.journalManager.OnStatusFileUpdate += JournalManager_OnStatusFileUpdate;
            this.journalManager.StartStatusWatcher(this);

            this.sharedData.StoreLive += OnStoreLive;
            this.sharedData.CurrentSystemChanged += OnCurrentSystemChanged;

            if (this.sharedData.IsLive)
            {
                OnStoreLive(null, true);
            }

            this.bookmarkData.StoreLive += BookmarkData_StoreLive;
            this.bookmarkData.BookmarksUpdated += BookmarkData_BookmarksUpdated;

            if (bookmarkData.IsLive)
            {
                BookmarkData_StoreLive(null, true);
            }
            ClearTargetCommand = new ODRelayCommand(OnClearTarget, (_) => CurrentTarget != null);
            OpenBookmarksCommand = new ODRelayCommand<Window>(OnOpenBookmarks);
        }

        protected override void Dispose()
        {
            journalManager.OnStatusFileUpdate -= JournalManager_OnStatusFileUpdate;
            journalManager.StopStatusWatcher(this);

            this.sharedData.StoreLive -= OnStoreLive;
            this.sharedData.CurrentSystemChanged -= OnCurrentSystemChanged;

            this.bookmarkData.StoreLive -= BookmarkData_StoreLive;
            this.bookmarkData.BookmarksUpdated -= BookmarkData_BookmarksUpdated;
        }

        private readonly IManageJournalEvents journalManager;
        private readonly SharedDataStore sharedData;
        private readonly BookmarkDataStore bookmarkData;
        private readonly EdsmApiService edsmApi;
        private readonly SettingsStore settings;

        public override string Name => $"Compass";
        public override bool IsLive => sharedData.IsLive;
        public override Uri TitleBarIcon => new("/Assets/Icons/compass.png", UriKind.Relative);

        private CompassSettingsViewModel compassSettings = new();
        public CompassSettingsViewModel CompassSettings
        {
            get => compassSettings;
            set
            {
                compassSettings = value;
                OnPropertyChanged(nameof(CompassSettings));
            }
        }
        private BookmarkViewModel currentTarget = new();
        public BookmarkViewModel CurrentTarget
        {
            get
            {
                return currentTarget;
            }
            set
            {
                currentTarget = value;
                OnPropertyChanged(nameof(CurrentTarget));
            }
        }

        private double heading;
        public double Heading
        {
            get => heading;
            set
            {
                heading = value;
                OnPropertyChanged(nameof(Heading));
            }
        }

        private double? targetHeading;
        public double? TargetHeading
        {
            get => targetHeading;
            set
            {
                targetHeading = value;
                OnPropertyChanged(nameof(TargetHeading));
            }
        }

        private string targetText = string.Empty;
        public string TargetText
        {
            get => targetText;
            set
            {
                targetText = value;
                OnPropertyChanged(nameof(TargetText));
            }
        }

        private bool validLat;
        public bool ValidLat
        {
            get => validLat;
            set
            {
                validLat = value;
                OnPropertyChanged(nameof(ValidLat));
            }
        }

        private string targetLat = string.Empty;
        public string TargetLat
        {
            get => targetLat;
            set
            {
                ValidLat = double.TryParse(value, CultureInfo.InvariantCulture, out var lat ) && lat >= -90 && lat <= 90;

                if (ValidLat)
                {
                    CurrentTarget.Latitude = lat;
                }
                targetLat = value;
                OnPropertyChanged(nameof(TargetLat));
            }
        }

        private double currentLat, currentLon;

        private bool validLon;
        public bool ValidLon
        {
            get => validLon;
            set
            {
                validLon = value;
                OnPropertyChanged(nameof(ValidLon));
            }

        }

        private string targetLon = string.Empty;
        public string TargetLon
        {
            get => targetLon;
            set
            {
                ValidLon = double.TryParse(value, CultureInfo.InvariantCulture, out var lon) && lon <= 180 && lon >= -180;
                if (ValidLon)
                {
                    CurrentTarget.Longitude = lon;
                }
                targetLon = value;
                OnPropertyChanged(nameof(TargetLon));
            }
        }


        public Visibility TargetInfoVis
        {
            get
            {
                return IsMouseOver ? Visibility.Visible :
                    CompassSettings.HideTargetInfo ? Visibility.Hidden : Visibility.Visible;
            }
        }

        private StarSystemVM? currentSystem;
        public StarSystemVM? CurrentSystem
        {
            get => currentSystem;
            set
            {
                currentSystem = value;
                OnPropertyChanged(nameof(CurrentSystem));
            }
        }

        private ObservableCollection<BookmarkVM> currentSystemBookmarks = [];
        public ObservableCollection<BookmarkVM> CurrentSystemBookmarks
        {
            get
            {
                return currentSystemBookmarks;
            }
            set
            {
                currentSystemBookmarks = value;
                OnPropertyChanged(nameof(CurrentSystemBookmarks));
            }
        }

        private BookmarkVM? selectedBookmark;
        public BookmarkVM? SelectedBookmark
        {
            get => selectedBookmark;
            set
            {
                selectedBookmark = value;
                if(selectedBookmark != null)
                {
                    TargetLon = selectedBookmark.LonString;
                    TargetLat = selectedBookmark.LatString;
                }
                OnPropertyChanged(nameof(SelectedBookmark));
            }
        }

        #region Commands
        public ICommand ClearTargetCommand { get; }
        public ICommand OpenBookmarksCommand { get; }

        private void OnClearTarget(object? obj)
        {
            CurrentTarget.Reset();
            TargetLat = string.Empty;
            TargetLon = string.Empty;
            TargetHeading = null;
            TargetText = string.Empty;
        }

        private void OnOpenBookmarks(Window owner)
        {
            if(sharedData.CurrentSystem == null)
            {
                return;
            }

            PauseWindowListener = true;

            EliteTrackerDialogService.ShowBookmarkManager(
                owner, sharedData, bookmarkData, edsmApi, settings, sharedData.CurrentSystem.Address, sharedData.CurrentBody?.BodyID ?? -1,
                currentLat, currentLon);
         
            PauseWindowListener = false;
        }
        #endregion

        internal override void OnResetPosition(object? obj)
        {
            ODWindowPosition.ResetWindowPosition(Position, 900, 300);
        }

        protected override void ParamsUpdated()
        {
            var settings = AdditionalSettings?.ToObject<CompassSettings>();
            CompassSettings.LoadSettings(settings ?? new());

            targetLon = CurrentTarget?.Longitude.ToString("N3") ?? string.Empty;
            targetLat = CurrentTarget?.Latitude.ToString("N3") ?? string.Empty;
            OnPropertyChanged(TargetLon);
            OnPropertyChanged(TargetLat);
        }

        public override void OnMouseEnter_Leave(bool mouseLeave)
        {
            base.OnMouseEnter_Leave(mouseLeave);
            OnPropertyChanged(nameof(TargetInfoVis));
        }

        internal override JObject? GetAdditionalSettings()
        {
            return JObject.FromObject(CompassSettings.GetSettings());
        }

        private void OnStoreLive(object? sender, bool e)
        {
            if (e)
            {
                OnModelLive();
                CompassSettings.SetCompassVis(false, false, false);
                OnCurrentSystemChanged(null, sharedData.CurrentSystem);
            }
        }

        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            if (e == null)
            {
                CurrentSystem = null;
                return;
            }

            CurrentSystem = new StarSystemVM(e);
            BookmarkData_BookmarksUpdated(null, EventArgs.Empty);
        }
        private void BookmarkData_StoreLive(object? sender, bool e)
        {
            if (e)
            {
                BookmarkData_BookmarksUpdated(null, EventArgs.Empty);
            }
        }

        private void BookmarkData_BookmarksUpdated(object? sender, EventArgs e)
        {
            CurrentSystemBookmarks.ClearCollection();
            var systemBookmark = bookmarkData.Bookmarks.FirstOrDefault(x => x.Address == CurrentSystem?.Address);

            if (systemBookmark == null || systemBookmark.Bookmarks.Count == 0)
                return;

            var bookmarks = systemBookmark.Bookmarks.Select(x => new BookmarkVM(x));

            CurrentSystemBookmarks.AddRange(bookmarks);

            if(SelectedBookmark != null)
            {
                SelectedBookmark = CurrentSystemBookmarks.FirstOrDefault(x => x.Id == SelectedBookmark.Id);
                return;
            }

            SelectedBookmark = CurrentSystemBookmarks.FirstOrDefault();
        }

        private void JournalManager_OnStatusFileUpdate(object? sender, StatusFileEvent e)
        {
            var hasLatLong = e.Flags.HasFlag(StatusFlags.HasLatLong);
            var onFoot = e.Flags2.HasFlag(MoreStatusFlags.OnFootOnPlanet);
            var inSrv = e.Flags.HasFlag(StatusFlags.InSRV);
            var inShip = !onFoot && !inSrv;

            CompassSettings.SetCompassSpeed(inShip);
            CompassSettings.SetCompassVis(hasLatLong, onFoot, inSrv);

            if (hasLatLong == false)
            {
                currentLat = 0;
                currentLon = 0;
                Heading = 0;
                TargetHeading = null;
                return;
            }

            currentLat = e.Latitude;
            currentLon = e.Longitude;
            Heading = e.Heading;

            CalcTarget(currentLat, currentLon, e.Altitude, e.PlanetRadius, onFoot);
        }      

        private void CalcTarget(double currentLat, double currentLon, double altitude, double radius, bool onFoot)
        {
            if (currentTarget == null)
                return;

            var distance = altitude < 1000 ? CompassMath.CalculateDistance(currentLat,
                                                                  currentLon,
                                                                  currentTarget.Latitude,
                                                                  currentTarget.Longitude,
                                                                  radius)
                                  : CompassMath.CalculateDistanceWithAltitudes(currentLat,
                                                                               currentLon,
                                                                               altitude,
                                                                               currentTarget.Latitude,
                                                                               currentTarget.Longitude,
                                                                               0,
                                                                               radius);
            
            TargetHeading = CompassMath.CalculateHeading(currentLat, currentLon, currentTarget.Latitude, currentTarget.Longitude);
            TargetText = $"{targetHeading:N0}° {distance.FormatMeters()}";
        }

       
    }
}
