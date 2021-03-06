﻿using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudNine.Core.Migrations
{
    public partial class ModNoticesFavQuotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FavoriteQuotes",
                table: "ServerConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModlogNotices",
                table: "Moderation",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FavoriteQuotes",
                table: "ServerConfigurations");

            migrationBuilder.DropColumn(
                name: "ModlogNotices",
                table: "Moderation");
        }
    }
}
