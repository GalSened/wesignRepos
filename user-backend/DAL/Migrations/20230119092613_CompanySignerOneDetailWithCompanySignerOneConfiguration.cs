using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class CompanySignerOneDetailWithCompanySignerOneConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Signer1Endpoint",
                table: "CompanySigner1Details",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signer1Password",
                table: "CompanySigner1Details",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Signer1User",
                table: "CompanySigner1Details",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Signer1Endpoint",
                table: "CompanySigner1Details");

            migrationBuilder.DropColumn(
                name: "Signer1Password",
                table: "CompanySigner1Details");

            migrationBuilder.DropColumn(
                name: "Signer1User",
                table: "CompanySigner1Details");
        }
    }
}
