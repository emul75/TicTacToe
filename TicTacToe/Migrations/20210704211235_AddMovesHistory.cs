using Microsoft.EntityFrameworkCore.Migrations;

namespace TicTacToe.Migrations
{
    public partial class AddMovesHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MovesHistory",
                table: "Boards",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovesHistory",
                table: "Boards");
        }
    }
}
