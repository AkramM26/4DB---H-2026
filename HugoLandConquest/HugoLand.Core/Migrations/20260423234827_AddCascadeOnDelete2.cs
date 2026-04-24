using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLand.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCascadeOnDelete2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installations_Games_GameId",
                table: "Installations");

            migrationBuilder.DropForeignKey(
                name: "FK_Installations_Territories_TerritoryId",
                table: "Installations");

            migrationBuilder.DropForeignKey(
                name: "FK_MilitaryDetachments_Games_GameId",
                table: "MilitaryDetachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MilitaryDetachments_Territories_TerritoryId",
                table: "MilitaryDetachments");

            migrationBuilder.AddForeignKey(
                name: "FK_Installations_Games_GameId",
                table: "Installations",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Installations_Territories_TerritoryId",
                table: "Installations",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MilitaryDetachments_Games_GameId",
                table: "MilitaryDetachments",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MilitaryDetachments_Territories_TerritoryId",
                table: "MilitaryDetachments",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installations_Games_GameId",
                table: "Installations");

            migrationBuilder.DropForeignKey(
                name: "FK_Installations_Territories_TerritoryId",
                table: "Installations");

            migrationBuilder.DropForeignKey(
                name: "FK_MilitaryDetachments_Games_GameId",
                table: "MilitaryDetachments");

            migrationBuilder.DropForeignKey(
                name: "FK_MilitaryDetachments_Territories_TerritoryId",
                table: "MilitaryDetachments");

            migrationBuilder.AddForeignKey(
                name: "FK_Installations_Games_GameId",
                table: "Installations",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installations_Territories_TerritoryId",
                table: "Installations",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MilitaryDetachments_Games_GameId",
                table: "MilitaryDetachments",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MilitaryDetachments_Territories_TerritoryId",
                table: "MilitaryDetachments",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
