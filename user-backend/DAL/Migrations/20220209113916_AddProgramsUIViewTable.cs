using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddProgramsUIViewTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldShowContacts",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowGroupSign",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowLiveMode",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "ShouldShowSelfSign",
                table: "Programs");

            migrationBuilder.CreateTable(
                name: "ProgramsUIView",
                columns: table => new
                {
                    ProgramId = table.Column<Guid>(nullable: false),
                    ShouldShowSelfSign = table.Column<bool>(nullable: false),
                    ShouldShowGroupSign = table.Column<bool>(nullable: false),
                    ShouldShowLiveMode = table.Column<bool>(nullable: false),
                    ShouldShowContacts = table.Column<bool>(nullable: false),
                    ShouldShowTemplates = table.Column<bool>(nullable: false),
                    ShouldShowAddNewTemplate = table.Column<bool>(nullable: false),
                    ShouldShowEditTextField = table.Column<bool>(nullable: false),
                    ShouldShowEditSignatureField = table.Column<bool>(nullable: false),
                    ShouldShowEditEmailField = table.Column<bool>(nullable: false),
                    ShouldShowEditPhoneField = table.Column<bool>(nullable: false),
                    ShouldShowEditDateField = table.Column<bool>(nullable: false),
                    ShouldShowEditNumberField = table.Column<bool>(nullable: false),
                    ShouldShowEditListField = table.Column<bool>(nullable: false),
                    ShouldShowEditCheckboxField = table.Column<bool>(nullable: false),
                    ShouldShowEditRadioField = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramsUIView", x => x.ProgramId);
                    table.ForeignKey(
                        name: "FK_ProgramsUIView_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramsUIView");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowContacts",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowGroupSign",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowLiveMode",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowSelfSign",
                table: "Programs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
