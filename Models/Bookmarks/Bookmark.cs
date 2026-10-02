using ODEliteTracker.Database.DTOs;

namespace ODEliteTracker.Models.Bookmarks
{
    public sealed class Bookmark(BookMarkDTO dto)
    {
        public int Id { get; } = dto.Id;
        public uint BodyId { get; } = dto.BodyId;
        public string BodyName { get; } = dto.BodyName;
        public string BodyNameLocal { get; } = dto.BodyNameLocal;
        public string? BookMarkName { get; } = dto.BookmarkName;
        public string? Description { get; } = dto.Description;
        public double Latitude { get; } = dto.Latitude;
        public double Longitude { get; } = dto.Longitude;
    }
}
