namespace ODEliteTracker.Models.Spansh
{
    public sealed class ExplorationTarget
    {
        public string SystemName { get; set; } = "No Data";
        public string? Property1 { get; set; }
        public string? Property2 { get; set; }
        public string? Property3 { get; set; }
        public string? Property4 { get; set; }
        public List<BodiesInfo>? BodiesInfo { get; set; }
    }
}
