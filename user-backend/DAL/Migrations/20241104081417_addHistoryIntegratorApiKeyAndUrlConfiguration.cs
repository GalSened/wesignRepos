using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class addHistoryIntegratorApiKeyAndUrlConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Configuration",
                columns: new[] { "Key", "Value" },
                values: new object[,]
                {
                    { "HistoryIntegratorServiceAPIKey", "Wp3f73RiRNQwyMGmFQyYZDYe/v8qQvB0dm8qwKZLcseDmFEjYdX/MPZAc467oolJTpWbER01yzvILRNs75p31Zfzf4Z8B8kQ/pjjQNUR8Os=" },
                    { "HistoryIntegratorServiceURL", "" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "HistoryIntegratorServiceAPIKey");

            migrationBuilder.DeleteData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "HistoryIntegratorServiceURL");
        }
    }
}
