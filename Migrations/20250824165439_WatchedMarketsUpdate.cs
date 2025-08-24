using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class WatchedMarketsUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WatchedMarkets",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "WatchedMarkets");
        }
    }
}
