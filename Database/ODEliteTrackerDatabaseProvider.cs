using EFCore.BulkExtensions;
using EliteJournalReader;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ODEliteTracker.Database.DTOs;
using ODEliteTracker.Models;
using ODEliteTracker.Models.Bookmarks;
using ODEliteTracker.Models.Spansh;
using ODEliteTracker.ViewModels.ModelViews.Bookmarks;
using ODJournalDatabase.Database.DTOs;
using ODJournalDatabase.Database.Interfaces;
using ODJournalDatabase.JournalManagement;

namespace ODEliteTracker.Database
{
    public sealed class ODEliteTrackerDatabaseProvider(ODEliteTrackerDbContextFactory contextFactory) : IODDatabaseProvider
    {
        private readonly ODEliteTrackerDbContextFactory _contextFactory = contextFactory;

        #region Commander Methods
        public async Task<List<JournalCommander>> GetAllJournalCommanders(bool includeHidden = false)
        {
            using var context = _contextFactory.CreateDbContext();

            if (!context.JournalCommanders.Any())
                return [];

            if (includeHidden)
            {
                var allCmdrs = await context.JournalCommanders
                    .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden, x.UseCAPI))
                    .ToListAsync();

                var reslt = allCmdrs.OrderBy(x => x.Name.Contains("(Legacy)"))
                                    .ThenBy(x => x.Name);
                return [.. reslt];
            }

            var cmdrs = await context.JournalCommanders
                .Where(x => x.IsHidden == false)
                .Select(x => new JournalCommander(x.Id, x.Name, x.JournalDir, x.LastFile, x.IsHidden, x.UseCAPI))
                .ToListAsync();

            var ret = cmdrs.OrderBy(x => x.Name.Contains("(Legacy)"))
                            .ThenBy(x => x.Name);

