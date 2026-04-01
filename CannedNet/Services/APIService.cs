using CannedNet;
using CannedNet.Data;
using Microsoft.EntityFrameworkCore;

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

        app.MapPost("/api/players/v1/progression/bulk", async (HttpRequest httpRequest, AppDbContext db) =>
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
            
            return Results.Json(avatar);
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
            
            if (!settings.Any())
                return Results.BadRequest("No settings provided");
            
            db.PlayerSettings.RemoveRange(db.PlayerSettings.Where(s => s.PlayerId == id));
            
            foreach (var setting in settings)
            {
                setting.PlayerId = id;
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
    }
}
