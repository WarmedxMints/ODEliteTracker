using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class ColonisationRouteTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BodyCount",
                table: "ColonisationRouteSystems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StarCount",
                table: "ColonisationRouteSystems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BodyCount",
                table: "ColonisationRouteSystems");

            migrationBuilder.DropColumn(
                name: "StarCount",
                table: "ColonisationRouteSystems");
        }
    }
}
