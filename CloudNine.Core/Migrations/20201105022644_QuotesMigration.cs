using Microsoft.EntityFrameworkCore.Migrations;

namespace CloudNine.Core.Migrations
{
    public partial class QuotesMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Quotes",
                table: "ServerConfigurations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quotes",
                table: "ServerConfigurations");
        }
    }
}
