using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class WatchedMarkets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchedMarkets",
                columns: table => new
                {
                    MarketID = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CommoditiesJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchedMarkets", x => x.MarketID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchedMarkets");
        }
    }
}
