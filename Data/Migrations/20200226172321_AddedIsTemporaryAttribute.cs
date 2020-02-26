using Microsoft.EntityFrameworkCore.Migrations;

namespace QuizWebApp.Migrations
{
    public partial class AddedIsTemporaryAttribute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isTemporary",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isTemporary",
                table: "AspNetUsers");
        }
    }
}
