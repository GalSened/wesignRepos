using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreationTime",
                value: new DateTime(2020, 4, 30, 6, 14, 44, 872, DateTimeKind.Utc).AddTicks(6373));

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyId", "CreationTime", "Email", "GroupId", "Name", "Password", "ProgramUtilizationId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000002"), new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2020, 4, 30, 6, 14, 44, 872, DateTimeKind.Utc).AddTicks(8294), "paymentAdmin@comda.co.il", new Guid("00000000-0000-0000-0000-000000000000"), "PaymentAdmin", "aFDgUq3rMdhhvRqzQ+/9v51hevUQyVubl2XdsvpZqQ/Q4dVz", null, 2, 4 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "CreationTime",
                value: new DateTime(2020, 4, 15, 18, 17, 17, 763, DateTimeKind.Utc).AddTicks(2794));
        }
    }
}
