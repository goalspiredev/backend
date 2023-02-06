using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalspireBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddGoalTagsAndDefaultSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultGoalVisibility",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "IanaTimeZone",
                table: "Settings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "Tags",
                table: "Goals",
                type: "text[]",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IanaTimeZone",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Goals");

            migrationBuilder.AddColumn<int>(
                name: "DefaultGoalVisibility",
                table: "Settings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
