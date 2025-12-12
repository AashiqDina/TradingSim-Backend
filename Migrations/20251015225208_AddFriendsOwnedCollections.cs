using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradingSimulator_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendsOwnedCollections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserObj");

            migrationBuilder.CreateTable(
                name: "Friends_FriendsList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FriendsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends_FriendsList", x => new { x.FriendsId, x.Id });
                    table.ForeignKey(
                        name: "FK_Friends_FriendsList_Friends_FriendsId",
                        column: x => x.FriendsId,
                        principalTable: "Friends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friends_ReceivedRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FriendsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends_ReceivedRequests", x => new { x.FriendsId, x.Id });
                    table.ForeignKey(
                        name: "FK_Friends_ReceivedRequests_Friends_FriendsId",
                        column: x => x.FriendsId,
                        principalTable: "Friends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Friends_SentRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FriendsId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    ProfitLoss = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends_SentRequests", x => new { x.FriendsId, x.Id });
                    table.ForeignKey(
                        name: "FK_Friends_SentRequests_Friends_FriendsId",
                        column: x => x.FriendsId,
                        principalTable: "Friends",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Friends_FriendsList");

            migrationBuilder.DropTable(
                name: "Friends_ReceivedRequests");

            migrationBuilder.DropTable(
                name: "Friends_SentRequests");

            migrationBuilder.CreateTable(
                name: "UserObj",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FriendsId = table.Column<int>(type: "INTEGER", nullable: true),
                    FriendsId1 = table.Column<int>(type: "INTEGER", nullable: true),
                    FriendsId2 = table.Column<int>(type: "INTEGER", nullable: true),
                    ProfitLoss = table.Column<decimal>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
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
    }
}
