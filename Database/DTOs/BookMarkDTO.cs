using System.ComponentModel.DataAnnotations.Schema;

namespace ODEliteTracker.Database.DTOs
{
    public sealed class BookMarkDTO
    {
        public int Id { get; set; }
        public uint BodyId { get; set; }
        public required string BodyName { get; set; }
        public required string BodyNameLocal { get; set; }
        public string? BookmarkName { get; set; }
        public string? Description { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public ulong SystemAddress { get; set; }
    }
}
