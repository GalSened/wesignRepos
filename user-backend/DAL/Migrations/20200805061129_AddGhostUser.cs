using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddGhostUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "Name", "ProgramId", "ProgramUtilizationId", "Status" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000505"), "Ghost Users", new Guid("00000000-0000-0000-0000-000000000001"), null, 0 });


            migrationBuilder.InsertData(
               table: "Users",
               columns: new[] { "Id", "CompanyId", "CreationTime", "Email", "GroupId", "Name", "Password", "ProgramUtilizationId", "Status", "Type" },
               values: new object[] { new Guid("00000000-0000-0000-0000-000000000505"), new Guid("00000000-0000-0000-0000-000000000505"), new DateTime(2020, 4, 30, 6, 14, 44, 872, DateTimeKind.Utc).AddTicks(8294), "ghost@comda.co.il", new Guid("00000000-0000-0000-0000-000000000000"), "GhostUser", "oRxggjg8wbOxTC5DvP4vXzV32vFmnNdhQH8vRpElJ6lziTJk", null, 2, 5 });


            //migrationBuilder.UpdateData(
            //    table: "Users",
            //    keyColumn: "Id",
            //    keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
            //    column: "CompanyId",
            //    value: new Guid("00000000-0000-0000-0000-000000000505"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"));

            migrationBuilder.DeleteData(
                table: "Companies",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"));

            //migrationBuilder.InsertData(
            //    table: "Users",
            //    columns: new[] { "Id", "CompanyId", "CreationTime", "Email", "GroupId", "Name", "Password", "ProgramUtilizationId", "Status", "Type" },
            //    values: new object[] { new Guid("00000000-0000-0000-0000-000000000505"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ghost@comda.co.il", new Guid("00000000-0000-0000-0000-000000000000"), "GhostUser", "mKlB/1NTSZ/TkglLU7xJj/B5Yr49GRlj/5RHwUsXHsciRcYDBTIIe3UorWAcJZCx", null, 2, 5 });
        }
    }
}
