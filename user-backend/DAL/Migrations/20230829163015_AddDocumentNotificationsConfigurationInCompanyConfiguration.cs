using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentNotificationsConfigurationInCompanyConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentNotificationsEndpoint",
                table: "CompanyConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldSendDocumentNotifications",
                table: "CompanyConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentNotificationsEndpoint",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldSendDocumentNotifications",
                table: "CompanyConfigurations");
        }
    }
}
