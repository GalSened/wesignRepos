using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShouldEnableGovernmentSignatureFromCompanyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF EXISTS (
            SELECT 1
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = 'CompanyConfigurations'
              AND COLUMN_NAME = 'ShouldEnableGovernmentSignatureFormat'
        )
        BEGIN
            ALTER TABLE CompanyConfigurations DROP COLUMN ShouldEnableGovernmentSignatureFormat
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldEnableGovernmentSignatureFormat",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "ShouldEnableGovernmentSignatureFormat",
                value: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "ShouldEnableGovernmentSignatureFormat",
                value: false);
        }
    }
}
