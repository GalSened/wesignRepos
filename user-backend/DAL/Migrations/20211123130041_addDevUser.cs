using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addDevUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CompanyId", "CreationSource", "CreationTime", "Email", "GroupDAOId", "GroupId", "Name", "Password", "ProgramUtilizationId", "Status", "Type" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000606"), new Guid("00000000-0000-0000-0000-000000000505"), 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "dev@comda.co.il", null, new Guid("00000000-0000-0000-0000-000000000000"), "DevUser", "h05j6uZ6S0kHffebPVOUy4Cr1QBfRbsb9oO4/IShSVNyw9sc", null, 2, 7 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000606"));
        }
    }
}
