using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class SpanshRouteUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpanshCsvs");

            migrationBuilder.CreateTable(
                name: "SpanshRoutes",
                columns: table => new
                {
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Format = table.Column<int>(type: "INTEGER", nullable: false),
                    RouteType = table.Column<int>(type: "INTEGER", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpanshRoutes", x => new { x.CommanderID, x.CreatedDate });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpanshRoutes");

            migrationBuilder.CreateTable(
                name: "SpanshCsvs",
                columns: table => new
                {
                    CsvType = table.Column<int>(type: "INTEGER", nullable: false),
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpanshCsvs", x => new { x.CsvType, x.CommanderID });
                });
        }
    }
}
