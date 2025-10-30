using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddHebrewMessagesToCompanyMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Language",
                table: "CompanyMessages",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "MessageBeforeHebrew", "[DOCUMENT_NAME] : [LINK]" });

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "MessageAfterHebrew", "[DOCUMENT_NAME] נחתם בהצלחה. [LINK]" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "MessageAfterHebrew");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "MessageBeforeHebrew");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "CompanyMessages");
        }
    }
}
