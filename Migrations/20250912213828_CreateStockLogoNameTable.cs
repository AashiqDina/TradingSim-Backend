using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingSimulator_Backend.Migrations
{
    /// <inheritdoc />
    public partial class CreateStockLogoNameTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockLogoName",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Logo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockLogoName", x => x.Symbol);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockLogoName");
        }
    }
}
