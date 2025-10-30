using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class addShouldSignUsingSigner1AfterDocumentSigningFlowToDocumentCollectionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldSignUsingSigner1AfterDocumentSigningFlow",
                table: "DocumentCollections",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldSignUsingSigner1AfterDocumentSigningFlow",
                table: "DocumentCollections");
        }
    }
}