            await context.Database.CloseConnectionAsync();
            return [.. ret];
        }

        public JournalCommander AddCommander(JournalCommander cmdr, bool updateHidden = false, bool updateCAPI = false)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.JournalCommanders.FirstOrDefault(x => string.Equals(x.Name, cmdr.Name));

            if (known == null)
            {
                known = new JournalCommanderDTO
                {
                    Name = cmdr.Name,
                    LastFile = cmdr.LastFile ?? string.Empty,
                    JournalDir = cmdr.JournalPath ?? string.Empty,
                    IsHidden = cmdr.IsHidden,
                    UseCAPI = cmdr.UseCAPI
                };
                context.JournalCommanders.Add(known);
                context.SaveChanges();
                return new(known.Id, known.Name, known.JournalDir, known.LastFile, known.IsHidden, known.UseCAPI);
            }

            known.LastFile = cmdr.LastFile ?? string.Empty;
            known.JournalDir = cmdr.JournalPath ?? string.Empty;
            known.Name = cmdr.Name;
            if (updateHidden)
                known.IsHidden = cmdr.IsHidden;
            if (updateCAPI)
                known.UseCAPI = cmdr.UseCAPI;
            context.SaveChanges();
            context.Database.CloseConnection();
            return new(known.Id, known.Name, known.JournalDir, known.LastFile, known.IsHidden, known.UseCAPI);
        }

        public JournalCommander? GetCommander(int cmdrId)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.JournalCommanders.FirstOrDefault(x => x.Id == cmdrId);

            if (known == null)
            {
                return null;
            }

            return new(known.Id, known.Name, known.JournalDir, known.LastFile, known.IsHidden, known.UseCAPI);
        }

        public async Task DeleteCommander(int commanderID)
        {
            using var context = _contextFactory.CreateDbContext();

            var cmdr = context.JournalCommanders.FirstOrDefault(x => x.Id == commanderID);

            if (cmdr == null)
            {
                return;
            }

            await context.JournalEntries.Where(x => x.CommanderID == commanderID).ExecuteDeleteAsync().ConfigureAwait(true);

            context.JournalCommanders.Remove(cmdr);

            await context.SaveChangesAsync(true);
        }
        #endregion

        #region Journal Methods
        public void AddJournalEntries(List<JournalEntry> journalEntries)
        {
            var entriesToAdd = journalEntries
                .OrderBy(x => x.Filename)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntryDTO(x.Filename,
                                                 x.Offset,
                                                 x.TimeStamp,
                                                 x.CommanderID,
                                                 (int)x.EventType,
                                                 x.OriginalEvent?.ToString(Formatting.None) ?? string.Empty)

                ).ToArray();


            using var context = _contextFactory.CreateDbContext();

            context.BulkInsertOrUpdate(entriesToAdd, new BulkConfig() { PropertiesToIncludeOnCompare = ["TimeStamp", "Offset"] });

            var connection = context.Database.GetDbConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "pragma optimize;";
                command.ExecuteNonQuery();
            }
        }

        public async Task<List<JournalEntry>> GetAllJournalEntries(int cmdrId)
        {
            using var context = _contextFactory.CreateDbContext();

            var ret = await context.JournalEntries
                .Where(x => x.CommanderID == cmdrId)
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntry(
                    x.Filename,
                    x.Offset,
                    x.CommanderID,
                    (JournalTypeEnum)x.EventTypeId,
                    JournalWatcher.GetEventData(x.EventData),
                    null))
                .ToListAsync();

            return ret;
        }

        public async Task<List<JournalEntry>> GetJournalEntriesOfType(int cmdrId, List<JournalTypeEnum> types)
        {
            return await GetJournalEntriesOfType(cmdrId, types, DateTime.MinValue);
        }

        public async Task GetJournalsStream(int cmdrId, IEnumerable<JournalTypeEnum> types, DateTime age, Func<JournalEntry, Task> callBack)
        {
            using var context = _contextFactory.CreateDbContext();
            var journals = context.JournalEntries.Where(x => x.CommanderID == cmdrId
                            && x.TimeStamp.Date >= age.Date
                            && types.Contains((JournalTypeEnum)x.EventTypeId))
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset)
                .AsNoTrackingWithIdentityResolution();
            await StreamJournals(journals, callBack);
        }

        private static async Task StreamJournals(IQueryable<JournalEntryDTO> journals, Func<JournalEntry, Task> callBack)
        {
            foreach (var x in journals)
            {
                var entry = new JournalEntry(
                    x.Filename,
                    x.Offset,
                    x.CommanderID,
                    (JournalTypeEnum)x.EventTypeId,
                    JournalWatcher.GetEventData(x.EventData),
                    null);

                await callBack(entry);
            }
        }

        public async Task<List<JournalEntry>> GetJournalEntriesOfType(int cmdrId, List<JournalTypeEnum> types, DateTime age)
        {
            if (types.Contains(JournalTypeEnum.Fileheader) == false)
                types.Add(JournalTypeEnum.Fileheader);

            using var context = _contextFactory.CreateDbContext();

            var ret = await context.JournalEntries
                .Where(x => x.CommanderID == cmdrId
                            && x.TimeStamp.Date >= age.Date
                            && types.Contains((JournalTypeEnum)x.EventTypeId))
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset)
                .Select(x => new JournalEntry(
                    x.Filename,
                    x.Offset,
                    x.CommanderID,
                    (JournalTypeEnum)x.EventTypeId,
                    JournalWatcher.GetEventData(x.EventData),
                    null))
                .ToListAsync();

            var firstHeader = ret.FirstOrDefault(x => x.EventType == JournalTypeEnum.Fileheader);

            if (firstHeader != null)
            {
                var index = ret.IndexOf(firstHeader);

                if (index > 0)
                {
                    ret.RemoveRange(0, index);
                }
            }
            return ret;
        }

        public Task ParseJournalEventsOfType(int cmdrId, List<JournalTypeEnum> types, Action<JournalEntry> callback, DateTime age)
        {
            using var context = _contextFactory.CreateDbContext();

            var entries = context.JournalEntries
                .Where(x => x.TimeStamp >= age && cmdrId == x.CommanderID)
                .EventTypeCompare(types)
                .OrderBy(x => x.TimeStamp)
                .ThenBy(x => x.Offset);

            foreach (var e in entries)
            {
                callback.Invoke(new JournalEntry(
                    e.Filename,
                    e.Offset,
                    e.CommanderID,
                    (JournalTypeEnum)e.EventTypeId,
                    JournalWatcher.GetEventData(e.EventData),
                    null));
            }
            return Task.CompletedTask;
        }

        public HashSet<string> GetAllReadFilenames()
        {
            var fourWeeks = DateTime.UtcNow.AddDays(-28);
            using var context = _contextFactory.CreateDbContext();

            var entries = context.JournalEntries
                .Where(x => x.EventTypeId == (int)JournalTypeEnum.Shutdown && x.TimeStamp >= fourWeeks
                || x.EventTypeId == (int)JournalTypeEnum.Fileheader && x.TimeStamp < fourWeeks)
                .Select(x => x.Filename)
                .Distinct()
                .ToHashSet();

            return entries;
        }
        #endregion

        #region Settings
        public List<SettingsDTO> GetAllSettings()
        {
            using var context = _contextFactory.CreateDbContext();

            return [.. context.Settings];
        }

        public void AddSettings(List<SettingsDTO> settings)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Settings.
                UpsertRange(settings)
                .On(x => x.Id)
                .Run();

            context.SaveChanges();

            context.Database.CloseConnection();
        }

        public void AddSetting(SettingsDTO settings)
        {
            using var context = _contextFactory.CreateDbContext();

            context.Settings.
                Upsert(settings)
                .On(x => x.Id)
                .Run();

            context.SaveChanges();
        }
        #endregion

        #region Database
        public async Task ResetDatabaseAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            await context.Database.ExecuteSqlRawAsync(
                "DELETE FROM JournalCommanders;" +
                "DELETE FROM JournalEntries;" +
                "DELETE FROM InactiveDepots;" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='CommanderIgnoredSystems';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='CartoIgnoredSystems';" +
                "DELETE FROM SQLITE_SEQUENCE WHERE name='InactiveDepots';");

            await context.SaveChangesAsync();
            context.Database.CloseConnection();
        }
        #endregion

        #region Colonisation
        public HashSet<Tuple<long, long, string>> GetInactiveDepots()
        {
            using var context = _contextFactory.CreateDbContext();

            return [.. context.InactiveDepots.Select(x => Tuple.Create(x.MarketID, x.SystemAddress, x.StationName))];
        }

        public void AddInactiveDepot(long marketID, long systemAddress, string stationName)
        {
            using var context = _contextFactory.CreateDbContext();
            context.InactiveDepots.
                Upsert(new DTOs.InactiveDepotsDTO(marketID, systemAddress, stationName))
                .Run();

            context.SaveChanges();
        }

        public void RemoveInactiveDepot(long marketID, long systemAddress, string stationName)
        {
            using var context = _contextFactory.CreateDbContext();

            var knownDepot = context.InactiveDepots.FirstOrDefault(x => x.MarketID == marketID && x.SystemAddress == systemAddress && x.StationName == stationName);

            if (knownDepot != null)
            {
                context.InactiveDepots.Remove(knownDepot);
                context.SaveChanges();
            }
        }

        public HashSet<Tuple<long, long, string>> GetDepotShoppingList()
        {
            using var context = _contextFactory.CreateDbContext();

            return [.. context.DepotShoppingList.Select(x => Tuple.Create(x.MarketID, x.SystemAddress, x.StationName))];
        }

        public void AddShoppingListDepot(long marketID, long systemAddress, string stationName)
        {
            using var context = _contextFactory.CreateDbContext();
            context.DepotShoppingList.
                Upsert(new DTOs.DepotShoppingListDTO(marketID, systemAddress, stationName))
                .Run();

            context.SaveChanges();
        }

        public void RemoveShoppingListDepot(long marketID, long systemAddress, string stationName)
        {
            using var context = _contextFactory.CreateDbContext();

            var knownDepot = context.DepotShoppingList.FirstOrDefault(x => x.MarketID == marketID && x.SystemAddress == systemAddress && x.StationName == stationName);

            if (knownDepot != null)
            {
                context.DepotShoppingList.Remove(knownDepot);
                context.SaveChanges();
            }
        }
        #endregion

        #region TickData
        public async Task AddTickData(IEnumerable<BGSTickData> data)
        {
            using var context = _contextFactory.CreateDbContext();
            context.TickData.
                UpsertRange(data)
                .Run();

            await context.SaveChangesAsync();
        }

        public async Task<List<BGSTickData>> GetTickData(DateTime maxAge)
        {
            using var context = _contextFactory.CreateDbContext();

            return await context.TickData.Where(x => x.Time >= maxAge)
                                         .OrderByDescending(x => x.Time)
                                         .ToListAsync();
        }

        internal async Task DeleteTickData(string iD)
        {
            using var context = _contextFactory.CreateDbContext();

            var data = context.TickData.FirstOrDefault(x => string.Equals(iD, x.Id));

            if (data is null)
                return;

            context.TickData.Remove(data);
            await context.SaveChangesAsync();
        }
        #endregion

        #region Bounties
        public Dictionary<string, DateTime> GetIgnoredBounties(int commanderID)
        {
            using var context = _contextFactory.CreateDbContext();

            return context.IgnoredBounties.Where(x => x.CommanderID == commanderID).ToDictionary(x => x.FactionName, x => x.BeforeDate);
        }

        public void DeleteIgnoredBounty(int commanderID, string factionName)
        {
            using var context = _contextFactory.CreateDbContext();

            var data = context.IgnoredBounties.FirstOrDefault(x => x.CommanderID == commanderID && string.Equals(x.FactionName, factionName));

            if (data is null)
                return;

            context.IgnoredBounties.Remove(data);
            context.SaveChanges();
        }

        public void AddIgnoredBounty(int commanderID, string factionName, DateTime beforeDate)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.IgnoredBounties.FirstOrDefault(x => x.CommanderID == commanderID && string.Equals(x.FactionName, factionName));

            if (known == null)
            {
                known = new IgnoredBounties(commanderID, factionName, beforeDate);
                context.IgnoredBounties.Add(known);
            }

            known.BeforeDate = beforeDate;
            context.SaveChanges();
        }
        #endregion

        #region Compass Bookmarks
        public List<SystemBookmarkDTO> GetBookmarkDTOs()
        {
            using var context = _contextFactory.CreateDbContext();

            return [.. context.SystemBookmarks.Include(x => x.Bookmarks)
                                              .Where(x => string.IsNullOrEmpty(x.Notes) == false || x.Bookmarks.Count > 0)];
        }

        public async Task<int> AddBookmark(SystemBookmarkVM system, BookmarkVM bookmark)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.SystemBookmarks.Include(x => x.Bookmarks).FirstOrDefault(x => x.Address == system.Address);

            var newBookmark = new BookMarkDTO()
            {
                BodyId = bookmark.BodyId,
                BodyName = bookmark.BodyName,
                BodyNameLocal = bookmark.BodyNameLocal,
                BookmarkName = bookmark.BookmarkName,
                Description = bookmark.Description,
                Latitude = bookmark.Latitude,
                Longitude = bookmark.Longitude,
            };

            if (known == null)
            {

                context.SystemBookmarks.Add(new SystemBookmarkDTO()
                {
                    Address = system.Address,
                    Name = system.Name,
                    Notes = system.Notes,
                    X = system.X,
                    Y = system.Y,
                    Z = system.Z,
                    Bookmarks = [newBookmark]
                });
                await context.SaveChangesAsync(true);

                return newBookmark.Id;
            }

            var knownBookmark = known.Bookmarks.FirstOrDefault(x => x.Id == bookmark.Id);

            if (knownBookmark != null)
            {
                knownBookmark.BodyId = bookmark.BodyId;
                knownBookmark.BodyName = bookmark.BodyName;
                knownBookmark.BodyNameLocal = bookmark.BodyNameLocal;
                knownBookmark.BookmarkName = bookmark.BookmarkName;
                knownBookmark.Description = bookmark.Description;
                knownBookmark.Latitude = bookmark.Latitude;
                knownBookmark.Longitude = bookmark.Longitude;

                await context.SaveChangesAsync(true);

                return knownBookmark.Id;
            }

            var newBkmark = new BookMarkDTO()
            {
                BodyId = bookmark.BodyId,
                BodyName = bookmark.BodyName,
                BodyNameLocal = bookmark.BodyNameLocal,
                BookmarkName = bookmark.BookmarkName,
                Description = bookmark.Description,
                Latitude = bookmark.Latitude,
                Longitude = bookmark.Longitude,
            };

            known.Notes = system.Notes;
            known.Bookmarks.Add(newBkmark);

            await context.SaveChangesAsync(true);
            return newBkmark.Id;
        }

        public async Task AddImportedBookmarks(List<SystemBookmarkDTO> systemBookmarks)
        {
            using var context = _contextFactory.CreateDbContext();

            foreach (var system in systemBookmarks)
            {
                var knownSystem = context.SystemBookmarks.Include(x => x.Bookmarks).FirstOrDefault(x => x.Address == system.Address);

                if (knownSystem == null)
                {
                    knownSystem = new SystemBookmarkDTO()
                    {
                        Address = system.Address,
                        Name = system.Name,
                        Notes = system.Notes,
                        X = system.X,
                        Y = system.Y,
                        Z = system.Z,
                        Bookmarks = []
                    };
                    context.SystemBookmarks.Add(knownSystem);
                }

                foreach (var bookmark in system.Bookmarks)
                {
                    var knownBookMark = knownSystem.Bookmarks.FirstOrDefault(x => x.BodyId == bookmark.BodyId
                                                                            && x.Latitude == bookmark.Latitude
                                                                            && x.Longitude == bookmark.Longitude);

                    if (knownBookMark != null)
                    {
                        continue;
                    }

                    knownBookMark = new BookMarkDTO()
                    {
                        BodyId = bookmark.BodyId,
                        BodyName = bookmark.BodyName,
                        BodyNameLocal = bookmark.BodyNameLocal,
                        BookmarkName = bookmark.BookmarkName,
                        Description = bookmark.Description,
                        Latitude = bookmark.Latitude,
                        Longitude = bookmark.Longitude,
                    };

                    knownSystem.Bookmarks.Add(knownBookMark);
                }
            }

            await context.SaveChangesAsync(true);
        }

        public async Task<List<SystemBookmark>> GetAllBookmarks()
        {
            using var context = _contextFactory.CreateDbContext();

            var ret = await context.SystemBookmarks
                .Include(body => body.Bookmarks)
                .Select(x => new SystemBookmark(x))
                .ToListAsync();

            return ret;
        }

        public async Task DeleteBookmark(long systemAddress, int bookmarkId)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.SystemBookmarks.Include(x => x.Bookmarks).FirstOrDefault(x => x.Address == systemAddress);

            if (known == null)
            {
                return;
            }

            var knownBookmark = known.Bookmarks.FirstOrDefault(x => x.Id == bookmarkId);

            if (knownBookmark == null)
            {
                return;
            }

            known.Bookmarks.Remove(knownBookmark);

            await context.SaveChangesAsync(true);
        }
        #endregion

        #region Spansh
        internal Dictionary<CsvType, SpanshCsvContainer> GetSpanshCSVsDictionary(int commanderID)
        {
            using var context = _contextFactory.CreateDbContext();

            var dict = context.SpanshCsvs
                .Where(x => x.CommanderID == commanderID)
                .ToDictionary(x => (CsvType)x.CsvType, y => JsonConvert.DeserializeObject<SpanshCsvContainer>(y.Json));

            var ret = new Dictionary<CsvType, SpanshCsvContainer>();

            foreach (var kvp in dict)
            {
                if (kvp.Value is not null)
                {
                    ret.Add(kvp.Key, kvp.Value);
                }
            }
            return ret;
        }

        internal void DeleteCSV(CsvType csvType, int commanderID)
        {
            using var context = _contextFactory.CreateDbContext();

            var known = context.SpanshCsvs.Where(x => x.CsvType == (int)csvType && x.CommanderID == commanderID).FirstOrDefault();

            if (known != null)
            {
                context.SpanshCsvs.Remove(known);
                context.SaveChanges();
            }
        }

        internal void SaveCVSs(Dictionary<CsvType, SpanshCsvContainer> csvs, int commanderID)
        {
            List<SpanshCsvDTO> csvList = [];

            foreach (var kvp in csvs)
            {
                if (kvp.Value.HasValue == false)
                    continue;

                var json = JsonConvert.SerializeObject(kvp.Value, Formatting.None);

                if (string.IsNullOrEmpty(json))
                    continue;

                csvList.Add(new((int)kvp.Key, commanderID, json));
            }

            using var context = _contextFactory.CreateDbContext();

            context.SpanshCsvs.UpsertRange(csvList)
                .On(e => new { e.CsvType, e.CommanderID })
                .Run();

            context.SaveChanges();
        }
        #endregion
    }
}
