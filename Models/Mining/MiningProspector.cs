namespace ODEliteTracker.Models.Mining
{
    public enum MiningContent
    {
        Low,
        Medium,
        High
    }

    public record MiningProspector(List<MiningMaterial> Materials, MiningContent Content, string? MotherlodeMaterial, double Remaining);
}
