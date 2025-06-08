using EliteJournalReader;
using EliteJournalReader.Events;
using ODEliteTracker.Extensions;
using ODEliteTracker.Models.Galaxy;

namespace ODEliteTracker.Models.BGS
{
    public sealed class BGSStarSystem : StarSystem
    {
        public BGSStarSystem(LocationEvent.LocationEventArgs evt) : base(evt)
        {
            TickData = [new(evt)];
            if (evt.Conflicts != null && evt.Conflicts.Any())
                AddConflicts(evt.Conflicts, evt.Timestamp);
        }

        public BGSStarSystem(FSDJumpEvent.FSDJumpEventArgs evt) : base(evt)
        {
            TickData = [new(evt)];
            VisitCount = 1;
            if (evt.Conflicts != null && evt.Conflicts.Any())
                AddConflicts(evt.Conflicts, evt.Timestamp);
        }

        public BGSStarSystem(CarrierJumpEvent.CarrierJumpEventArgs evt) : base(evt)
        {
            TickData = [new(evt)];
            VisitCount = 1;
            if (evt.Conflicts != null && evt.Conflicts.Any())
                AddConflicts(evt.Conflicts, evt.Timestamp);
        }

        public int VisitCount { get; private set; }
        public List<SystemTickData> TickData { get; }
        public List<VoucherClaim> VoucherClaims { get; } = [];
        public List<TradeTransaction> Transactions { get; } = [];
        public List<SystemCrime> Crimes { get; } = [];
        public List<ExplorationData> CartoData { get; } = [];
        public List<SearchAndRescue> SearchAndRescueData { get; } = [];
        public List<SystemConflict> Conflicts { get;} = [];
        public List<SystemWarZone> Wars { get; } = [];

        public void AddTickData(BGSStarSystem sys, DateTime eventTime)
        {
            VisitCount += sys.VisitCount;

            if (sys.Conflicts.Count != 0)
                AddConflicts(sys.Conflicts, eventTime);

            var data = sys.TickData.First();

            var newData = TickData.Last().NewData(data);

            if (newData == false)
                return;
        
            TickData.Add(data);
        }

        public void AddConflicts(IEnumerable<Conflict> conflicts, DateTime eventTime)
        {
            foreach(var conflict in conflicts)
            {
                var known = Conflicts.FirstOrDefault(x => x.Conflict.Equals(conflict));

                if(known != null)
                {
                    known.EventTimes.Add(eventTime);
                    continue;
                }

                Conflicts.Add(new(conflict, eventTime));
            }
        }

        public void AddConflicts(IEnumerable<SystemConflict> conflicts, DateTime eventTime)
        {
            foreach (var conflict in conflicts)
            {
                var known = Conflicts.FirstOrDefault(x => x.Conflict.Equals(conflict.Conflict));

                if (known != null)
                {
                    known.EventTimes.Add(eventTime);
                    continue;
                }

                Conflicts.Add(conflict);
            }
        }

        public void AddWar(SystemWarZone war)
        {
            Wars.Add(war);
        }

        public BGSTickSystem? GetBGSTickSystem(TickData data)
        {
            var tickData = TickData.Where(x => x.VisitedDuringPeriod(data.From, data.To))
                .OrderBy(x => x.VisitedTimes.LatestTime()).LastOrDefault();

            if (tickData == null) 
                return null;

            var claims = VoucherClaims.Where(x => data.TimeWithinTick(x.Timestamp));
            var transactions = Transactions.Where(x => data.TimeWithinTick(x.TransactionTime));
            var crimes = Crimes.Where(x => data.TimeWithinTick(x.EventTime));
            var carto = CartoData.Where(x => data.TimeWithinTick(x.EventTime));
            var s_r = SearchAndRescueData.Where(x => data.TimeWithinTick(x.EventTime));
            var conflicts = Conflicts
                .Where(x => data.TimeWithinTick(x.EventTimes))
                .GroupBy(x => x.Hash)
                .ToDictionary(x => x.Key, x => x)
                .Select(x => x.Value.OrderBy(x => x.EventTimes.LatestTime()).Last());
            var wars = Wars.Where(x => data.TimeWithinTick(x.TimeCompleted));

            return new BGSTickSystem(this,  tickData, claims, transactions, crimes, carto, s_r, conflicts, wars);
        }
    }
}
