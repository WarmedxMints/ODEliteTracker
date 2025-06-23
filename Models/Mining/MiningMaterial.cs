namespace ODEliteTracker.Models.Mining
{
    public class MiningMaterial(string name, double proportion)
    {
        public string Name { get; } = name;
        public double Proportion { get; } = proportion;
    }
}