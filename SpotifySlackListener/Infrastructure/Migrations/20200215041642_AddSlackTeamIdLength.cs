using Microsoft.EntityFrameworkCore.Migrations;

namespace SpotifySlackListener.Infrastructure.Migrations
{
    public partial class AddSlackTeamIdLength : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SlackTeamId",
                table: "Users",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlackTeamId",
                table: "Users");
        }
    }
}
