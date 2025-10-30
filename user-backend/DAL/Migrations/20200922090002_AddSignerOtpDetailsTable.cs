using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddSignerOtpDetailsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignerOtpDetailsDAO",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SignerId = table.Column<Guid>(nullable: false),
                    Identification = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    ExpirationTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignerOtpDetailsDAO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignerOtpDetailsDAO_Signers_SignerId",
                        column: x => x.SignerId,
                        principalTable: "Signers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SignerOtpDetailsDAO_SignerId",
                table: "SignerOtpDetailsDAO",
                column: "SignerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignerOtpDetailsDAO");
        }
    }
}
