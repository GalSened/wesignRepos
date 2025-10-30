using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class CompanyConfigurationOptionToChoosePersonalizedPFX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isPersonalizedPFX",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isPersonalizedPFX",
                table: "CompanyConfigurations");
        }
    }
}
