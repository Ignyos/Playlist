using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Playlist.Migrations
{
    public partial class AddPlaylistPlaybackMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlaybackMode",
                table: "Playlists",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaybackMode",
                table: "Playlists");
        }
    }
}
