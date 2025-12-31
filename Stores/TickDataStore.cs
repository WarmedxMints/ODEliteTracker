using NLog.LayoutRenderers;
using ODEliteTracker.Database;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models.BGS;
using ODEliteTracker.Services;
using ODJournalDatabase.Database.Interfaces;

namespace ODEliteTracker.Stores
{
    public enum TickSource
    {
        TichInformancer,
        EliteBGS
    }

    public sealed class TickDataStore
    {
        public TickDataStore(IODDatabaseProvider databaseProvider, SettingsStore settings, EliteBGSApiService eliteBGS, TickInformacerApiService tickInformacerApi) 
        {
            this.databaseProvider = (ODEliteTrackerDatabaseProvider)databaseProvider;
            this.settings = settings;
            this.eliteBGS = eliteBGS;
            this.tickInformacerApi = tickInformacerApi;
        }

        private List<BGSTickData> tickData = [];
        private readonly ODEliteTrackerDatabaseProvider databaseProvider;
        private readonly SettingsStore settings;
        private readonly EliteBGSApiService eliteBGS;
        private readonly TickInformacerApiService tickInformacerApi;

        public List<BGSTickData> BGSTickData => tickData;
        public TickSource TickSource { get; private set; } = TickSource.TichInformancer;
        public EventHandler? NewTick;

        public async Task Initialise()
        {
            await CheckForNewTick(false);
        }

        public async Task CheckForNewTick(bool fireEvent = true)
        {
            bool newTick = false;

            switch (TickSource)
            {
                case TickSource.TichInformancer:
                    newTick = await CheckForNewTickInformancer();
                    break;
                case TickSource.EliteBGS:
                    newTick = await CheckForNewTicksEliteBGS();
                    break;
            }

            if (newTick && fireEvent)
                NewTick?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateTickFromDatabase()
        {
            tickData = await databaseProvider.GetTickData(settings.JournalAgeDateTime.AddDays(-7));
        }

        private async Task<bool> CheckForNewTickInformancer()
        {
            await UpdateTickFromDatabase();

            var newDate = await tickInformacerApi.GetLastestTick();

            var newTickData = new BGSTickData(newDate.Ticks.ToString(), newDate, DateTime.UtcNow);

            var newTick = await databaseProvider.TryAddTick(newTickData);

            if (newTick)
            {
                tickData = await databaseProvider.GetTickData(settings.JournalAgeDateTime.AddDays(-7));
                return true;
            }

            return false;
        }

        private async Task<bool> CheckForNewTicksEliteBGS()
        {
            var min = 0L;

            await UpdateTickFromDatabase();

            if (tickData.Count != 0)
            {
                //Get Unix Milliseconds and add a minute so we don't get the same tick again
                min = ((DateTimeOffset)tickData.First().Updated_At.AddMinutes(1)).ToUnixTimeMilliseconds();
            }

            var newTicks = await GetLatestTickHistory(min).ConfigureAwait(true);

            if (newTicks is null || newTicks.Count == 0)
                return false;

            var newTickData = newTicks.Except(tickData);

            if (newTickData.Any())
            {
                await databaseProvider.AddTickData(newTickData);
                tickData = await databaseProvider.GetTickData(settings.JournalAgeDateTime.AddDays(-7));
                return true;
            }

            return false;
        }

        private async Task<List<BGSTickData>?> GetLatestTickHistory(long min)
        {
            if(min == 0)
            {
                min = DateTimeOffset.Parse("2018-01-01T00:00:00.000Z").ToUnixTimeMilliseconds();
            }

            long max = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var data = await eliteBGS.GetTicks(min, max);

            return data;
        }

        internal async Task<BGSTickData> AddTick(DateTime dateTime)
        {
            var newTick = new BGSTickData(DateTime.UtcNow.Ticks.ToString(), dateTime, dateTime, true);

            await databaseProvider.AddTickData([newTick]).ConfigureAwait(true);
            tickData = await databaseProvider.GetTickData(settings.JournalAgeDateTime.AddDays(-7)).ConfigureAwait(true);

            return newTick;
        }

        internal async Task DeleteTick(string iD)
        {
            await databaseProvider.DeleteTickData(iD).ConfigureAwait(true);
            await UpdateTickFromDatabase();
        }
    }
}
