using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace EventHost.Web.Migrations
{
    public partial class SessionNonUserHost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_HostUserId",
                table: "Sessions");

            migrationBuilder.AlterColumn<int>(
                name: "HostUserId",
                table: "Sessions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<string>(
                name: "HostName",
                table: "Sessions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_HostUserId",
                table: "Sessions",
                column: "HostUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Users_HostUserId",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "HostName",
                table: "Sessions");

            migrationBuilder.AlterColumn<int>(
                name: "HostUserId",
                table: "Sessions",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Users_HostUserId",
                table: "Sessions",
                column: "HostUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
