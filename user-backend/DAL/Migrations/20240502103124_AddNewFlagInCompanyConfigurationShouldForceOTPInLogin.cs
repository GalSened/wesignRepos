using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFlagInCompanyConfigurationShouldForceOTPInLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldForceOTPInLogin",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "ShouldForceOTPInLogin",
                value: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "ShouldForceOTPInLogin",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldForceOTPInLogin",
                table: "CompanyConfigurations");
        }
    }
}
