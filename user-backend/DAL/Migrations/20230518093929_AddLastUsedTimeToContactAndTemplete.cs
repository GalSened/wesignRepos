using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DAL.Migrations
{
    public partial class AddLastUsedTimeToContactAndTemplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedTime",
                table: "Templates",
                nullable: false,
                defaultValueSql: "GETDATE()")
            ; 

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedTime",
                table: "Contacts",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.Sql(
                sql: "update Contacts set LastUsedTime = GETDATE()");

            migrationBuilder.Sql(
                sql: "update Templates set LastUsedTime = GETDATE()");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUsedTime",
                table: "Templates");

            migrationBuilder.DropColumn(
                name: "LastUsedTime",
                table: "Contacts");
        }
    }
}
