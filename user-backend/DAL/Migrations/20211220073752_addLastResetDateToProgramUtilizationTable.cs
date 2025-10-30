using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addLastResetDateToProgramUtilizationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastResetDate",
                table: "ProgramUtilizations",
                type: "datetime2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(
                sql: "update ProgramUtilizations set StartDate = GETDATE() where StartDate = '0001-01-01 00:00:00.0000000'");

            migrationBuilder.Sql(
                sql: "update ProgramUtilizations set LastResetDate = DATEADD(day, -30, StartDate)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastResetDate",
                table: "ProgramUtilizations");
        }
    }
}
