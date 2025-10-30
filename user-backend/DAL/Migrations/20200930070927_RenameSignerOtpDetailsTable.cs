using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class RenameSignerOtpDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignerOtpDetailsDAO_Signers_SignerId",
                table: "SignerOtpDetailsDAO");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignerOtpDetailsDAO",
                table: "SignerOtpDetailsDAO");

            migrationBuilder.RenameTable(
                name: "SignerOtpDetailsDAO",
                newName: "SignerOtpDetails");

            migrationBuilder.RenameIndex(
                name: "IX_SignerOtpDetailsDAO_SignerId",
                table: "SignerOtpDetails",
                newName: "IX_SignerOtpDetails_SignerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignerOtpDetails",
                table: "SignerOtpDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SignerOtpDetails_Signers_SignerId",
                table: "SignerOtpDetails",
                column: "SignerId",
                principalTable: "Signers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SignerOtpDetails_Signers_SignerId",
                table: "SignerOtpDetails");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignerOtpDetails",
                table: "SignerOtpDetails");

            migrationBuilder.RenameTable(
                name: "SignerOtpDetails",
                newName: "SignerOtpDetailsDAO");

            migrationBuilder.RenameIndex(
                name: "IX_SignerOtpDetails_SignerId",
                table: "SignerOtpDetailsDAO",
                newName: "IX_SignerOtpDetailsDAO_SignerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignerOtpDetailsDAO",
                table: "SignerOtpDetailsDAO",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SignerOtpDetailsDAO_Signers_SignerId",
                table: "SignerOtpDetailsDAO",
                column: "SignerId",
                principalTable: "Signers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
