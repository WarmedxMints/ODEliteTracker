using ODEliteTracker.Database;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models.Bookmarks;
using ODEliteTracker.ViewModels.ModelViews.Bookmarks;
using ODJournalDatabase.Database.Interfaces;
using ODMVVM.Services.MessageBox;

namespace ODEliteTracker.Stores
{
    public sealed class BookmarkDataStore
    {
        public BookmarkDataStore(IODDatabaseProvider databaseProvider)
        {
             this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;

            _initializeLazy = new Lazy<Task>(Initialise);

            _ = Load();
        }
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private Lazy<Task> _initializeLazy;

        public List<SystemBookmark> Bookmarks { get; private set; } = [];

        public event EventHandler<bool>? StoreLive;
        public event EventHandler? BookmarksUpdated;

        private bool isLive = false;
        public bool IsLive 
        {
            get => isLive;
            private set
            {
                if (isLive != value)
                {
                    isLive = value;
                    StoreLive?.Invoke(this, value);
                }
            }
        }

        public async Task Load()
        {
            try
            {
                await _initializeLazy.Value;
            }
            catch (Exception)
            {
                _initializeLazy = new Lazy<Task>(Initialise);
                throw;
            }
        }

        private async Task Initialise()
        {
            await UpdateBookmarks();

            IsLive = true;
        }

        internal bool ExportBookmarks()
        {
            var bookmarksToSave = databaseProvider.GetBookmarkDTOs();

            if (bookmarksToSave.Count == 0) 
                return false;

            var filename = ODDialogService.SaveFileDialog("Export Bookmarks", "Json File|*.json");

            if (string.IsNullOrEmpty(filename))
                return false;

            var saved = ODMVVM.Helpers.IO.Json.SaveJson(bookmarksToSave, filename);

            return saved;
        }

        internal async Task ImportBookmarks()
        {
            var filename = ODDialogService.OpenFileDialog("Import Bookmarks", "json", "Json File|*.json", 1);

            if (string.IsNullOrEmpty(filename)) 
                return;

            var bookmarks = ODMVVM.Helpers.IO.Json.LoadJson<List<SystemBookmarkDTO>>(filename);

            if (bookmarks == null || bookmarks.Count == 0)
                return;

            await databaseProvider.AddImportedBookmarks(bookmarks);

            await UpdateBookmarks();
        }

        private async Task UpdateBookmarks()
        {
            Bookmarks.Clear();
            var bookmarks = await databaseProvider.GetAllBookmarks();

            if (bookmarks.Count > 0)
            {
                Bookmarks = bookmarks;
            }

            if (IsLive)
            {
                BookmarksUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<int> SaveBookMark(SystemBookmarkVM system, BookmarkVM bookmark)
        {
            var ret = await databaseProvider.AddBookmark(system, bookmark);

            await UpdateBookmarks();

            if (IsLive)
            {
                BookmarksUpdated?.Invoke(this, EventArgs.Empty);
            }
            return ret;
        }

        public async Task DeleteBookmark(long systemAddress, int bookmarkId)
        {
            await databaseProvider.DeleteBookmark(systemAddress, bookmarkId);

            await UpdateBookmarks();

            if (IsLive)
            {
                BookmarksUpdated?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
