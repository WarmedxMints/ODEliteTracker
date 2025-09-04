using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(SystemAddress))]
    public record BGSIgnoredSystemsDTO(long SystemAddress, string SystemName);
}
