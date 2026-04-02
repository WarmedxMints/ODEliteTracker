using ODEliteTracker.Database.DTOs;

namespace ODEliteTracker.ViewModels.ModelViews.BGS
{
    public sealed class TickDataVM(BGSTickData data)
    {
        public string ID { get; } = data.Id;
        public DateTime TickTime => data.Time;
        public string Time { get; } = data.Time.AddYears(1286).ToString("dd MMM yyyy HH:mm");
        public string LocalTime { get; } = $"{data.Time.ToLocalTime():dd MMM yyyy HH:mm} Local";
        public bool ManualTick { get; } = data.ManualTick;

        public override string ToString()
        {
            return Time;
        }
    }
}
