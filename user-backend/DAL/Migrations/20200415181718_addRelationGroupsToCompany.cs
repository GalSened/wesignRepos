using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addRelationGroupsToCompany : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreationTime",
                value: new DateTime(2020, 4, 15, 18, 17, 17, 763, DateTimeKind.Utc).AddTicks(2794));

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CompanyId",
                table: "Groups",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Companies_CompanyId",
                table: "Groups",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Companies_CompanyId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CompanyId",
                table: "Groups");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreationTime",
                value: new DateTime(2020, 4, 14, 6, 32, 12, 888, DateTimeKind.Utc).AddTicks(400));
        }
    }
}
