using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class setHistoryIntegratorServiceAPIKeyEmptyDefaultValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "HistoryIntegratorServiceAPIKey",
                column: "Value",
                value: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Configuration",
                keyColumn: "Key",
                keyValue: "HistoryIntegratorServiceAPIKey",
                column: "Value",
                value: "Wp3f73RiRNQwyMGmFQyYZDYe/v8qQvB0dm8qwKZLcseDmFEjYdX/MPZAc467oolJTpWbER01yzvILRNs75p31Zfzf4Z8B8kQ/pjjQNUR8Os=");
        }
    }
}
