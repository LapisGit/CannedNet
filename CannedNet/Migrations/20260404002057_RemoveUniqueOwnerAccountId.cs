using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CannedNet.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueOwnerAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_room_instances_OwnerAccountId",
                table: "room_instances");

            migrationBuilder.CreateIndex(
                name: "IX_room_instances_OwnerAccountId_roomId",
                table: "room_instances",
                columns: new[] { "OwnerAccountId", "roomId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_room_instances_OwnerAccountId_roomId",
                table: "room_instances");

            migrationBuilder.CreateIndex(
                name: "IX_room_instances_OwnerAccountId",
                table: "room_instances",
                column: "OwnerAccountId",
                unique: true);
        }
    }
}
