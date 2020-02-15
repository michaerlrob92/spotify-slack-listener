using Microsoft.EntityFrameworkCore.Migrations;

namespace SpotifySlackListener.Infrastructure.Migrations
{
    public partial class AddCurrentStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "Users");
        }
    }
}
