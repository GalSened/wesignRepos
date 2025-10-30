using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddActiveDirectory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreationSource",
                table: "Users",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CreationSource",
                table: "Contacts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    Domain = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveDirectoryConfigurations_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveDirectoryGroupDAO",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    GroupType = table.Column<int>(nullable: false),
                    ActiveDirectoryConfigId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveDirectoryGroupDAO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActiveDirectoryGroupDAO_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                        column: x => x.ActiveDirectoryConfigId,
                        principalTable: "ActiveDirectoryConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "LogArichveIntervalInDays",
                column: "Value",
                value: "30");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryConfigurations_CompanyId",
                table: "ActiveDirectoryConfigurations",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryGroupDAO_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroupDAO",
                column: "ActiveDirectoryConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveDirectoryGroupDAO");

            migrationBuilder.DropTable(
                name: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "CreationSource",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreationSource",
                table: "Contacts");

            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "LogArichveIntervalInDays",
                column: "Value",
                value: "14");
        }
    }
}
