using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class PasswordSetupTimeDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
 
            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                columns: new[] { "PasswordExpirationInDays", "RecentPasswordsAmount" },
                values: new object[] { 0, 3 });

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                columns: new[] { "PasswordExpirationInDays", "RecentPasswordsAmount" },
                values: new object[] { 0, 3 });

         

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "PasswordSetupTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "PasswordSetupTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "PasswordSetupTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000606"),
                column: "PasswordSetupTime",
                value: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
            migrationBuilder.Sql(sql: "update Users set PasswordSetupTime = GETDATE() where PasswordSetupTime = '0001-01-01 00:00:00.0000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

        }
    }
}
