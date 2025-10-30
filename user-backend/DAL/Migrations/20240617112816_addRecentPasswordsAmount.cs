using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class addRecentPasswordsAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecentPasswordsAmount",
                table: "CompanyConfigurations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "RecentPasswordsAmount",
                value: 3);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "RecentPasswordsAmount",
                value: 3);

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "RecentPasswordsAmount", "3" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "RecentPasswordsAmount");

            migrationBuilder.DropColumn(
                name: "RecentPasswordsAmount",
                table: "CompanyConfigurations");
        }
    }
}
