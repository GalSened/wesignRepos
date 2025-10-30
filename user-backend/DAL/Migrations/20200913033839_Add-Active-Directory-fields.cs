using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddActiveDirectoryfields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDirectoryConfigurations_Companies_CompanyId",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDirectoryGroups_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropIndex(
                name: "IX_ActiveDirectoryGroups_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropIndex(
                name: "IX_ActiveDirectoryConfigurations_CompanyId",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "GroupType",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveDirectoryConfigId",
                table: "Companies",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiveDirectoryContactsGroupName",
                table: "ActiveDirectoryGroups",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActiveDirectoryUsersGroupName",
                table: "ActiveDirectoryGroups",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "ActiveDirectoryGroups",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Container",
                table: "ActiveDirectoryConfigurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Host",
                table: "ActiveDirectoryConfigurations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "ActiveDirectoryConfigurations",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Port",
                table: "ActiveDirectoryConfigurations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "ActiveDirectoryConfigurations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ProgramUtilizationId",
                table: "Users",
                column: "ProgramUtilizationId",
                unique: true,
                filter: "[ProgramUtilizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SignerTokensMapping_DocumentCollectionId",
                table: "SignerTokensMapping",
                column: "DocumentCollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentSignatureFields_DocumentId",
                table: "DocumentSignatureFields",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ActiveDirectoryConfigId",
                table: "Companies",
                column: "ActiveDirectoryConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_ProgramUtilizationId",
                table: "Companies",
                column: "ProgramUtilizationId",
                unique: true,
                filter: "[ProgramUtilizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryGroups_GroupId",
                table: "ActiveDirectoryGroups",
                column: "GroupId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDirectoryGroups_Groups_GroupId",
                table: "ActiveDirectoryGroups",
                column: "GroupId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "Companies",
                column: "ActiveDirectoryConfigId",
                principalTable: "ActiveDirectoryConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_ProgramUtilizations_ProgramUtilizationId",
                table: "Companies",
                column: "ProgramUtilizationId",
                principalTable: "ProgramUtilizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentSignatureFields_Documents_DocumentId",
                table: "DocumentSignatureFields",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SignerTokensMapping_DocumentCollections_DocumentCollectionId",
                table: "SignerTokensMapping",
                column: "DocumentCollectionId",
                principalTable: "DocumentCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ProgramUtilizations_ProgramUtilizationId",
                table: "Users",
                column: "ProgramUtilizationId",
                principalTable: "ProgramUtilizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ActiveDirectoryGroups_Groups_GroupId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_Companies_ProgramUtilizations_ProgramUtilizationId",
                table: "Companies");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentSignatureFields_Documents_DocumentId",
                table: "DocumentSignatureFields");

            migrationBuilder.DropForeignKey(
                name: "FK_SignerTokensMapping_DocumentCollections_DocumentCollectionId",
                table: "SignerTokensMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_ProgramUtilizations_ProgramUtilizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ProgramUtilizationId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SignerTokensMapping_DocumentCollectionId",
                table: "SignerTokensMapping");

            migrationBuilder.DropIndex(
                name: "IX_DocumentSignatureFields_DocumentId",
                table: "DocumentSignatureFields");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ActiveDirectoryConfigId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_ProgramUtilizationId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_ActiveDirectoryGroups_GroupId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "ActiveDirectoryConfigId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "ActiveDirectoryContactsGroupName",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "ActiveDirectoryUsersGroupName",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "ActiveDirectoryGroups");

            migrationBuilder.DropColumn(
                name: "Container",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "Host",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "Port",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.DropColumn(
                name: "User",
                table: "ActiveDirectoryConfigurations");

            migrationBuilder.AddColumn<Guid>(
                name: "ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "GroupType",
                table: "ActiveDirectoryGroups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ActiveDirectoryGroups",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "ActiveDirectoryConfigurations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryGroups_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups",
                column: "ActiveDirectoryConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveDirectoryConfigurations_CompanyId",
                table: "ActiveDirectoryConfigurations",
                column: "CompanyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDirectoryConfigurations_Companies_CompanyId",
                table: "ActiveDirectoryConfigurations",
                column: "CompanyId",
                principalTable: "Companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ActiveDirectoryGroups_ActiveDirectoryConfigurations_ActiveDirectoryConfigId",
                table: "ActiveDirectoryGroups",
                column: "ActiveDirectoryConfigId",
                principalTable: "ActiveDirectoryConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
