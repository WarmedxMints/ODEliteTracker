using ODEliteTracker.Models.Spansh;

namespace ODEliteTracker.ViewModels.ModelViews.Spansh
{
    public sealed class BodiesInfoViewModel(BodiesInfo info)
    {
        public BodiesInfo Info { get; } = info;
        public string? Body => Info.Body;
        public string? Distance => Info.Distance;
        public string? Property1 => Info.Property1;
    }
}
