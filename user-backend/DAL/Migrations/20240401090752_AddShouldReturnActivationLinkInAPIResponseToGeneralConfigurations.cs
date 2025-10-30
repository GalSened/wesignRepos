using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddShouldReturnActivationLinkInAPIResponseToGeneralConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[] { "ShouldReturnActivationLinkInAPIResponse", "false" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "ShouldReturnActivationLinkInAPIResponse");
        }
    }
}
