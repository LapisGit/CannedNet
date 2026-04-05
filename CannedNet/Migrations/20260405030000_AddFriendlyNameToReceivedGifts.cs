using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CannedNet.Migrations
{
    /// <inheritdoc />
    public partial class AddFriendlyNameToReceivedGifts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FriendlyName",
                table: "received_gifts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FriendlyName",
                table: "received_gifts");
        }
    }
}

