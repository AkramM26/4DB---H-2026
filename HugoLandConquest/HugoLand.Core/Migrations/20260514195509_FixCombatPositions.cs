using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLand.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixCombatPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ForcesTable",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TerritoryDefendedPosX",
                table: "CombatEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TerritoryDefendedPosY",
                table: "CombatEvents",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TerritoryDefendedPosX",
                table: "CombatEvents");

            migrationBuilder.DropColumn(
                name: "TerritoryDefendedPosY",
                table: "CombatEvents");

            migrationBuilder.AlterColumn<string>(
                name: "ForcesTable",
                table: "Players",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
