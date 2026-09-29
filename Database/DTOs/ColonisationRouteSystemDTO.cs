using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(CommanderId), nameof(SystemAddress))]
    public record ColonisationRouteSystemDTO(int CommanderId, long SystemAddress, int HopNumber, string SystemName, double X, double Y, double Z, string StarClass, bool Built, bool Hub, string Notes);
}
