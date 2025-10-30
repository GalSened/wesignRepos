using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class renamePhoneExtentionToPhoneExtension : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneExtention",
                table: "Contacts");

            migrationBuilder.AddColumn<string>(
                name: "PhoneExtension",
                table: "Contacts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneExtension",
                table: "Contacts");

            migrationBuilder.AddColumn<string>(
                name: "PhoneExtention",
                table: "Contacts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
