using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLand.Core.Migrations
{
    /// <inheritdoc />
    public partial class NDB : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
