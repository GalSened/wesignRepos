using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddNotifyCountsToProgramUtilizationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentsSentNotifyCount",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmsSentNotifyCount",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentsSentNotifyCount",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "SmsSentNotifyCount",
                table: "ProgramUtilizations");
        }
    }
}
