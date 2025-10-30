using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class useManagementOtpAuthRename : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "UseOtpAuthentication");

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "UseManagementOtpAuth", "false" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "UseManagementOtpAuth");

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "UseOtpAuthentication", "false" });
        }
    }
}
