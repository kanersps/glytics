using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace glytics.Data.Migrations
{
    public partial class AddPathStatistic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatisticPath",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Visits = table.Column<int>(type: "int", nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "longtext", nullable: true),
                    ApplicationTrackingCode = table.Column<string>(type: "varchar(255)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatisticPath", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatisticPath_Application_ApplicationTrackingCode",
                        column: x => x.ApplicationTrackingCode,
                        principalTable: "Application",
                        principalColumn: "TrackingCode",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatisticPath_ApplicationTrackingCode",
                table: "StatisticPath",
                column: "ApplicationTrackingCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatisticPath");
        }
    }
}
