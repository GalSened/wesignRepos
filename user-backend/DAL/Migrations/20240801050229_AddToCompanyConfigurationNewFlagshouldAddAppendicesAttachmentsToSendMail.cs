using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddToCompanyConfigurationNewFlagshouldAddAppendicesAttachmentsToSendMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldAddAppendicesAttachmentsToSendMail",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "ShouldAddAppendicesAttachmentsToSendMail",
                value: false);

            migrationBuilder.UpdateData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"),
                column: "ShouldAddAppendicesAttachmentsToSendMail",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldAddAppendicesAttachmentsToSendMail",
                table: "CompanyConfigurations");
        }
    }
}
