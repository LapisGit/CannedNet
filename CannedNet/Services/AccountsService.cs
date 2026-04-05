using CannedNet.Data;
using Microsoft.EntityFrameworkCore;

namespace CannedNet.Services;

public class AccountsService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app, JwtTokenService jwtService)
    {
        app.MapGet("/", (HttpRequest request) =>
        {
            return Results.Ok("FUCKL MYSJDIOJD");
        });
        
        app.MapGet("/account/me", (HttpRequest request, AppDbContext db) =>
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

            var selfAccount = new SelfAccount
            {
                AccountId = id,
                ProfileImage = "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                IsJunior = false,
                Platforms = 0,
                PersonalPronouns = 0,
                IdentityFlags = 0,
                Username = $"Player{id}",
                DisplayName = $"Player{id}",
                CreatedAt = DateTime.UtcNow,
                Email = null,
                Phone = null,
                JuniorState = null,
                Birthday = null,
                ParentAccountId = null,
                AvailableUsernameChanges = 1
            };

            return Results.Ok(selfAccount);
        });

        app.MapGet("/account/bulk", (HttpRequest request) =>
        {
            var ids = request.Query["id"];
            var accounts = new List<Account>();

            foreach (var id in ids)
            {
                if (int.TryParse(id, out var accountId))
                {
                    accounts.Add(new Account
                    {
                        AccountId = accountId,
                        ProfileImage = "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                        IsJunior = false,
                        Platforms = 0,
                        PersonalPronouns = 0,
                        IdentityFlags = 0,
                        Username = $"Player{accountId}",
                        DisplayName = $"Player{accountId}",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            return Results.Json(accounts);
        });
        
        app.MapGet("/account/{id}", (HttpRequest request, string id) =>
        {
            var accounts = new Account();
            
            if (int.TryParse(id, out var accountId))
            {
                accounts = new Account
                {
                    AccountId = accountId,
                    ProfileImage = "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                    IsJunior = false,
                    Platforms = 0,
                    PersonalPronouns = 0,
                    IdentityFlags = 0,
                    Username = $"Player{accountId}",
                    DisplayName = $"Player{accountId}",
                    CreatedAt = DateTime.UtcNow
                };
            }

            return Results.Json(accounts);
        });
        
        app.MapPost("/account/create", async (HttpRequest httpRequest, AppDbContext db) =>
        {
            int platform = 0;
            string platformId = "";
            
            if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength > 0)
            {
                try
                {
                    var contentType = httpRequest.ContentType ?? "";
                    httpRequest.EnableBuffering();
                    
                    using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    
                    if (!string.IsNullOrWhiteSpace(body) && contentType.Contains("application/x-www-form-urlencoded"))
                    {
                        foreach (var pair in body.Split('&'))
                        {
                            var keyValue = pair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                var key = Uri.UnescapeDataString(keyValue[0]);
                                var value = Uri.UnescapeDataString(keyValue[1]);

                                if (key == "platform" && int.TryParse(value, out var parsedPlatform))
                                    platform = parsedPlatform;
                                else if (key == "platformId")
                                    platformId = value;
                            }
                        }
                    }
                    httpRequest.Body.Position = 0;
                }
                catch { }
            }

            var accountId = new Random().Next(10000, 99999);
            var account = new Account
            {
                AccountId = accountId,
                ProfileImage = "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                IsJunior = false,
                Platforms = 0,
                PersonalPronouns = 0,
                IdentityFlags = 0,
                Username = $"Player{accountId}",
                DisplayName = $"Player{accountId}",
                CreatedAt = DateTime.UtcNow
            };
            
            db.Accounts.Add(account);

            if (!string.IsNullOrEmpty(platformId))
            {
                db.CachedLogins.Add(new CachedLogin
                {
                    AccountId = accountId,
                    Platform = (PlatformType)platform,
                    PlatformID = platformId,
                    LastLoginTime = DateTime.UtcNow,
                    RequirePassword = false
                });
            }

            await db.SaveChangesAsync();

            // create players dorm room
            var maxRoomId = await db.Rooms.MaxAsync(r => (int?)r.RoomId) ?? 0;
            var maxId = await db.Rooms.MaxAsync(r => (int?)r.Id) ?? 0;
            var dormRoomId = maxRoomId + 1;
            var dormRoom = new Room
            {
                Id = maxId + 1,
                RoomId = dormRoomId,
                Name = "DormRoom",
                Description = "Your personal room",
                CreatorAccountId = accountId,
                ImageName = "",
                State = 0,
                Accessibility = 0,
                SupportsLevelVoting = false,
                IsRRO = false,
                IsDorm = true,
                CloningAllowed = false,
                SupportsVRLow = true,
                SupportsQuest2 = true,
                SupportsMobile = true,
                SupportsScreens = true,
                SupportsWalkVR = true,
                SupportsTeleportVR = true,
                SupportsJuniors = true,
                MinLevel = 0,
                WarningMask = 0,
                CustomWarning = null,
                DisableMicAutoMute = false,
                DisableRoomComments = false,
                EncryptVoiceChat = false,
                CreatedAt = DateTime.UtcNow,
                Tags = "[]"
            };
            db.Rooms.Add(dormRoom);

            // Create a sub room for the dorm
            var dormSubRoom = new SubRoom
            {
                RoomId = dormRoomId,
                SubRoomId = 1,
                Name = "DormRoom",
                DataBlob = "",
                IsSandbox = false,
                MaxPlayers = 4,
                Accessibility = 0,
                UnitySceneId = "76d98498-60a1-430c-ab76-b54a29b7a163", // Dorm scene ID
                DataSavedAt = DateTime.UtcNow
            };
            db.SubRooms.Add(dormSubRoom);

            await db.SaveChangesAsync();

            return Results.Ok(RecNetResultWithValue<Account>.Ok(account));
        });
        app.MapGet("/parentalcontrol/me", async (HttpRequest request, AppDbContext db) =>
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
                
                return Results.Content($"{{\"accountId\":{accountId},\"disallowInAppPurchases\":false}}", "application/json");
        });
        
        app.MapPut("/account/me/displayname", async (HttpRequest request, AppDbContext db) =>
        {
            // TODO: get what the server should actually respond with, OK fails.
            
            var authHeader = request.Headers.Authorization.ToString();
    
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Results.Unauthorized();

            var token = authHeader.Substring("Bearer ".Length);
            var accountId = jwtService.ValidateAndGetAccountId(token);

            if (string.IsNullOrEmpty(accountId))
                return Results.Unauthorized();

            if (!int.TryParse(accountId.AsSpan(), out var id))
                return Results.Unauthorized();

            string newDisplayName = "";
            
            if (request.ContentLength.HasValue && request.ContentLength > 0)
            {
                try
                {
                    request.EnableBuffering();
                    using var reader = new StreamReader(request.Body, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        foreach (var pair in body.Split('&'))
                        {
                            var keyValue = pair.Split('=');
                            if (keyValue.Length == 2)
                            {
                                var key = Uri.UnescapeDataString(keyValue[0]);
                                var value = Uri.UnescapeDataString(keyValue[1]);

                                if (key == "displayName")
                                    newDisplayName = value;
                            }
                        }
                    }
                    request.Body.Position = 0;
                }
                catch { }
            }

            var account = await db.Accounts.FindAsync(id);
            if (account == null)
                return Results.NotFound();

            account.DisplayName = newDisplayName;
            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }
}
