namespace ODEliteTracker.Models.Spansh
{
    public abstract record SpanshApiParameters
    {
        public string? JobId { get; set; }
        public abstract SpanshRouteType GetRouteType();
    }
}
