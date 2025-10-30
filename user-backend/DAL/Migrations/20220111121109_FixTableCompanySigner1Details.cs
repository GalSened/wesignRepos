using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class FixTableCompanySigner1Details : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySigner1Details_CompanyConfigurations_CompanyConfigurationId",
                table: "CompanySigner1Details");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanySigner1Details",
                table: "CompanySigner1Details");

            migrationBuilder.DropColumn(
                name: "CompanyConfigurationId",
                table: "CompanySigner1Details");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "CompanySigner1Details",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanySigner1Details",
                table: "CompanySigner1Details",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySigner1Details_Companies_CompanyId",
                table: "CompanySigner1Details",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CompanySigner1Details_Companies_CompanyId",
                table: "CompanySigner1Details");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CompanySigner1Details",
                table: "CompanySigner1Details");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CompanySigner1Details");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyConfigurationId",
                table: "CompanySigner1Details",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompanySigner1Details",
                table: "CompanySigner1Details",
                column: "CompanyConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_CompanySigner1Details_CompanyConfigurations_CompanyConfigurationId",
                table: "CompanySigner1Details",
                column: "CompanyConfigurationId",
                principalTable: "CompanyConfigurations",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
