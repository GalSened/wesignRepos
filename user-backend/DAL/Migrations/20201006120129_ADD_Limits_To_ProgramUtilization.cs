using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class ADD_Limits_To_ProgramUtilization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Documents",
                table: "ProgramUtilizations");

            migrationBuilder.AddColumn<long>(
                name: "DocumentsLimit",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "DocumentsUsage",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "ProgramResetType",
                table: "ProgramUtilizations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentsLimit",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "DocumentsUsage",
                table: "ProgramUtilizations");

            migrationBuilder.DropColumn(
                name: "ProgramResetType",
                table: "ProgramUtilizations");

            migrationBuilder.AddColumn<long>(
                name: "Documents",
                table: "ProgramUtilizations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
