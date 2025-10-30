using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddParamToProgramUIViewTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowDocuments",
                table: "ProgramsUIView",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowProfile",
                table: "ProgramsUIView",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldShowUploadAndsign",
                table: "ProgramsUIView",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                sql: "INSERT INTO ProgramsUIView SELECT Id as ProgramId, 1, 1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1 FROM Programs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldShowDocuments",
                table: "ProgramsUIView");

            migrationBuilder.DropColumn(
                name: "ShouldShowProfile",
                table: "ProgramsUIView");

            migrationBuilder.DropColumn(
                name: "ShouldShowUploadAndsign",
                table: "ProgramsUIView");
        }
    }
}
