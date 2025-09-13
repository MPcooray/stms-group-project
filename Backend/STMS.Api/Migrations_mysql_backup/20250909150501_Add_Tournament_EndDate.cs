using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STMS.Api.Migrations
{
    /// <inheritdoc />
    public partial class Add_Tournament_EndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Tournaments",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Tournaments");
        }
    }
}
