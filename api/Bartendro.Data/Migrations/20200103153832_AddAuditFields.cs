using Microsoft.EntityFrameworkCore.Migrations;

namespace Bartendro.Database.Migrations
{
    public partial class AddAuditFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Recipes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Deleted",
                table: "Ingredients",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Deleted",
                table: "Ingredients");
        }
    }
}
