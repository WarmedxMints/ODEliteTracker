using Microsoft.EntityFrameworkCore;

namespace ODEliteTracker.Database
{
    public sealed class ODEliteTrackerDbContextFactory(string connectionString)
    {
        private readonly string _connectionString = connectionString;
        public ODEliteTrackerDbContext CreateDbContext()
        {
            DbContextOptions options = new DbContextOptionsBuilder().UseSqlite(_connectionString, x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)).Options;

            var context = new ODEliteTrackerDbContext(options);
            using var connection = context.Database.GetDbConnection();
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "pragma journal_mode = WAL;PRAGMA synchronous = normal;pragma temp_store = memory;pragma mmap_size = 30000000000;";
                command.ExecuteNonQuery();
            }
            connection.Close();
            return context;
        }
    }
}
