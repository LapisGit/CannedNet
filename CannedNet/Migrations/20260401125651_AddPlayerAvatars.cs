using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CannedNet.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerAvatars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avatar_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AvatarItemType = table.Column<int>(type: "integer", nullable: false),
                    AvatarItemDesc = table.Column<string>(type: "text", nullable: false),
                    PlatformMask = table.Column<int>(type: "integer", nullable: false),
                    FriendlyName = table.Column<string>(type: "text", nullable: false),
                    Tooltip = table.Column<string>(type: "text", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    OwnerAccountId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avatar_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "player_avatars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerAccountId = table.Column<int>(type: "integer", nullable: false),
                    OutfitSelections = table.Column<string>(type: "text", nullable: false),
                    FaceFeatures = table.Column<string>(type: "text", nullable: false),
                    SkinColor = table.Column<string>(type: "text", nullable: false),
                    HairColor = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_avatars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avatar_items_OwnerAccountId",
                table: "avatar_items",
                column: "OwnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_player_avatars_OwnerAccountId",
                table: "player_avatars",
                column: "OwnerAccountId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avatar_items");

            migrationBuilder.DropTable(
                name: "player_avatars");
        }
    }
}
