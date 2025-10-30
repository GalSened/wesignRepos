using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HistoryIntegratorService.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplateId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "TemplateSignatureFields",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TemplateId",
                table: "Templates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "TemplateSignatureFields");

            migrationBuilder.DropColumn(
                name: "TemplateId",
                table: "Templates");
        }
    }
}
