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
            migrationBuilder.AddColumn<string>(
                name: "ForcesTable",
                table: "Players",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForcesTable",
                table: "Players");
        }
    }
}
