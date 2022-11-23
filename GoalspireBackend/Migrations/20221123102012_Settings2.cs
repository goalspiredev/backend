using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GoalspireBackend.Migrations
{
    /// <inheritdoc />
    public partial class Settings2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReducedAnimations = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultSnoozeDuration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    DailyNotificationTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DefaultGoalVisibility = table.Column<int>(type: "integer", nullable: false),
                    GoalTags = table.Column<List<string>>(type: "text[]", nullable: false),
                    DisableEmailNotifications = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.UserId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
