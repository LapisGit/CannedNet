using CannedNet;
using CannedNet.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CannedNet.Services;

public class APIService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        var jwtService = app.Services.GetRequiredService<JwtTokenService>();

        app.MapGet("/api/config/v1/amplitude", () => Results.Ok(new
        {
            AmplitudeKey = "fd846cd67066d19eec62fbdd00be9258",
            StatSigKey = "client-mnew4Ggjuu3Fbh86Ed3tRgWUxapp0D7PPB0i3MXUEao",
            RudderStackKey = "37qVlzZyXqdmZckXJ61vQeEJp8Y",
            UseRudderStack = false
        }));

        app.MapGet("/api/config/v2", () => Results.Content(File.ReadAllText("JSON/configv2.json"), "application/json"));
        app.MapGet("/api/versioncheck/v4", () => Results.Content("{\"VersionStatus\":0}", "application/json"));
        app.MapGet("/api/gameconfigs/v1/all", () => Results.Content(File.ReadAllText("JSON/gameconfigs.json"), "application/json"));

        app.MapGet("/api/relationships/v2/get", () => Results.Content("[]", "application/json"));
        app.MapGet("/api/messages/v2/get", () => Results.Content("[]", "application/json"));

        app.MapGet("/api/playerReputation/v1/{id}", (string id) => 
            Results.Content($"{{\"AccountId\":{id},\"Noteriety\":0,\"CheerGeneral\":0,\"CheerHelpful\":0,\"CheerCreative\":0,\"CheerGreatHost\":0,\"CheerSportsman\":0,\"CheerCredit\":20,\"SelectedCheer\":null}}", "application/json"));

        app.MapGet("/api/players/v1/progression/{id}", (string id) => 
            Results.Content($"{{\"PlayerId\":{id},\"Level\":1,\"XP\":0}}", "application/json"));

        app.MapPost("/api/playerReputation/v1/bulk", async (HttpRequest httpRequest, AppDbContext db) =>
        {
            /*var ids = await ParseFormIds(httpRequest);
            
            if (!ids.Any())
                return Results.Json(new List<object>());
            
            var reputations = ids.Select(id => new
            {
                AccountId = id,
                Noteriety = 0,
                CheerGeneral = 0,
                CheerHelpful = 0,
                CheerCreative = 0,
                CheerGreatHost = 0,
                CheerSportsman = 0,
                CheerCredit = 20,
                SelectedCheer = (object?)null
            }).ToList();
            
            return Results.Json(reputations);*/

            // TODO: implement real endpoint from grabbing from db
            var json = File.ReadAllText("JSON/bulkprogression.json");
            return Results.Content(json, "application/json");
        });

        app.MapPost("/api/players/v1/progression/bulk", async (HttpRequest httpRequest, AppDbContext db) =>
        {
            var ids = await ParseFormIds(httpRequest);
            
            if (!ids.Any())
                return Results.Json(new List<PlayerProgressionBulkResponse>());
            
            var progressions = await db.PlayerProgressions
                .Where(p => ids.Contains(p.PlayerId))
                .Select(p => new PlayerProgressionBulkResponse { PlayerId = p.PlayerId, Level = p.Level, Xp = p.Xp })
                .ToListAsync();
            
            return Results.Json(progressions);
        });

        app.MapPost("/api/v1/progression/bulk", async (HttpRequest httpRequest, AppDbContext db) =>
        {
            var ids = await ParseFormIds(httpRequest);
            
            if (!ids.Any())
                return Results.Json(new List<PlayerProgressionBulkResponse>());
            
            var progressions = await db.PlayerProgressions
                .Where(p => ids.Contains(p.PlayerId))
                .Select(p => new PlayerProgressionBulkResponse { PlayerId = p.PlayerId, Level = p.Level, Xp = p.Xp })
                .ToListAsync();
            
            return Results.Json(progressions);
        });

        app.MapGet("/api/avatar/v4/items", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();
            
            var items = await db.AvatarItems
                .Where(i => i.OwnerAccountId == id)
                .ToListAsync();
            
            return Results.Json(items);
        });

        app.MapGet("/api/avatar/v2", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();
            
            var avatar = await db.PlayerAvatars
                .FirstOrDefaultAsync(a => a.OwnerAccountId == id);
            
            if (avatar == null)
            {
                avatar = new PlayerAvatar
                {
                    OwnerAccountId = id,
                    OutfitSelections = "",
                    FaceFeatures = "{}",
                    SkinColor = "",
                    HairColor = ""
                };
                db.PlayerAvatars.Add(avatar);
                await db.SaveChangesAsync();
            }
            
            return Results.Json(new
            {
                OutfitSelections = avatar.OutfitSelections,
                FaceFeatures = avatar.FaceFeatures,
                SkinColor = avatar.SkinColor,
                HairColor = avatar.HairColor
            });
        });

        app.MapPost("/api/avatar/v2/set", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();
            
            request.EnableBuffering();
            var avatarUpdate = await System.Text.Json.JsonSerializer.DeserializeAsync<PlayerAvatar>(request.Body);
            
            if (avatarUpdate == null)
                return Results.BadRequest();
            
            var avatar = await db.PlayerAvatars
                .FirstOrDefaultAsync(a => a.OwnerAccountId == id);
            
            if (avatar == null)
            {
                avatar = new PlayerAvatar { OwnerAccountId = id };
                db.PlayerAvatars.Add(avatar);
            }
            
            avatar.OutfitSelections = avatarUpdate.OutfitSelections;
            avatar.FaceFeatures = avatarUpdate.FaceFeatures;
            avatar.SkinColor = avatarUpdate.SkinColor;
            avatar.HairColor = avatarUpdate.HairColor;
            
            await db.SaveChangesAsync();
            return Results.Ok(avatar);
        });

        app.MapGet("/api/avatar/v3/saved", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();
            
            request.EnableBuffering();
            
            var items = await db.SavedOutfits
                .Where(i => i.OwnerAccountId == id)
                .ToListAsync();
            
            return Results.Json(items);
        });
        
        app.MapGet("/api/PlayerReporting/v1/moderationBlockDetails", () => 
            Results.Content("{\"ReportCategory\":0,\"Duration\":0,\"GameSessionId\":0,\"IsHostKick\":false,\"Message\":\"\",\"PlayerIdReporter\":null,\"IsBan\":false}", "application/json"));
        
        app.MapGet("/api/PlayerReporting/v1/voteToKickReasons", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/vtkreasons.json");
            return Results.Content(json, "application/json");
        });

        app.MapGet("/api/settings/v2" +
                   "", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();
            
            var settings = await db.PlayerSettings
                .Where(s => s.PlayerId == id)
                .ToListAsync();
            
            if (!settings.Any())
            {
                var defaults = new List<PlayerSetting>
                {
                    new() { PlayerId = id, Key = "Recroom.OOBE", Value = "77" },
                    new() { PlayerId = id, Key = "SplitTestAssignedSegments", Value = "1|{\"SplitTesting+PhotonMaxDatagrams_2021_01_11\":\"Off\",\"SplitTesting+Curated_Rooms_2020_08_06\":\"Off\",\"SplitTesting+RoomRecommendationsType_2020_08_14\":\"Aug14MinVisitors35000\"}" },
                    new() { PlayerId = id, Key = "PlayerSessionCount", Value = "13" },
                    new() { PlayerId = id, Key = "TUTORIAL_COMPLETE_MASK", Value = "11" },
                    new() { PlayerId = id, Key = "BACKPACK_FAVORITE_TOOL", Value = "1" },
                    new() { PlayerId = id, Key = "VoiceChat", Value = "2" },
                    new() { PlayerId = id, Key = "VRAUTOSPRINT", Value = "1" },
                    new() { PlayerId = id, Key = "VR_MOVEMENT_MODE", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_SPRINT", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_WALK", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_VEHICLES", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_FLY", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_ROTATE", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_FORCES", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_FALL", Value = "0" },
                    new() { PlayerId = id, Key = "COMFORT_TELEPORT", Value = "0" },
                    new() { PlayerId = id, Key = "ROTATE_IN_PLACE_ENABLED", Value = "1" },
                    new() { PlayerId = id, Key = "ROTATION_INCREMENT", Value = "2" },
                    new() { PlayerId = id, Key = "CONTINUOUS_ROTATION_MODE", Value = "1" },
                    new() { PlayerId = id, Key = "DONT_LOCK_TOOLS_TO_HAND", Value = "0" },
                    new() { PlayerId = id, Key = "QualitySettings", Value = "2" },
                    new() { PlayerId = id, Key = "TeleportBuffer", Value = "0" },
                    new() { PlayerId = id, Key = "IgnoreBuffer", Value = "1" },
                    new() { PlayerId = id, Key = "FIRST_TIME_IN_FLAGS", Value = "0" },
                    new() { PlayerId = id, Key = "ShowRoomCenter", Value = "1" },
                    new() { PlayerId = id, Key = "USER_TRACKING", Value = "1" },
                    new() { PlayerId = id, Key = "STABILIZE_HANDS", Value = "0" },
                    new() { PlayerId = id, Key = "MakerPen_SnappingMode", Value = "2" },
                    new() { PlayerId = id, Key = "Recroom.ChallengeMap", Value = "17" },
                    new() { PlayerId = id, Key = "VoiceFilter2", Value = "1" },
                    new() { PlayerId = id, Key = "SFX_VOLUME_PERCENT_PREF", Value = "1" },
                };
                db.PlayerSettings.AddRange(defaults);
                await db.SaveChangesAsync();
                settings = defaults;
            }
    
            return Results.Json(settings);
        });
        
        app.MapPost("/api/settings/v2/set", async (HttpRequest request, AppDbContext db) =>
        {
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();

            request.EnableBuffering();
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            
            Console.WriteLine($"Settings request body: {body}");
            
            var settings = new List<PlayerSetting>();
            
            if (body.TrimStart().StartsWith("["))
            {
                settings = System.Text.Json.JsonSerializer.Deserialize<List<PlayerSetting>>(body) ?? [];
            }
            else
            {
                var single = System.Text.Json.JsonSerializer.Deserialize<PlayerSetting>(body);
                if (single != null) settings.Add(single);
            }
            
            settings = settings.Where(s => !string.IsNullOrEmpty(s.Key)).ToList();

            if (!settings.Any())
                return Results.Ok();
            
            db.PlayerSettings.RemoveRange(db.PlayerSettings.Where(s => s.PlayerId == id));
            
            foreach (var setting in settings)
            {
                setting.PlayerId = id;
                setting.Key = setting.Key ?? "";
                setting.Value = setting.Value ?? "";
                db.PlayerSettings.Add(setting);
            }
            
            await db.SaveChangesAsync();
            return Results.Ok();
        });

        app.MapGet("/api/equipment/v2/getUnlocked", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return "[]";
        });
        app.MapGet("/api/consumables/v2/getUnlocked", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return "[]";
        });
        app.MapGet("/api/objectives/v1/myprogress", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            var json = File.ReadAllText("JSON/tempmyprogress.json");
            return Results.Content(json, "application/json");
        });
        app.MapGet("/api/avatar/v2/gifts", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return "[]";
        });
        app.MapGet("/api/gamerewards/v1/pending", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return "[]";
        });
        app.MapGet("/api/communityboard/v2/current", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/communityboard.json");
            return Results.Content(json, "application/json");
        });
        app.MapGet("/api/playerevents/v1/all", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return Results.Content("{\"Created\":[],\"Responses\":[]}", "application/json");
        });
        app.MapPost("/api/CampusCard/v1/UpdateAndGetSubscription", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return Results.Json(new { subscription = (object?)null, platformAccountSubscribedPlayerId = (object?)null });
        });
        app.MapGet("/api/storefronts/v4/balance/2", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return Results.Json(new List<object> { new { Balance = 99999, CurrencyType = 2, BalanceType = -1, Platform = -1 } });
        });
        app.MapGet("/api/storefronts/v1/p2p/betaEnabled", async (HttpRequest request, AppDbContext db) =>
        {
            return "true";
        });
        app.MapGet("/api/announcement/v1/get", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/announcements.json");
            return Results.Content(json, "application/json");
        });
        app.MapGet("/api/roomkeys/v1/mine", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return "[]";
        });
        app.MapGet("/api/quickPlay/v1/getandclear", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            return Results.Content("{\"RoomName\":null,\"ActionCode\":null,\"TargetPlayerId\":null}", "application/json");
        });
        app.MapGet("/api/roomkeys/v1/room", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            var roomid = request.Query["roomId"];
            return "[]";
        });
        app.MapGet("/api/storefronts/v3/giftdropstore/3", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/storefront3.json");
            return Results.Content(json, "application/json");
        });
        app.MapGet("/api/challenge/v2/getCurrent", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/weeklychallenge.json");
            return Results.Content(json, "application/json");
            //return "{}";
        });
        app.MapGet("/roomserver/rooms/createdby/me", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO ADD FUNCTIONALITY
            var json = File.ReadAllText("JSON/ownedrooms.json");
            return Results.Content(json, "application/json");
        });
        app.MapGet("/roomserver/rooms/{id}", async (HttpRequest request, AppDbContext db, string id) =>
        {
            var json = File.ReadAllText("JSON/ownedrooms.json");
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var room in root.EnumerateArray())
                {
                    if (room.TryGetProperty("RoomId", out var roomIdProp) && roomIdProp.TryGetInt32(out var roomId) && roomId.ToString() == id)
                    {
                        return Results.Content(room.GetRawText(), "application/json");
                    }
                }
            }
            return Results.NotFound();
        });
        app.MapGet("/api/images/v2/named", async (HttpRequest request, AppDbContext db) =>
        {
            var json = File.ReadAllText("JSON/namedimages.json");
            return Results.Content(json, "application/json");
        });
    }

    private static async Task<List<int>> ParseFormIds(HttpRequest httpRequest)
    {
        var ids = new List<int>();
        
        if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength > 0)
        {
            httpRequest.EnableBuffering();
            using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            
            if (!string.IsNullOrWhiteSpace(body))
            {
                foreach (var pair in body.Split('&'))
                {
                    var keyValue = pair.Split('=');
                    if (keyValue.Length == 2 && keyValue[0] == "Ids")
                    {
                        var idString = Uri.UnescapeDataString(keyValue[1]);
                        foreach (var id in idString.Split(','))
                            if (int.TryParse(id, out var parsedId))
                                ids.Add(parsedId);
                        break;
                    }
                }
            }
            httpRequest.Body.Position = 0;
        }
        
        return ids;
    }
}
