using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class UpdateSignerUserAndCompanyConfigurationsForSignReminders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldNotifySignReminder",
                table: "UserConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SignReminderFrequencyInDays",
                table: "UserConfigurations",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeLastSent",
                table: "Signers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "CanUserControlReminderSettings",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldEnableSignReminders",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SignReminderFrequencyInDays",
                table: "CompanyConfigurations",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldNotifySignReminder",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "SignReminderFrequencyInDays",
                table: "UserConfigurations");

            migrationBuilder.DropColumn(
                name: "TimeLastSent",
                table: "Signers");

            migrationBuilder.DropColumn(
                name: "CanUserControlReminderSettings",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldEnableSignReminders",
                table: "CompanyConfigurations");

            migrationBuilder.DropColumn(
                name: "SignReminderFrequencyInDays",
                table: "CompanyConfigurations");
        }
    }
}
