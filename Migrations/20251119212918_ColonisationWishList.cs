using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODEliteTracker.Migrations
{
    /// <inheritdoc />
    public partial class ColonisationWishList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColonisationWishList",
                columns: table => new
                {
                    CommanderID = table.Column<int>(type: "INTEGER", nullable: false),
                    Build = table.Column<int>(type: "INTEGER", nullable: false),
                    Count = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColonisationWishList", x => new { x.CommanderID, x.Build });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColonisationWishList");
        }
    }
}
