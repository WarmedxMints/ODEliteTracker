using ODEliteTracker.Models.Galaxy;
using ODEliteTracker.Services;
using ODEliteTracker.Stores;
using ODEliteTracker.ViewModels.ModelViews.Bookmarks;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODMVVM.Commands;
using ODMVVM.Extensions;
using ODMVVM.Services.MessageBox;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ODEliteTracker.ViewModels
{
    public sealed class BookmarkManagerViewModel : ODObservableObject, IDisposable
    {
        public BookmarkManagerViewModel(SharedDataStore dataStore,
                                        BookmarkDataStore bookmarkData,
                                        EdsmApiService edsmApiService,
                                        SettingsStore settings,
                                        long currentSystemAddress,
                                        long currentBodyId,
                                        double currentLatitude,
                                        double currentLongitude) 
        {
            this.dataStore = dataStore;
            this.bookmarkData = bookmarkData;
            this.edsmApiService = edsmApiService;
            this.settings = settings;
            this.currentSystemAddress = currentSystemAddress;
            this.currentBodyId = currentBodyId;
            this.currentLatitude = currentLatitude;
            this.currentLongitude = currentLongitude;
            this.dataStore.CurrentSystemChanged += OnCurrentSystemChanged;

            this.bookmarkData.StoreLive += BookmarkData_StoreLive;
            this.bookmarkData.BookmarksUpdated += BookmarkData_BookmarksUpdated;

            if (this.dataStore.IsLive && this.dataStore.CurrentSystem != null)
            {
                CurrentSystem = new(this.dataStore.CurrentSystem);               
                SelectedBody = CurrentSystem.Bodies.FirstOrDefault(x => x.BodyID == currentBodyId);

                if(SelectedBody != null)
                {
                    SelectedBookmark = new(SelectedBody, currentLongitude, currentLatitude);
                }
            }

            SaveBookmarkCommand = new ODAsyncRelayCommand(OnSaveBookmark);
            SaveBookmarkAllCommand = new ODAsyncRelayCommand(OnSaveAllBookmark);
            OpenWebsiteCommand = new ODAsyncRelayCommand<Website>(OnOpenWebsite);
            AddBookmarkCommand = new ODAsyncRelayCommand<BookmarkVM>(OnAddBookmark);
            DeleteBookmarkCommand = new ODAsyncRelayCommand(OnDeleteBookmark);
            DeleteBookmarkAllCommand = new ODAsyncRelayCommand(OnDeleteAllBookmark);
            SelectBookmarkCommand = new ODRelayCommand<BookmarkVM>(OnSetSelectedBookmark);
            SelectBookmarkAllCommand = new ODRelayCommand<BookmarkVM>(OnSetSelectedAllBookmark);
            SelectSystemCommand = new ODRelayCommand<SystemBookmarkVM>(OnSelectSystem);
            ImportBookmarks = new ODAsyncRelayCommand(OnImportBookmarks);
            ExportBookmarks = new ODRelayCommand(OnExportBookmarks, (_) => Bookmarks.Count > 0);

            if (this.bookmarkData.IsLive)
                BookmarkData_StoreLive(null, true);
        }

        private readonly SharedDataStore dataStore;
        private readonly BookmarkDataStore bookmarkData;
        private readonly EdsmApiService edsmApiService;
        private readonly SettingsStore settings;
        private readonly long currentSystemAddress;
        private readonly long currentBodyId;
        private readonly double currentLatitude;
        private readonly double currentLongitude;

        public bool IsBusy => bookmarkData.IsLive;

        #region Properties
        public double UiScale
        {
            get => settings.UiScale;
            set
            {
                if (settings.UiScale == value)
                    return;
                settings.UiScale = value;
                OnPropertyChanged(nameof(UiScale));
            }
        }

        private ObservableCollection<SystemBookmarkVM> systemBookmarks = [];
        public ObservableCollection<SystemBookmarkVM> Bookmarks
        {
            get => systemBookmarks;
            set
            {
                systemBookmarks = value;
                OnPropertyChanged(nameof(Bookmarks));
            }
        }

        public IEnumerable<SystemBookmarkVM> FilteredBookmarks => Bookmarks
            .OrderBy(x => x.Name)
            .Where(x => x.Bookmarks.Count > 0 || string.IsNullOrEmpty(x.Notes) == false);

        private SystemBookmarkVM? selectedSystem;
        public SystemBookmarkVM? SelectedSystem
        {
            get => selectedSystem;
            set
            {
                if (selectedSystem == value)
                    return;
                SelectedBookmarkAll = null;
                selectedSystem = value;
                OnPropertyChanged(nameof(SelectedSystem));
            }
        }

        private BookmarkVM? selectedBookmarkAll;
        public BookmarkVM? SelectedBookmarkAll
        {
            get => selectedBookmarkAll;
            set
            {
                selectedBookmarkAll = value;
                OnPropertyChanged(nameof(SelectedBookmarkAll));
            }


        }
        public ObservableCollection<BookmarkVM>? CurrentSystemBookmarks
        {
            get
            {
                if(CurrentSystem == null)
                    return null;

                var known = Bookmarks.FirstOrDefault(x => x.Address == CurrentSystem.Address);

                if (known == null)
                {
                    known = new SystemBookmarkVM(CurrentSystem);
                    Bookmarks.Add(known);
                }
                
                CurrentBookmark = known;
                return known.Bookmarks;
            } 
        }

        private SystemBookmarkVM? currentBookmark;
        public SystemBookmarkVM? CurrentBookmark
        {
            get => currentBookmark;
            set
            {
                currentBookmark = value;
                OnPropertyChanged(nameof(CurrentBookmark));
            }
        }

        private BookmarkVM? selectedBookmark;
        public BookmarkVM? SelectedBookmark
        {
            get => selectedBookmark;
            set
            {
                selectedBookmark = value;
                OnPropertyChanged(nameof(SelectedBookmark));
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

        private SystemBodyVM? selectedBody;
        public SystemBodyVM? SelectedBody
        {
            get => selectedBody;
            set
            {
                selectedBody = value;

                if (value!= null)
                {
                    SelectedBookmark ??= new BookmarkVM(value, currentLongitude, currentLatitude);
                    SelectedBookmark.BodyId = value.BodyID;
                    SelectedBookmark.BodyName = value.BodyName;
                    SelectedBookmark.BodyNameLocal = value.BodyNameLocal;
                }
                OnPropertyChanged(nameof(SelectedBody));
            }
        }
        #endregion

        #region Commands
        public ICommand SaveBookmarkCommand { get; }
        public ICommand SaveBookmarkAllCommand { get; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand AddBookmarkCommand { get; }
        public ICommand DeleteBookmarkCommand { get; }
        public ICommand DeleteBookmarkAllCommand { get; }
        public ICommand SelectBookmarkCommand { get; }
        public ICommand SelectBookmarkAllCommand { get; }
        public ICommand SelectSystemCommand { get; }
        public ICommand ImportBookmarks { get; }
        public ICommand ExportBookmarks { get; }

        private void OnSetSelectedBookmark(BookmarkVM bookmark)
        {
            if (bookmark != null)
            {
                SelectedBookmark = bookmark;
                SelectedBody = CurrentSystem?.Bodies.FirstOrDefault(x => x.BodyID == bookmark.BodyId);
            }
        }

        private void OnSetSelectedAllBookmark(BookmarkVM vM)
        {
            if (vM != null)
            {
                SelectedBookmarkAll = vM;
            }
        }
        private void OnSelectSystem(SystemBookmarkVM vM)
        {
            if(vM != null)
            {
                SelectedSystem = vM;
            }
        }

        private async Task OnSaveBookmark()
        {
            if (selectedBookmark == null || CurrentSystem == null)
                return;

            var bookmarkSystem = Bookmarks.FirstOrDefault(x => x.Address == CurrentSystem.Address);

            bookmarkSystem ??= new(CurrentSystem);

            await SaveBookmark(bookmarkSystem, selectedBookmark);
        }

        private async Task OnSaveAllBookmark()
        {
            if (selectedBookmarkAll == null || selectedSystem == null)
                return;

            await SaveBookmark(selectedSystem, selectedBookmarkAll);
        }

        private async Task SaveBookmark(SystemBookmarkVM system, BookmarkVM bookmark)
        {
            var ret = await bookmarkData.SaveBookMark(system, bookmark);

            bookmark.Id = ret;
        }

        private async Task OnOpenWebsite(Website website)
        {
            if (CurrentSystem == null)
                return;

            switch (website)
            {
                case Website.Inara:
                    WebsiteService.OpenInara(CurrentSystem.Name);
                    break;
                case Website.Spansh:
                    WebsiteService.OpenSpansh(CurrentSystem.Address);
                    break;
                case Website.Edsm:
                    if (string.IsNullOrEmpty(CurrentSystem.EdsmUrl))
                    {
                        var ret = await edsmApiService.GetSystemUrlAsync(CurrentSystem.Address);

                        if (ret != null)
                        {
                            CurrentSystem.EdsmUrl = ret;
                            WebsiteService.OpenEdsm(ret);
                            return;
                        }
                        return;
                    }
                    WebsiteService.OpenEdsm(CurrentSystem.EdsmUrl);
                    break;
            }
        }

        private async Task OnAddBookmark(BookmarkVM vM)
        {
            if (SelectedSystem == null || SelectedBookmark == null)
                return;

            vM.Id = -1;

            var ret = await bookmarkData.SaveBookMark(SelectedSystem, vM);

            vM.Id = ret;
        }

        private async Task OnDeleteAllBookmark()
        {
            if (selectedBookmarkAll == null || selectedSystem == null)
                return;

            await DeleteBookmark(selectedSystem, selectedBookmarkAll);
        }

        private async Task OnDeleteBookmark()
        {
            if (CurrentSystem == null || SelectedBookmark == null)
                return;

            var known = Bookmarks.FirstOrDefault(x => x.Address == CurrentSystem.Address);

            if (known == null)
                return;

            await DeleteBookmark(known, SelectedBookmark);
        }

        private async Task DeleteBookmark(SystemBookmarkVM system, BookmarkVM bookmark)
        {
            if (bookmark.Id < 0)
            {
                bookmark.Latitude = currentLatitude;
                bookmark.Longitude = currentLongitude;
                bookmark.BookmarkName = string.Empty;
                bookmark.Description = string.Empty;
                return;
            }

            var result = ODDialogService.ShowWithOwner(null, "Delete?", $"Delete Bookmark {bookmark.BookmarkName}?", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                await bookmarkData.DeleteBookmark(system.Address, bookmark.Id);
            }
        }
        private async Task OnImportBookmarks()
        {
            await bookmarkData.ImportBookmarks();
        }

        private void OnExportBookmarks(object? obj)
        {
            _= bookmarkData.ExportBookmarks();
        }

  
        #endregion

        #region Event Methods
        private void OnCurrentSystemChanged(object? sender, StarSystem? e)
        {
            if(e == null)
            {
                CurrentSystem = null;
                return;
            }

            CurrentSystem = new StarSystemVM(e);
            OnPropertyChanged(nameof(CurrentSystemBookmarks));
        }

        private void BookmarkData_StoreLive(object? sender, bool e)
        {
            if(e)
            {
                BookmarkData_BookmarksUpdated(null, EventArgs.Empty);
            }

            OnPropertyChanged(nameof(IsBusy));
            OnPropertyChanged(nameof(CurrentSystemBookmarks));
        }

        private void BookmarkData_BookmarksUpdated(object? sender, EventArgs e)
        {
            Bookmarks.ClearCollection();
            Bookmarks.AddRange(bookmarkData.Bookmarks.Select(x => new SystemBookmarkVM(x)));

            var system = Bookmarks.FirstOrDefault(system => system.Address == CurrentSystem?.Address);
            SelectedSystem = Bookmarks.FirstOrDefault(x => x.Address == SelectedSystem?.Address);

            SelectedSystem ??= Bookmarks.FirstOrDefault();

            var bookmark = system?.Bookmarks.FirstOrDefault(x => x.Id == SelectedBookmark?.Id);

            if (bookmark != null)
            {
                SelectedBookmark = bookmark;
                OnPropertyChanged(nameof(CurrentSystemBookmarks));
                return;
            }

            if (system == null || CurrentSystem == null || CurrentSystem.Address != currentSystemAddress)
            {
                OnPropertyChanged(nameof(CurrentSystemBookmarks));
                return;
            }

            var body = CurrentSystem.Bodies.FirstOrDefault(x => x.BodyID == currentBodyId);

            if (body == null)
            {
                OnPropertyChanged(nameof(CurrentSystemBookmarks));
                return;
            }

            bookmark = new BookmarkVM(body, currentLongitude, currentLatitude);

            SelectedBookmark = bookmark;
            OnPropertyChanged(nameof(CurrentSystemBookmarks));
        }
        #endregion

        public void Dispose()
        {
            this.dataStore.CurrentSystemChanged -= OnCurrentSystemChanged;
            this.bookmarkData.StoreLive -= BookmarkData_StoreLive;
            this.bookmarkData.BookmarksUpdated -= BookmarkData_BookmarksUpdated;
        }
    }
}
