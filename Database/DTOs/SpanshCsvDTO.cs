using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database.DTOs
{
    [PrimaryKey(nameof(CsvType), nameof(CommanderID))]
    public record SpanshCsvDTO(int CsvType, int CommanderID, string Json);
}
