using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class UseEventIdForTiming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add EventId column to Timings table
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Timings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Drop the old index on Event
            migrationBuilder.DropIndex(
                name: "IX_Timings_PlayerId_Event",
                table: "Timings");

            // Remove the old Event column
            migrationBuilder.DropColumn(
                name: "Event",
                table: "Timings");

            // Create new index for PlayerId and EventId
            migrationBuilder.CreateIndex(
                name: "IX_Timings_PlayerId_EventId",
                table: "Timings",
                columns: new[] { "PlayerId", "EventId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the new index
            migrationBuilder.DropIndex(
                name: "IX_Timings_PlayerId_EventId",
                table: "Timings");

            // Remove the new EventId column
            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Timings");

            // Add back the old Event column
            migrationBuilder.AddColumn<string>(
                name: "Event",
                table: "Timings",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                defaultValue: "");

            // Restore the old index
            migrationBuilder.CreateIndex(
                name: "IX_Timings_PlayerId_Event",
                table: "Timings",
                columns: new[] { "PlayerId", "Event" });
        }
    }
}