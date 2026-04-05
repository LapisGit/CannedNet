using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CannedNet.Migrations
{
    /// <inheritdoc />
    public partial class InitalDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    ProfileImage = table.Column<string>(type: "text", nullable: true),
                    IsJunior = table.Column<bool>(type: "boolean", nullable: false),
                    Platforms = table.Column<int>(type: "integer", nullable: true),
                    PersonalPronouns = table.Column<int>(type: "integer", nullable: true),
                    IdentityFlags = table.Column<int>(type: "integer", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: false),
                    DisplayName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accounts", x => x.AccountId);
                });

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
                name: "cached_logins",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    PlatformID = table.Column<string>(type: "text", nullable: false),
                    LastLoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequirePassword = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cached_logins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "consumable_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerAccountId = table.Column<int>(type: "integer", nullable: false),
                    Ids = table.Column<List<int>>(type: "integer[]", nullable: false),
                    CreatedAts = table.Column<List<DateTime>>(type: "timestamp with time zone[]", nullable: false),
                    ConsumableItemDesc = table.Column<string>(type: "text", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    InitialCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    IsTransferable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumable_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "earnable_rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardContext = table.Column<int>(type: "integer", nullable: false),
                    ConsumableItemDesc = table.Column<string>(type: "text", nullable: false),
                    AvatarItemDesc = table.Column<string>(type: "text", nullable: false),
                    AvatarItemType = table.Column<int>(type: "integer", nullable: false),
                    GiftRarity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_earnable_rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "load_screens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Tooltip = table.Column<string>(type: "text", nullable: false),
                    IsThumbnail = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_load_screens", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "player_bios",
                columns: table => new
                {
                    accountId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_bios", x => x.accountId);
                });

            migrationBuilder.CreateTable(
                name: "player_progressions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_progressions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "player_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<int>(type: "integer", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promo_external_contents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Tooltip = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_external_contents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promo_images",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    Tooltip = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promo_images", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "received_gifts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceiverAccountId = table.Column<int>(type: "integer", nullable: false),
                    FromPlayerId = table.Column<int>(type: "integer", nullable: true),
                    ConsumableItemDesc = table.Column<string>(type: "text", nullable: false),
                    AvatarItemDesc = table.Column<string>(type: "text", nullable: false),
                    AvatarItemType = table.Column<int>(type: "integer", nullable: false),
                    EquipmentPrefabName = table.Column<string>(type: "text", nullable: false),
                    EquipmentModificationGuid = table.Column<string>(type: "text", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false),
                    PlatformsToSpawnOn = table.Column<int>(type: "integer", nullable: false),
                    BalanceType = table.Column<int>(type: "integer", nullable: false),
                    GiftContext = table.Column<int>(type: "integer", nullable: false),
                    GiftRarity = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsConsumed = table.Column<bool>(type: "boolean", nullable: false),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_received_gifts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "room_instances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerAccountId = table.Column<int>(type: "integer", nullable: false),
                    roomInstanceId = table.Column<int>(type: "integer", nullable: false),
                    roomId = table.Column<int>(type: "integer", nullable: false),
                    subRoomId = table.Column<int>(type: "integer", nullable: false),
                    roomInstanceType = table.Column<int>(type: "integer", nullable: false),
                    location = table.Column<string>(type: "text", nullable: false),
                    dataBlob = table.Column<string>(type: "text", nullable: false),
                    eventId = table.Column<int>(type: "integer", nullable: false),
                    clubId = table.Column<int>(type: "integer", nullable: false),
                    roomCode = table.Column<string>(type: "text", nullable: false),
                    photonRegionId = table.Column<string>(type: "text", nullable: false),
                    photonRoomId = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    maxCapacity = table.Column<int>(type: "integer", nullable: false),
                    isFull = table.Column<bool>(type: "boolean", nullable: false),
                    isPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    isInProgress = table.Column<bool>(type: "boolean", nullable: false),
                    EncryptVoiceChat = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_instances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "room_roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoomId = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    InvitedRole = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_roles", x => x.Id);
                });

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
                name: "saved_outfits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerAccountId = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    PreviewImageName = table.Column<string>(type: "text", nullable: false),
                    OutfitSelections = table.Column<string>(type: "text", nullable: false),
                    HairColor = table.Column<string>(type: "text", nullable: false),
                    SkinColor = table.Column<string>(type: "text", nullable: false),
                    FaceFeatures = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saved_outfits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "storefronts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StorefrontType = table.Column<int>(type: "integer", nullable: false),
                    NextUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storefronts", x => x.Id);
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

            migrationBuilder.CreateTable(
                name: "token_balances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Balance = table.Column<int>(type: "integer", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    BalanceType = table.Column<int>(type: "integer", nullable: false),
                    Platform = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_token_balances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "storefront_items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StorefrontId = table.Column<int>(type: "integer", nullable: false),
                    PurchasableItemId = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    NewUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storefront_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storefront_items_storefronts_StorefrontId",
                        column: x => x.StorefrontId,
                        principalTable: "storefronts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gift_drops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StorefrontItemId = table.Column<int>(type: "integer", nullable: false),
                    GiftDropId = table.Column<int>(type: "integer", nullable: false),
                    FriendlyName = table.Column<string>(type: "text", nullable: false),
                    Tooltip = table.Column<string>(type: "text", nullable: true),
                    ConsumableItemDesc = table.Column<string>(type: "text", nullable: true),
                    AvatarItemDesc = table.Column<string>(type: "text", nullable: true),
                    AvatarItemType = table.Column<int>(type: "integer", nullable: false),
                    EquipmentPrefabName = table.Column<string>(type: "text", nullable: true),
                    EquipmentModificationGuid = table.Column<string>(type: "text", nullable: true),
                    IsQuery = table.Column<bool>(type: "boolean", nullable: false),
                    Unique = table.Column<bool>(type: "boolean", nullable: false),
                    SubscribersOnly = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Rarity = table.Column<int>(type: "integer", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<int>(type: "integer", nullable: false),
                    Context = table.Column<int>(type: "integer", nullable: false),
                    ItemSetId = table.Column<int>(type: "integer", nullable: true),
                    ItemSetFriendlyName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gift_drops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gift_drops_storefront_items_StorefrontItemId",
                        column: x => x.StorefrontItemId,
                        principalTable: "storefront_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "storefront_prices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StorefrontItemId = table.Column<int>(type: "integer", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storefront_prices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_storefront_prices_storefront_items_StorefrontItemId",
                        column: x => x.StorefrontItemId,
                        principalTable: "storefront_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avatar_items_OwnerAccountId",
                table: "avatar_items",
                column: "OwnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_cached_logins_Platform_PlatformID",
                table: "cached_logins",
                columns: new[] { "Platform", "PlatformID" });

            migrationBuilder.CreateIndex(
                name: "IX_consumable_items_OwnerAccountId",
                table: "consumable_items",
                column: "OwnerAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_earnable_rewards_RewardContext",
                table: "earnable_rewards",
                column: "RewardContext");

            migrationBuilder.CreateIndex(
                name: "IX_gift_drops_StorefrontItemId",
                table: "gift_drops",
                column: "StorefrontItemId");

            migrationBuilder.CreateIndex(
                name: "IX_load_screens_RoomId",
                table: "load_screens",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_player_avatars_OwnerAccountId",
                table: "player_avatars",
                column: "OwnerAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_progressions_PlayerId",
                table: "player_progressions",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_settings_PlayerId_Key",
                table: "player_settings",
                columns: new[] { "PlayerId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promo_external_contents_RoomId",
                table: "promo_external_contents",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_promo_images_RoomId",
                table: "promo_images",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_received_gifts_ReceiverAccountId",
                table: "received_gifts",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_received_gifts_ReceiverAccountId_IsConsumed",
                table: "received_gifts",
                columns: new[] { "ReceiverAccountId", "IsConsumed" });

            migrationBuilder.CreateIndex(
                name: "IX_room_instances_OwnerAccountId_roomId",
                table: "room_instances",
                columns: new[] { "OwnerAccountId", "roomId" });

            migrationBuilder.CreateIndex(
                name: "IX_room_roles_RoomId",
                table: "room_roles",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_CreatorAccountId",
                table: "rooms",
                column: "CreatorAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_saved_outfits_OwnerAccountId",
                table: "saved_outfits",
                column: "OwnerAccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_storefront_items_StorefrontId",
                table: "storefront_items",
                column: "StorefrontId");

            migrationBuilder.CreateIndex(
                name: "IX_storefront_prices_StorefrontItemId",
                table: "storefront_prices",
                column: "StorefrontItemId");

            migrationBuilder.CreateIndex(
                name: "IX_sub_rooms_RoomId",
                table: "sub_rooms",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "avatar_items");

            migrationBuilder.DropTable(
                name: "cached_logins");

            migrationBuilder.DropTable(
                name: "consumable_items");

            migrationBuilder.DropTable(
                name: "earnable_rewards");

            migrationBuilder.DropTable(
                name: "gift_drops");

            migrationBuilder.DropTable(
                name: "load_screens");

            migrationBuilder.DropTable(
                name: "player_avatars");

            migrationBuilder.DropTable(
                name: "player_bios");

            migrationBuilder.DropTable(
                name: "player_progressions");

            migrationBuilder.DropTable(
                name: "player_settings");

            migrationBuilder.DropTable(
                name: "promo_external_contents");

            migrationBuilder.DropTable(
                name: "promo_images");

            migrationBuilder.DropTable(
                name: "received_gifts");

            migrationBuilder.DropTable(
                name: "room_instances");

            migrationBuilder.DropTable(
                name: "room_roles");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "saved_outfits");

            migrationBuilder.DropTable(
                name: "storefront_prices");

            migrationBuilder.DropTable(
                name: "sub_rooms");

            migrationBuilder.DropTable(
                name: "token_balances");

            migrationBuilder.DropTable(
                name: "storefront_items");

            migrationBuilder.DropTable(
                name: "storefronts");
        }
    }
}
