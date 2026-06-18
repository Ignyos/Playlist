using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Playlist.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistQueueState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "Playlists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "QueueOrder",
                table: "Playlists",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "QueueOrder",
                table: "Playlists");
        }
    }
}
