using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLand.Core.Migrations
{
    /// <inheritdoc />
    public partial class gamesize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameSizeX",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GameSizeY",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameSizeX",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "GameSizeY",
                table: "Games");
        }
    }
}
