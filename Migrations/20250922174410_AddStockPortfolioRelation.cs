using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingSimulator_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddStockPortfolioRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Portfolios_PortfolioId",
                table: "Stocks");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Stocks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Portfolios_PortfolioId",
                table: "Stocks",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stocks_Portfolios_PortfolioId",
                table: "Stocks");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Stocks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Stocks_Portfolios_PortfolioId",
                table: "Stocks",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "Id");
        }
    }
}
