using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace glytics.Migrations
{
    public partial class AddApiKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApiKey",
                columns: table => new
                {
                    Key = table.Column<Guid>(type: "char(36)", nullable: false),
                    AccountId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKey", x => x.Key);
                    table.ForeignKey(
                        name: "FK_ApiKey_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_AccountId",
                table: "ApiKey",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApiKey");
        }
    }
}
