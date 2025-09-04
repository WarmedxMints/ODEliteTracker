using Microsoft.EntityFrameworkCore;
using ODEliteTracker.Database.DTOs;
using ODJournalDatabase.Database;

namespace ODEliteTracker.Database
{
    public sealed class ODEliteTrackerDbContext(DbContextOptions options) : JournalContextBase(options)
    {
        public DbSet<InactiveDepotsDTO> InactiveDepots { get; set; }
        public DbSet<DepotShoppingListDTO> DepotShoppingList { get; set; }
        public DbSet<BGSTickData> TickData { get; set; }
        public DbSet<IgnoredBounties> IgnoredBounties { get; set; }
        public DbSet<SystemBookmarkDTO> SystemBookmarks { get; set; }
        public DbSet<SpanshCsvDTO> SpanshCsvs { get; set; }
        public DbSet<WatchedMarketDTO> WatchedMarkets { get; set; }
        public DbSet<BGSIgnoredSystemsDTO> BGSIgnoredSystems { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<SystemBookmarkDTO>()
                .HasMany(e => e.Bookmarks)
                .WithOne()
                .HasForeignKey(e => e.SystemAddress)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SystemAddress");
        }
    }
}
