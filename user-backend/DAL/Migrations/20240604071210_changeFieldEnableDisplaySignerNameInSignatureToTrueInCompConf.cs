using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class changeFieldEnableDisplaySignerNameInSignatureToTrueInCompConf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "EnableDisplaySignerNameInSignature",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "EnableDisplaySignerNameInSignature",
                value: true);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "EnableDisplaySignerNameInSignature",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "EnableDisplaySignerNameInSignature",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "EnableDisplaySignerNameInSignature",
                value: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "EnableDisplaySignerNameInSignature",
                value: false);
        }
    }
}
