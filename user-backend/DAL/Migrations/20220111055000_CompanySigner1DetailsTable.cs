using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class CompanySigner1DetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySigner1Details",
                columns: table => new
                {
                    CompanyConfigurationId = table.Column<Guid>(nullable: false),
                    Key1 = table.Column<string>(nullable: true),
                    Key2 = table.Column<string>(nullable: true),
                    ShouldSignAsDefaultValue = table.Column<bool>(nullable: false),
                    ShouldShowInUI = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySigner1Details", x => x.CompanyConfigurationId);
                    table.ForeignKey(
                        name: "FK_CompanySigner1Details_CompanyConfigurations_CompanyConfigurationId",
                        column: x => x.CompanyConfigurationId,
                        principalTable: "CompanyConfigurations",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanySigner1Details");
        }
    }
}
