using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class fixAdditionalGroupMapperDAOIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdditionalGroupsMapper_CompanyId",
                table: "AdditionalGroupsMapper");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalGroupsMapper_GroupId",
                table: "AdditionalGroupsMapper");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGroupsMapper_CompanyId",
                table: "AdditionalGroupsMapper",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGroupsMapper_GroupId",
                table: "AdditionalGroupsMapper",
                column: "GroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdditionalGroupsMapper_CompanyId",
                table: "AdditionalGroupsMapper");

            migrationBuilder.DropIndex(
                name: "IX_AdditionalGroupsMapper_GroupId",
                table: "AdditionalGroupsMapper");

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGroupsMapper_CompanyId",
                table: "AdditionalGroupsMapper",
                column: "CompanyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdditionalGroupsMapper_GroupId",
                table: "AdditionalGroupsMapper",
                column: "GroupId",
                unique: true);
        }
    }
}
