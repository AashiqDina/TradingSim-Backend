using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingSimulator_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId1 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Friends_Users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserObj",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "TEXT", nullable: false),
                    FriendsId = table.Column<int>(type: "INTEGER", nullable: true),
                    FriendsId1 = table.Column<int>(type: "INTEGER", nullable: true),
                    FriendsId2 = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserObj", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserObj_Friends_FriendsId",
                        column: x => x.FriendsId,
                        principalTable: "Friends",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserObj_Friends_FriendsId1",
                        column: x => x.FriendsId1,
                        principalTable: "Friends",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserObj_Friends_FriendsId2",
                        column: x => x.FriendsId2,
                        principalTable: "Friends",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId",
                table: "Friends",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friends_UserId1",
                table: "Friends",
                column: "UserId1",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserObj_FriendsId",
                table: "UserObj",
                column: "FriendsId");

            migrationBuilder.CreateIndex(
                name: "IX_UserObj_FriendsId1",
                table: "UserObj",
                column: "FriendsId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserObj_FriendsId2",
                table: "UserObj",
                column: "FriendsId2");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserObj");

            migrationBuilder.DropTable(
                name: "Friends");
        }
    }
}
