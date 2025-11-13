namespace ODEliteTracker.Models.Settings
{
    public sealed class FleetCarrierSettings
    {
        public bool PlayOnPersonalCarrierCooldown { get; set; } = true;
        public bool PlayOnSquadronCarrierCooldown { get; set; } = true;
        public string? TimerAudioFile { get; set; }
        public float Volume { get; set; } = 1f;
    }
}
