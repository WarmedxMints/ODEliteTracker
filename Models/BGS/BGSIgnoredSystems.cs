namespace ODEliteTracker.Models.BGS
{
    public sealed class BGSIgnoredSystems(ulong address, string name)
    {
        public ulong Address { get; set; } = address;
        public string Name { get; set; } = name;
    }
}
