using ODEliteTracker.Database.DTOs;

namespace ODEliteTracker.Models.Bookmarks
{
    public sealed class SystemBookmark
    {
        public SystemBookmark(SystemBookmarkDTO bookmarkDTO)
        { 
            Address = bookmarkDTO.Address;
            Name = bookmarkDTO.Name;
            X = bookmarkDTO.X;
            Y = bookmarkDTO.Y;
            Z = bookmarkDTO.Z;
            Notes = bookmarkDTO.Notes;
            Bookmarks = [.. bookmarkDTO.Bookmarks.Select(x => new Bookmark(x))];
        }

        public ulong Address { get; }
        public string Name { get; }
        public double X { get; }
        public double Y { get; }
        public double Z { get; }
        public string? Notes { get; }
        public List<Bookmark> Bookmarks { get; } 
    }
}
