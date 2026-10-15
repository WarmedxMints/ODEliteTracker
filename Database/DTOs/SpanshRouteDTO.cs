using Microsoft.EntityFrameworkCore;
using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(CommanderID), nameof(CreatedDate))]
    public record SpanshRouteDTO(int CommanderID,
                                 DateTime CreatedDate,
                                 DateTime LastUsed,
                                 SpanshRouteFormat Format,
                                 SpanshRouteType RouteType,
                                 string Json);
}
