using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class ColonisationRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColonisationRouteSystems",
                columns: table => new
                {
                    CommanderId = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemAddress = table.Column<long>(type: "INTEGER", nullable: false),
                    HopNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SystemName = table.Column<string>(type: "TEXT", nullable: false),
                    X = table.Column<double>(type: "REAL", nullable: false),
                    Y = table.Column<double>(type: "REAL", nullable: false),
                    Z = table.Column<double>(type: "REAL", nullable: false),
                    StarClass = table.Column<string>(type: "TEXT", nullable: false),
                    Built = table.Column<bool>(type: "INTEGER", nullable: false),
                    Hub = table.Column<bool>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColonisationRouteSystems", x => new { x.CommanderId, x.SystemAddress });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColonisationRouteSystems");
        }
    }
}
