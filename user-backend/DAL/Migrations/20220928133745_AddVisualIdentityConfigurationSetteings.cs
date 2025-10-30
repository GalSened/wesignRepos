using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddVisualIdentityConfigurationSetteings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "EnableComsignIDP");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "SendWithComsignIDPByDefault");

            migrationBuilder.DropColumn(
                name: "EnableComsignIDP",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldSendWithComsignIDPByDefault",
                table: "CompanyConfigurations");

            migrationBuilder.AddColumn<bool>(
                name: "EnableVisualIdentityFlow",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "EnableVisualIdentityFlow", "false" },
                    { "EnableRenewalPayingUserLogic", "false" },
                    { "VisualIdentityURL", "" },
                    { "VisualIdentityUser", "" },
                    { "VisualIdentityPassword", "" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "EnableRenewalPayingUserLogic");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "EnableVisualIdentityFlow");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "VisualIdentityPassword");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "VisualIdentityURL");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "VisualIdentityUser");

            migrationBuilder.DropColumn(
                name: "EnableVisualIdentityFlow",
                table: "CompanyConfigurations");

            migrationBuilder.AddColumn<bool>(
                name: "EnableComsignIDP",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldSendWithComsignIDPByDefault",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "EnableComsignIDP", "false" });

            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "SendWithComsignIDPByDefault", "false" });
        }
    }
}
