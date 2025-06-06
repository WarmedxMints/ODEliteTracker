using ODEliteTracker.Models.Bookmarks;
using ODEliteTracker.ViewModels.ModelViews.Galaxy;
using ODMVVM.ViewModels;
using System.Globalization;

namespace ODEliteTracker.ViewModels.ModelViews.Bookmarks
{
    public class BookmarkVM : ODObservableObject
    {
        public BookmarkVM(Bookmark bookmark)
        {
            Id = bookmark.Id;
            BodyId = bookmark.BodyId;
            bodyName = bookmark.BodyName;
            bodyNameLocal = bookmark.BodyNameLocal;
            BookmarkName = bookmark.BookMarkName;
            Description = bookmark.Description;
            Latitude = bookmark.Latitude;
            latString = bookmark.Latitude.ToString();
            Longitude = bookmark.Longitude;
            lonString = bookmark.Longitude.ToString();
        }

        public BookmarkVM(SystemBodyVM body, double lon, double lat)
        {
            Id = -1;
            BodyId = body.BodyID;
            bodyName = body.BodyName;
            bodyNameLocal = body.BodyNameLocal;
            Latitude = lat;
            latString = lat.ToString();
            Longitude = lon;
            lonString = lat.ToString();
        }

        public int Id { get; set; }

        private long bodyID;
        public long BodyId
        {
            get => bodyID;
            set
            {
                bodyID = value;
                OnPropertyChanged(nameof(BodyId));
            }
        }

        private string bodyName;
        public string BodyName
        {
            get => bodyName;
            set
            {
                bodyName = value;
                OnPropertyChanged(nameof(BodyName));
            }
        }

        private string bodyNameLocal;
        public string BodyNameLocal
        {
            get => bodyNameLocal;
            set
            {
                bodyNameLocal = value;
                OnPropertyChanged(nameof(BodyNameLocal));
            }

        }

        private string? bookmarkName;
        public string? BookmarkName
        {
            get => bookmarkName;
            set
            {
                bookmarkName = value;
                OnPropertyChanged(nameof(BookmarkName));
            }
        }

        private string? description;
        public string? Description
        {
            get => description;
            set
            {
                description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        private double latitude;
        public double Latitude
        {
            get => latitude;
            set
            {
                latitude = value;
                OnPropertyChanged(nameof(Latitude));
            }
        }

        private string latString;
        public string LatString
        {
            get => latString;
            set
            {
                latString = value;
                if(double.TryParse(value, CultureInfo.InvariantCulture, out var lat))
                {
                    latitude = lat;
                }
                OnPropertyChanged(nameof(LatString));
            }
        }

        private double longitude;
        public double Longitude
        {
            get => longitude;
            set
            {
                longitude = value;
                OnPropertyChanged(nameof(Longitude));
            }
        }

        private string lonString;
        public string LonString
        {
            get => lonString;
            set
            {
                lonString = value;
                if(double.TryParse(value, CultureInfo.InvariantCulture, out var lon))
                {
                    longitude = lon;
                }
                OnPropertyChanged(nameof(LonString));
            }
        }
    }
}
