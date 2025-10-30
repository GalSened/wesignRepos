using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualIdentificationToProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VisualIdentificationUsedNotifyCount",
                table: "ProgramUtilizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "VisualIdentifications",
                table: "ProgramUtilizations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VisualIdentificationsUsage",
                table: "ProgramUtilizationHistories",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "VisualIdentificationsPerMonth",
                table: "Programs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "VisualIdentificationsPerMonth",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "VisualIdentificationsPerMonth",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "VisualIdentificationsPerMonth",
                value: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisualIdentificationUsedNotifyCount",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "VisualIdentifications",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "VisualIdentificationsUsage",
                table: "ProgramUtilizationHistories");

            migrationBuilder.DropColumn(
                name: "VisualIdentificationsPerMonth",
                table: "Programs");
        }
    }
}
