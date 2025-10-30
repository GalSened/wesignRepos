using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class addCompanyConfigurationToDefualtCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CompanyConfigurations",
                columns: new[] { "CompanyId", "CanUserControlReminderSettings", "DefaultSigningType", "DeleteSignedDocumentAfterXDays", "DeleteUnsignedDocumentAfterXDays", "DocumentNotificationsEndpoint", "EnableVisualIdentityFlow", "Language", "ShouldEnableSignReminders", "ShouldNotifyWhileSignerSigned", "ShouldSendDocumentNotifications", "ShouldSendSignedDocument", "ShouldSendWithOTPByDefault", "SignReminderFrequencyInDays", "SignatureColor", "SignerLinkExpirationInDays", "isPersonalizedPFX" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), false, 1, 0, 0, null, false, 0, false, false, false, false, false, 0, null, 0, false },
                    { new Guid("00000000-0000-0000-0000-000000000505"), false, 1, 0, 0, null, false, 0, false, false, false, false, false, 0, null, 0, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CompanyConfigurations",
                keyColumn: "CompanyId",
                keyValue: new Guid("00000000-0000-0000-0000-000000000505"));
        }
    }
}
