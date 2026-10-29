using EliteJournalReader.Events;
using NLog;
using ODCapi.Models;
using ODCapi.Services;
using ODEliteTracker.Stores;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Services
{
    public sealed class JournalManager : IManageJournalEvents
    {
        #region ctor
        public JournalManager(JournalEventParser eventParser,
                              SettingsStore settingsStore,
                              IODDatabaseProvider oDDatabase,
                              CAPIService cAPIService)
        {
            //Event parser
            this.eventParser = eventParser;
            this.settingsStore = settingsStore;
            this.oDDatabase = oDDatabase;
            this.capiService = cAPIService;
            this.eventParser.OnJournalEventReceived += EventParser_OnJournalEventReceived;
            this.eventParser.LiveStatusChange += async (sender, e) => await EventParser_LiveStatusChange(sender, e);

            this.capiService.ErrorException += OnCAPIErrorException;
        }
        #endregion

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly JournalEventParser eventParser;
        private readonly SettingsStore settingsStore;
        private readonly IODDatabaseProvider oDDatabase;
        private readonly CAPIService capiService;
        private readonly List<IProcessJournalLogs> journalLogParserList = [];
        private DateTime lastCAPICall = DateTime.MinValue;
        private readonly List<IWatchStatus> statusWatchers = [];
        private bool managerLive { get; set; }
        private bool Processing { get; set; }
        public bool CanCallCAPI
        {
            get
            {
#if DEBUG
                return true;
#else
                return capiService.CurrentState == ODCapi.Services.State.Authorised && DateTime.UtcNow - lastCAPICall > TimeSpan.FromMinutes(10);
#endif
            }
        }

        public List<JournalCommander> Commanders { get; private set; } = [];

        private JournalCommander? selectedCommander;
        public JournalCommander? SelectedCommander
        {
            get => selectedCommander;
            private set
            {
                selectedCommander = value;
                if (selectedCommander != null)
                    OnCommanderChanged?.Invoke(this, selectedCommander);
            }
        }

        public event EventHandler<string>? CAPIErrorMessage
        {
            add { capiService.ErrorString += value; }
            remove { capiService.ErrorString -= value; }
        }

        public EventHandler<JournalCommander>? OnCommanderChanged;

        public bool CAPIIsLive => capiService.IsLive;

        public event EventHandler? CommandersUpdated;

        public event EventHandler<StatusFileEvent>? OnStatusFileUpdate
        {
            add { eventParser.StatusUpdated += value; }
            remove { eventParser.StatusUpdated -= value; }
        }

        public event EventHandler<bool>? CAPILive
        {
            add => capiService.CAPILive += value;
            remove => capiService.CAPILive -= value;
        }

        public event EventHandler<bool>? ManagerLive;
        private readonly HashSet<Tuple<string, long>> prossedEntries = [];

        public bool IsLive
        {
            get => managerLive;
            set
            {
                if (managerLive == value)
                    return;

                managerLive = value;
                ManagerLive?.Invoke(this, value);
            }
        }

        #region Event Parser Methods
        private void EventParser_OnJournalEventReceived(object? sender, JournalEntry e)
        {
            if (e.CommanderID != settingsStore.SelectedCommanderID)
            {
                return;
            }

            var entryTuple = Tuple.Create(e.Filename, e.Offset);

            if (prossedEntries.Contains(entryTuple))
            {
                return;
            }

            prossedEntries.Add(entryTuple);

            foreach (var parser in journalLogParserList)
            {
                parser.ParseJournalEvent(e);
            }
        }

        private async Task EventParser_LiveStatusChange(object? sender, bool e)
        {
            if (e)
            {
                Processing = true;
                if (settingsStore.SelectedCommanderID <= 0)
                {
                    Commanders = await oDDatabase.GetAllJournalCommanders();
                    SelectedCommander = Commanders.FirstOrDefault(x => x.Id == eventParser.JournalCMDR?.Id) ?? Commanders.FirstOrDefault();
                    if (SelectedCommander == null)
                        return;

                    settingsStore.SelectedCommanderID = SelectedCommander.Id;
                }

                //Make a copy as when switching views during load it will add to this list
                var currentParsers = journalLogParserList.ToArray();

                foreach (var parser in currentParsers)
                {
                    parser.ClearData();
                    parser.RunBeforeParsingHistory(settingsStore.SelectedCommanderID);
                }

                var history = journalLogParserList.Where(x => x.EventsToParse.Count != 0).ToList();
                if (SelectedCommander?.UseCAPI == true && string.IsNullOrEmpty(SelectedCommander.Name) == false)
                    _ = Task.Run(() => capiService.Login(SelectedCommander.Name, SelectedCommander.IsLegacy).ConfigureAwait(true));

                await eventParser.StreamJournalHistoryOfTypeAsync(settingsStore.SelectedCommanderID, history, settingsStore.JournalAgeDateTime).ConfigureAwait(true);
                IsLive = true;
                Processing = false;

                foreach (var parser in currentParsers)
                {
                    parser.IsLive = true;
                }
            }
        }
        #endregion

        #region IManageJournalEvents Implementation    

        public async Task RegisterLogProcessor(IProcessJournalLogs logProcesser)
        {
            journalLogParserList.Add(logProcesser);
            while (Processing)
            {
                await Task.Delay(100);
            }

            if (IsLive)
            {
                logProcesser.ClearData();
                logProcesser.RunBeforeParsingHistory(settingsStore.SelectedCommanderID);

                await Task.Run(async () => await eventParser.StreamJournalHistoryOfTypeAsync(settingsStore.SelectedCommanderID, [logProcesser], settingsStore.JournalAgeDateTime)).ConfigureAwait(true);

                logProcesser.IsLive = true;
            }
        }

        public void UnregisterLogProcessor(IProcessJournalLogs logProcesser)
        {
            journalLogParserList.Remove(logProcesser);
        }

        public async Task<bool> Initialise()
        {
            IsLive = false;
            Commanders = await oDDatabase.GetAllJournalCommanders();
            return await ReadSelectedCommander();
        }

        public async Task ChangeCommander()
        {
            IsLive = false;

            foreach (var parser in journalLogParserList)
            {
                parser.SaveData();
                parser.ClearData();
            }
            lastCAPICall = DateTime.MinValue;
            await ReadSelectedCommander();
        }

        public async Task ReadNewDirectory(string path)
        {
            IsLive = false;
            var commander = new JournalCommander(-1, "Reading History", path, "", false, false, -1);

            await eventParser.StartWatchingAsync(commander, false).ConfigureAwait(true);
            await UpdateCommanders();
        }

        public async Task<CapiMarket?> GetCAPIMarket()
        {
            if (capiService.IsLive == false || selectedCommander == null)
                return null;
            //await Task.Delay(1000);
            var market = await capiService.GetMarketAsync(selectedCommander.Name);

            return market;
        }


        public NavigationRoute? ReadNavRouteJson(int commanderId)
        {
            if (managerLive == false)
                return null;

            return eventParser.ReadNavRouteJson(commanderId);
        }

        public MarketInfo? GetMarketInfo()
        {
            if (managerLive == false)
                return null;

            return eventParser.GetMarketInfo(settingsStore.SelectedCommanderID);
        }
        public async Task<CapiMarket?> GetCapiMarketInfo()
        {
            if (selectedCommander == null || string.IsNullOrEmpty(selectedCommander.Name))
                return null;

            var ret = await capiService.GetMarketAsync(selectedCommander.Name);

            return ret;
        }
        public CargoEvent.CargoEventArgs? GetCargo()
        {
            if (managerLive == false)
                return null;

            return eventParser.GetCargo(settingsStore.SelectedCommanderID);
        }

        public async Task UpdateCommanders()
        {
            var commanders = await oDDatabase.GetAllJournalCommanders().ConfigureAwait(true);
            Commanders.Clear();
            Commanders.AddRange(commanders);

            //If we haven't found any commanders yet, set the first one
            if (settingsStore.SelectedCommanderID <= 0 && Commanders.Count != 0)
            {
                var ret = Commanders.FirstOrDefault();
                settingsStore.SelectedCommanderID = ret?.Id ?? 0;
            }
            CommandersUpdated?.Invoke(this, EventArgs.Empty);
        }

        public async Task ResetDatabase()
        {
            IsLive = false;
            eventParser.StopWatcher();
            Commanders.Clear();
            CommandersUpdated?.Invoke(this, EventArgs.Empty);
            await Task.Factory.StartNew(oDDatabase.ResetDatabaseAsync);
        }

        public bool StartStatusWatcher(IWatchStatus watcher)
        {
            if (selectedCommander == null)
                return false;

            if (statusWatchers.Contains(watcher) == false)
                statusWatchers.Add(watcher);
            return eventParser.StartStatusWatcher();
        }

        public void StopStatusWatcher(IWatchStatus watcher)
        {
            statusWatchers.Remove(watcher);
            if (statusWatchers.Count == 0)
                eventParser.StopStatusWatcher();
        }

        public void Shutdown()
        {
            foreach (var parser in journalLogParserList)
            {
                parser.SaveData();
            }
            eventParser.StopWatcher();
        }
        #endregion

        public async Task<bool> ReadSelectedCommander()
        {
            SelectedCommander = Commanders.FirstOrDefault(cmdr => cmdr.Id == settingsStore.SelectedCommanderID)
                ?? new(-1, string.Empty, string.Empty, string.Empty, false, false, -1);

            return await eventParser.StartWatchingAsync(SelectedCommander, false).ConfigureAwait(true);
        }

        internal async Task<CAPIFleetCarrier?> GetCAPICarrier()
        {
            if (selectedCommander == null
                || selectedCommander.UseCAPI == false
                || string.IsNullOrEmpty(selectedCommander.Name)
                || CanCallCAPI == false)
            {
                return null;
            }

            var ret = await capiService.GetCarrier(selectedCommander.Name);

            if (ret != null)
            {
                lastCAPICall = DateTime.UtcNow;
            }

            return ret;
        }

        internal async Task<CAPISquadron?> GetCAPISquadron()
        {
            if (selectedCommander == null
                || selectedCommander.UseCAPI == false
                || string.IsNullOrEmpty(selectedCommander.Name))
            {
                return null;
            }

            var ret = await capiService.GetSquadron(selectedCommander.Name);

            return ret;
        }

        private void OnCAPIErrorException(object? sender, Exception e)
        {
            Logger.Error(e);
        }
    }
}
