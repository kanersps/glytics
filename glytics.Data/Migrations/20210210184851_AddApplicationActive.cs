using Microsoft.EntityFrameworkCore.Migrations;

namespace glytics.Migrations
{
    public partial class AddApplicationActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Active",
                table: "Application",
                type: "tinyint(1)",
                nullable: true,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Active",
                table: "Application");
        }
    }
}
