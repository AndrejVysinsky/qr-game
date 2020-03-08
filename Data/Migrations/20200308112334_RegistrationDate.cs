using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizWebApp.Migrations
{
    public partial class RegistrationDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isActive",
                table: "Contests",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "isTemporary",
                table: "AspNetUsers",
                newName: "IsTemporary");

            migrationBuilder.AddColumn<DateTime>(
                name: "RegistrationDate",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegistrationDate",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "Contests",
                newName: "isActive");

            migrationBuilder.RenameColumn(
                name: "IsTemporary",
                table: "AspNetUsers",
                newName: "isTemporary");
        }
    }
}
