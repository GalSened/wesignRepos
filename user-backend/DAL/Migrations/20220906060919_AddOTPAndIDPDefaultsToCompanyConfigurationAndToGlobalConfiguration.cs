using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddOTPAndIDPDefaultsToCompanyConfigurationAndToGlobalConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableComsignIDP",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldSendWithComsignIDPByDefault",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldSendWithOTPByDefault",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "SendWithOTPByDefault", "false" });

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "EnableComsignIDP", "false" });

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "SendWithComsignIDPByDefault", "false" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "EnableComsignIDP");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "SendWithComsignIDPByDefault");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "SendWithOTPByDefault");

            migrationBuilder.DropColumn(
                name: "EnableComsignIDP",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldSendWithComsignIDPByDefault",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldSendWithOTPByDefault",
                table: "CompanyConfigurations");
        }
    }
}
