namespace ODEliteTracker.Models.Spansh
{
    public sealed class BodiesInfo
    {
        public BodiesInfo() { }
        public BodiesInfo(RoadToRichesBody x)
        {
            Body = x.Name;
            Distance = $"{x.DistanceToArrival:N2}";
            Property1 = $"{x.EstimatedMappingValue:N0}";
        }

        public BodiesInfo(ApiWorldTypeBody x, int jumps)
        {
            Body = x.Name;
            Distance = $"{x.DistanceToArrival:N0}";
            Property1 = $"{jumps:N0}";
        }

        public BodiesInfo(RoadToRichesBody x, BodyLandmarks bodyLandmarks, bool v) : this(x)
        {
            Body = v ? x.Name : string.Empty;
            Distance = $"{bodyLandmarks.Subtype}";
            Property1 = $"{bodyLandmarks.Value:N0}";
        }

        public string? Body { get; set; }
        public string? Distance { get; set; }
        public string? Property1 { get; set; }
    }
}
