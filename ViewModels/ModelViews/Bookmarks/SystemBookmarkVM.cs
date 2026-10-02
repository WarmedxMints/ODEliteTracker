using ODEliteTracker.Models.Bookmarks;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODMVVM.ViewModels;
using System.Collections.ObjectModel;

namespace ODEliteTracker.ViewModels.ModelViews.Bookmarks
{
    public sealed class SystemBookmarkVM : ODObservableObject
    {
        public SystemBookmarkVM(SystemBookmark bookmark)
        { 
            Address = bookmark.Address;
            Name = bookmark.Name;
            X = bookmark.X;
            Y = bookmark.Y;
            Z = bookmark.Z;
            notes = bookmark.Notes ?? string.Empty;
            Bookmarks = [.. bookmark.Bookmarks.Select(x => new BookmarkVM(x))];
        }

        public SystemBookmarkVM(StarSystemVM currentSystem)
        {
            Address = currentSystem.Address;
            Name = currentSystem.Name;
            X = currentSystem.Position.X; 
            Y = currentSystem.Position.Y;
            Z = currentSystem.Position.Z;
            notes = string.Empty;
            Bookmarks = [];
        }

        public ulong Address { get; }
        public string Name { get; }
        public double X { get; }
        public string XString => X.ToString("N3");
        public double Y { get; }
        public string YString => Y.ToString("N3");
        public double Z { get; }
        public string ZString => Z.ToString("N3");

        private string notes;
        public string Notes
        {
            get => notes;
            set
            {
                notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        public ObservableCollection<BookmarkVM> Bookmarks { get; }
    }
}
