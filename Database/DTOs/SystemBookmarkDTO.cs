using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(Address))]
    public sealed class SystemBookmarkDTO
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Address { get; set; }
        public required string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public string? Notes { get; set; }
        public List<BookMarkDTO> Bookmarks { get; set; } = [];     
    }
}
