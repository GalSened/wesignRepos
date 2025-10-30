using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class RenameActiveDirectoryGroups : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDirectoryGroupDAO_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroupDAO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveDirectoryGroupDAO",
                table: "ActiveDirectoryGroupDAO");

            migrationBuilder.RenameTable(
                name: "ActiveDirectoryGroupDAO",
                newName: "ActiveDirectoryGroups");

            migrationBuilder.RenameIndex(
                name: "IX_ActiveDirectoryGroupDAO_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups",
                newName: "IX_ActiveDirectoryGroups_ActiveDirectoryConfigId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveDirectoryGroups",
                table: "ActiveDirectoryGroups",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDirectoryGroups_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups",
                column: "ActiveDirectoryConfigId",
                principalTable: "ActiveDirectoryConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDirectoryGroups_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ActiveDirectoryGroups",
                table: "ActiveDirectoryGroups");

            migrationBuilder.RenameTable(
                name: "ActiveDirectoryGroups",
                newName: "ActiveDirectoryGroupDAO");

            migrationBuilder.RenameIndex(
                name: "IX_ActiveDirectoryGroups_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroupDAO",
                newName: "IX_ActiveDirectoryGroupDAO_ActiveDirectoryConfigId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ActiveDirectoryGroupDAO",
                table: "ActiveDirectoryGroupDAO",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDirectoryGroupDAO_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroupDAO",
                column: "ActiveDirectoryConfigId",
                principalTable: "ActiveDirectoryConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
