using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addEnableSigner1ExtraSigningTypesToConfigTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "EnableSigner1ExtraSigningTypes", "false" });

            migrationBuilder.DropTable(
                name: "Logs");
            migrationBuilder.DropTable(
                name: "SignerLogs");
            migrationBuilder.DropTable(
                name: "ManagementLogs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "EnableSigner1ExtraSigningTypes");
        }
    }
}
