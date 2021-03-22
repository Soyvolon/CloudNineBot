using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudNine.Core.Migrations.BlogDatabaseModelMigrations
{
    public partial class BlogInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Featured = table.Column<bool>(type: "INTEGER", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Author = table.Column<string>(type: "TEXT", nullable: false),
                    Editors = table.Column<string>(type: "TEXT", nullable: false),
                    Markdown = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Showcases",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Markdown = table.Column<string>(type: "TEXT", nullable: false),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Showcases", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "Showcases");
        }
    }
}
