using EliteJournalReader.Events;
using ODEliteTracker.Models.BGS;

namespace ODEliteTracker.Models.Missions
{
    public sealed class BGSMission(MissionAcceptedEvent.MissionAcceptedEventArgs args,
                                   ulong originAddress,
                                   string originSystemName,
                                   ulong originMarketID,
                                   string originStationName,
                                   bool odyssey) : MissionBase(args, 
                                                               originAddress,
                                                               originSystemName,
                                                               originMarketID,
                                                               originStationName,
                                                               odyssey)
    {
        public List<FactionEffects>? FactionEffects { get; private set; }

        internal void ApplyFactionEffects(IReadOnlyList<MissionCompletedEvent.MissionCompletedEventArgs.FactionEffectsDesc> factionEffects)
        {
            FactionEffects = [];

            foreach (var factionEffect in factionEffects)
            {
                if (string.IsNullOrEmpty(factionEffect.Faction))
                    continue;

                var effect = new FactionEffects(factionEffect);
                FactionEffects.Add(effect);
            }
        }

        internal bool CompletedDuringTimeframe(TickData data)
        {
            return CompletionTime > data.From && CompletionTime < data.To;
        }
    }
}
