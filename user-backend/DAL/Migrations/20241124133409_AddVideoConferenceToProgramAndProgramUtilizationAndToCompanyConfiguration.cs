using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoConferenceToProgramAndProgramUtilizationAndToCompanyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "VideoConference",
                table: "ProgramUtilizations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "VideoConferenceUsedNotifyCount",
                table: "ProgramUtilizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "VideoConferencePerMonth",
                table: "Programs",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldEnableVideoConference",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "ShouldEnableVideoConference",
                value: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "ShouldEnableVideoConference",
                value: false);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "VideoConferencePerMonth",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "VideoConferencePerMonth",
                value: 0L);

            migrationBuilder.UpdateData(
                table: "Programs",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "VideoConferencePerMonth",
                value: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoConference",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "VideoConferenceUsedNotifyCount",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "VideoConferencePerMonth",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldEnableVideoConference",
                table: "CompanyConfigurations");
        }
    }
}
