using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bobail.Infrastructure.Migrations
{
    public partial class AddGamePlayersConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // add unique index on GameId + Color
            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_Color",
                table: "GamePlayers",
                columns: new[] { "GameId", "Color" },
                unique: true);

            // add unique filtered index on GameId + UserId where UserId is not null
            migrationBuilder.CreateIndex(
                name: "IX_GamePlayers_GameId_UserId_Unique",
                table: "GamePlayers",
                columns: new[] { "GameId", "UserId" },
                unique: true,
                filter: "[UserId] IS NOT NULL");

            // add FK from GamePlayers.UserId -> Users.Id (nullable)
            migrationBuilder.AddForeignKey(
                name: "FK_GamePlayers_Users_UserId",
                table: "GamePlayers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // add FK from Games.WinnerUserId -> Users.Id (nullable)
            migrationBuilder.AddForeignKey(
                name: "FK_Games_Users_WinnerUserId",
                table: "Games",
                column: "WinnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GamePlayers_Users_UserId",
                table: "GamePlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_Games_Users_WinnerUserId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_Color",
                table: "GamePlayers");

            migrationBuilder.DropIndex(
                name: "IX_GamePlayers_GameId_UserId_Unique",
                table: "GamePlayers");
        }
    }
}
