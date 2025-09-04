namespace ODEliteTracker.Models.BGS
{
    public sealed class BGSIgnoredSystems(long address, string name)
    {
        public long Address { get; set; } = address;
        public string Name { get; set; } = name;
    }
}
