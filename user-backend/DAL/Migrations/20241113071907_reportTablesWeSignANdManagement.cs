using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class reportTablesWeSignANdManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ManagementPeriodicReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportType = table.Column<int>(type: "int", nullable: false),
                    LastTimeSent = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportFrequency = table.Column<int>(type: "int", nullable: false),
                    ReportParameters = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementPeriodicReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementPeriodicReport_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodicReportFile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTIme = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodicReportFile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagementPeriodicReportEmail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PeriodicReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementPeriodicReportEmail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementPeriodicReportEmail_ManagementPeriodicReport_ReportId",
                        column: x => x.ReportId,
                        principalTable: "ManagementPeriodicReport",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementPeriodicReport_UserId",
                table: "ManagementPeriodicReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementPeriodicReportEmail_ReportId",
                table: "ManagementPeriodicReportEmail",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementPeriodicReportEmail");

            migrationBuilder.DropTable(
                name: "PeriodicReportFile");

            migrationBuilder.DropTable(
                name: "ManagementPeriodicReport");
        }
    }
}
