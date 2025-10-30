using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class UpdateProgramModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OnlineMode",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "TemplatesPerMonth",
                table: "Programs");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowContacts",
                table: "Programs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowGroupSign",
                table: "Programs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowLiveMode",
                table: "Programs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowSelfSign",
                table: "Programs",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Templates",
                table: "Programs",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Templates",
                value: 2L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "Templates",
                value: -1L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "Templates",
                value: 15L);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "Password",
                value: "oRxggjg8wbOxTC5DvP4vXzV32vFmnNdhQH8vRpElJ6lziTJk");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "ShouldShowContacts",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowGroupSign",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowLiveMode",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowSelfSign",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "Templates",
                table: "Programs");

            migrationBuilder.AddColumn<bool>(
                name: "OnlineMode",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TemplatesPerMonth",
                table: "Programs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "TemplatesPerMonth",
                value: 2L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                columns: new[] { "OnlineMode", "TemplatesPerMonth" },
                values: new object[] { true, -1L });

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                columns: new[] { "OnlineMode", "TemplatesPerMonth" },
                values: new object[] { true, 15L });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "Password",
                value: "mKlB/1NTSZ/TkglLU7xJj/B5Yr49GRlj/5RHwUsXHsciRcYDBTIIe3UorWAcJZCx");
        }
    }
}
