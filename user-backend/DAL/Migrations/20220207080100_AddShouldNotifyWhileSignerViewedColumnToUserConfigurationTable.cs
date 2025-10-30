using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddShouldNotifyWhileSignerViewedColumnToUserConfigurationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "shouldNotifyWhileSignerViewed",
                table: "UserConfigurations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "shouldNotifyWhileSignerViewed",
                table: "UserConfigurations");
        }
    }
}
