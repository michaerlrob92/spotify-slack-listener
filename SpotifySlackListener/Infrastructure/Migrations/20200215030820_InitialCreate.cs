using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SpotifySlackListener.Infrastructure.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 128, nullable: false),
                    SlackId = table.Column<string>(maxLength: 256, nullable: false),
                    SlackAccessToken = table.Column<string>(maxLength: 256, nullable: false),
                    SpotifyId = table.Column<string>(maxLength: 256, nullable: false),
                    SpotifyAccessToken = table.Column<string>(maxLength: 256, nullable: false),
                    SpotifyRefreshToken = table.Column<string>(maxLength: 256, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpotifyAccessToken_SlackAccessToken",
                table: "Users",
                columns: new[] { "SpotifyAccessToken", "SlackAccessToken" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SpotifyId_SlackId",
                table: "Users",
                columns: new[] { "SpotifyId", "SlackId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
