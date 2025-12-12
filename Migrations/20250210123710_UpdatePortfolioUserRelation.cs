using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingSimulator_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePortfolioUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios");

            migrationBuilder.CreateIndex(
                name: "IX_Portfolios_UserId",
                table: "Portfolios",
                column: "UserId");
        }
    }
}
