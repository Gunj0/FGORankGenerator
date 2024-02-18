using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FGORankGenerator.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Servants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: true),
                    Class = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Range = table.Column<string>(type: "TEXT", nullable: true),
                    AppMediaRate = table.Column<int>(type: "INTEGER", nullable: false),
                    AppMediaOrbit = table.Column<int>(type: "INTEGER", nullable: false),
                    OverallRank = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servants", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Servants");
        }
    }
}
