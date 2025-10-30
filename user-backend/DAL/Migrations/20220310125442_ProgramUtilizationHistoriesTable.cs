using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class ProgramUtilizationHistoriesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProgramUtilizationHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CompanyId = table.Column<Guid>(nullable: false),
                    UpdateDate = table.Column<DateTime>(nullable: false),
                    DocumentsUsage = table.Column<long>(nullable: false),
                    SmsUsage = table.Column<long>(nullable: false),
                    TemplatesUsage = table.Column<long>(nullable: false),
                    UsersUsage = table.Column<long>(nullable: false),
                    Expired = table.Column<DateTime>(nullable: false),
                    ResourceMode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramUtilizationHistories", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgramUtilizationHistories");
        }
    }
}
