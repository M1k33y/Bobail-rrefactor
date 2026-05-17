using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bobail.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGamePlayersConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers");

            migrationBuilder.CreateIndex(
                name: "IX_Games_WinnerUserId",
                table: "Games",
                column: "WinnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_Color",
                table: "GamePlayers",
                columns: new[] { "GameId", "Color" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_UserId_Unique",
                table: "GamePlayers",
                columns: new[] { "GameId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_UserId",
                table: "GamePlayers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Users_UserId",
                table: "GamePlayers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_WinnerUserId",
                table: "Games",
                column: "WinnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Users_UserId",
                table: "GamePlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_WinnerUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_WinnerUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_Color",
                table: "GamePlayers");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_UserId_Unique",
                table: "GamePlayers");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_UserId",
                table: "GamePlayers");

            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId",
                table: "GamePlayers",
                column: "GameId");
        }
    }
}
