using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudNine.Core.Migrations
{
    public partial class WarnForgive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ForgiveAfter",
                table: "Moderation",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ForgiveAfter",
                table: "Moderation");
        }
    }
}
