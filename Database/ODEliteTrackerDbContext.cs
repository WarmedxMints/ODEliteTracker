﻿using Microsoft.EntityFrameworkCore;
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
    }
}
