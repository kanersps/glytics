using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace glytics.Data.Migrations
{
    public partial class BasicStatistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Visits = table.Column<int>(type: "int", nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    ApplicationTrackingCode = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Statistic_Application_ApplicationTrackingCode",
                        column: x => x.ApplicationTrackingCode,
                        principalTable: "Application",
                        principalColumn: "TrackingCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Statistic_ApplicationTrackingCode",
                table: "Statistic",
                column: "ApplicationTrackingCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistic");
        }
    }
}
