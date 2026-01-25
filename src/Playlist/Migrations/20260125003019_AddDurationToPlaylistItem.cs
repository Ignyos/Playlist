using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Playlist.Migrations
{
    /// <inheritdoc />
    public partial class AddDurationToPlaylistItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Duration",
                table: "PlaylistItems",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "PlaylistItems");
        }
    }
}
