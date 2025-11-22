using Microsoft.EntityFrameworkCore;
using ODEliteTracker.ViewModels.ModelViews.Colonisation;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(CommanderID), nameof(Build))]
    public sealed class ColonisationBuildWishlistItemDTO
    {
        public int CommanderID { get; set; }
        public ColonisationBuild Build { get; set; }
        public int Count { get; set; }
    }
}
