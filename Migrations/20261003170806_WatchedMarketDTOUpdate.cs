using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class WatchedMarketDTOUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchedMarkets",
                table: "WatchedMarkets");

            migrationBuilder.AlterColumn<ulong>(
                name: "MarketID",
                table: "WatchedMarkets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .OldAnnotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "CommanderID",
                table: "WatchedMarkets",
                type: "INTEGER",
                nullable: false,
                defaultValue: -1);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchedMarkets",
                table: "WatchedMarkets",
                columns: new[] { "MarketID", "CommanderID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_WatchedMarkets",
                table: "WatchedMarkets");

            migrationBuilder.DropColumn(
                name: "CommanderID",
                table: "WatchedMarkets");

            migrationBuilder.AlterColumn<ulong>(
                name: "MarketID",
                table: "WatchedMarkets",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(ulong),
                oldType: "INTEGER")
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_WatchedMarkets",
                table: "WatchedMarkets",
                column: "MarketID");
        }
    }
}
