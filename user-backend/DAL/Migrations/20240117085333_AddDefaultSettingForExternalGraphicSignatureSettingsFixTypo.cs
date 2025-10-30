using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultSettingForExternalGraphicSignatureSettingsFixTypo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraficSignatureCert");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraficSignaturePin");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraficSignatureSigner1Url");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "UseExternalGraficSignature");

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "ExternalGraphicSignatureCert", "" },
                    { "ExternalGraphicSignaturePin", "" },
                    { "ExternalGraphicSignatureSigner1Url", "" },
                    { "UseExternalGraphicSignature", "false" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraphicSignatureCert");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraphicSignaturePin");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ExternalGraphicSignatureSigner1Url");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "UseExternalGraphicSignature");

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "ExternalGraficSignatureCert", "" },
                    { "ExternalGraficSignaturePin", "" },
                    { "ExternalGraficSignatureSigner1Url", "" },
                    { "UseExternalGraficSignature", "false" }
                });
        }
    }
}
