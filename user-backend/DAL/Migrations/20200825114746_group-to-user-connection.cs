using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class grouptouserconnection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupDAOId",
                table: "Users",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "LogArichveIntervalInDays",
                column: "Value",
                value: "30");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GroupDAOId",
                table: "Users",
                column: "GroupDAOId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Groups_GroupDAOId",
                table: "Users",
                column: "GroupDAOId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Groups_GroupDAOId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_GroupDAOId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GroupDAOId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "LogArichveIntervalInDays",
                column: "Value",
                value: "14");
        }
    }
}
