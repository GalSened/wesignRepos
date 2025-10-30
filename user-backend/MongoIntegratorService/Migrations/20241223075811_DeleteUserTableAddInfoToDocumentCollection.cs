using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoryIntegratorService.Migrations
{
    /// <inheritdoc />
    public partial class DeleteUserTableAddInfoToDocumentCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeletedDocumentCollections_Users_UserId",
                table: "DeletedDocumentCollections");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_DeletedDocumentCollections_UserId",
                table: "DeletedDocumentCollections");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "DeletedDocumentCollections",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "DeletedDocumentCollections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                table: "DeletedDocumentCollections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "DeletedDocumentCollections");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "DeletedDocumentCollections");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                table: "DeletedDocumentCollections");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeletedDocumentCollections_UserId",
                table: "DeletedDocumentCollections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeletedDocumentCollections_Users_UserId",
                table: "DeletedDocumentCollections",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
