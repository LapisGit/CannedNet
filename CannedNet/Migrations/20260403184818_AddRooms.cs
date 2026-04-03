using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CannedNet.Migrations
{
    /// <inheritdoc />
    public partial class AddRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatorAccountId = table.Column<int>(type: "integer", nullable: false),
                    ImageName = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Accessibility = table.Column<int>(type: "integer", nullable: false),
                    SupportsLevelVoting = table.Column<bool>(type: "boolean", nullable: false),
                    IsRRO = table.Column<bool>(type: "boolean", nullable: false),
                    IsDorm = table.Column<bool>(type: "boolean", nullable: false),
                    CloningAllowed = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsVRLow = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsQuest2 = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsMobile = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsScreens = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsWalkVR = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsTeleportVR = table.Column<bool>(type: "boolean", nullable: false),
                    SupportsJuniors = table.Column<bool>(type: "boolean", nullable: false),
                    MinLevel = table.Column<int>(type: "integer", nullable: false),
                    WarningMask = table.Column<int>(type: "integer", nullable: false),
                    CustomWarning = table.Column<string>(type: "text", nullable: true),
                    DisableMicAutoMute = table.Column<bool>(type: "boolean", nullable: false),
                    DisableRoomComments = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptVoiceChat = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sub_rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    SubRoomId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DataBlob = table.Column<string>(type: "text", nullable: false),
                    IsSandbox = table.Column<bool>(type: "boolean", nullable: false),
                    MaxPlayers = table.Column<int>(type: "integer", nullable: false),
                    Accessibility = table.Column<int>(type: "integer", nullable: false),
                    UnitySceneId = table.Column<string>(type: "text", nullable: false),
                    DataSavedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sub_rooms", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rooms_CreatorAccountId",
                table: "rooms",
                column: "CreatorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_sub_rooms_RoomId",
                table: "sub_rooms",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "sub_rooms");
        }
    }
}
