using CannedNet.Data;
using Microsoft.EntityFrameworkCore;
using CannedNet.Services;
using CannedNet.Services.Infrastructure;

namespace CannedNet.Services.Controllers;

public class AccountsController
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app, JwtTokenService jwtService)
    {
        app.MapGet("/", (HttpRequest request) =>
        {
            return Results.Ok("FUCKL MYSJDIOJD");
        });
        
        app.MapGet("/account/me", async (HttpRequest request, AppDbContext db) =>
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

            var account = await db.Accounts.FindAsync(id);
            if (account == null)
                return Results.NotFound();

            var selfAccount = new SelfAccount
            {
                AccountId = id,
                ProfileImage = account.ProfileImage ?? "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                IsJunior = account.IsJunior,
                Platforms = account.Platforms ?? 0,
                PersonalPronouns = account.PersonalPronouns ?? 0,
                IdentityFlags = account.IdentityFlags ?? 0,
                Username = account.Username ?? $"Player{id}",
                DisplayName = account.DisplayName ?? $"Player{id}",
                CreatedAt = account.CreatedAt,
                Email = null,
                Phone = null,
                JuniorState = null,
                Birthday = null,
                ParentAccountId = null,
                AvailableUsernameChanges = 1
            };

            return Results.Ok(selfAccount);
        });

        app.MapGet("/account/bulk", async (HttpRequest request, AppDbContext db) =>
        {
            var ids = request.Query["id"];
            var accountIds = new List<int>();

            foreach (var id in ids)
            {
                if (int.TryParse(id, out var accountId))
                {
                    accountIds.Add(accountId);
                }
            }

            var accounts = await db.Accounts
                .Where(a => accountIds.Contains(a.AccountId.Value))
                .ToListAsync();

            var result = accounts.Select(a => new Account
            {
                AccountId = a.AccountId,
                ProfileImage = a.ProfileImage ?? "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                IsJunior = a.IsJunior,
                Platforms = a.Platforms ?? 0,
                PersonalPronouns = a.PersonalPronouns ?? 0,
                IdentityFlags = a.IdentityFlags ?? 0,
                Username = a.Username ?? $"Player{a.AccountId}",
                DisplayName = a.DisplayName ?? $"Player{a.AccountId}",
                CreatedAt = a.CreatedAt
            }).ToList();

            return Results.Json(result);
        });
        
        app.MapGet("/account/{id}", async (HttpRequest request, string id, AppDbContext db) =>
        {
            if (!int.TryParse(id, out var accountId))
                return Results.BadRequest();

            var account = await db.Accounts.FindAsync(accountId);
            if (account == null)
                return Results.NotFound();

            var result = new Account
            {
                AccountId = account.AccountId,
                ProfileImage = account.ProfileImage ?? "hdqeamlcmatc6qzoi2ybgf0ddijjcf.jpg",
                IsJunior = account.IsJunior,
                Platforms = account.Platforms ?? 0,
                PersonalPronouns = account.PersonalPronouns ?? 0,
                IdentityFlags = account.IdentityFlags ?? 0,
                Username = account.Username ?? $"Player{account.AccountId}",
                DisplayName = account.DisplayName ?? $"Player{account.AccountId}",
                CreatedAt = account.CreatedAt
            };

            return Results.Json(result);
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

            return Results.Ok(RecNetResult.Ok());
        });
        
        app.MapPut("/account/me/username", async (HttpRequest request, AppDbContext db) =>
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

            string newAccountName = "";
            
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

                                if (key == "username")
                                    newAccountName = value;
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

            account.Username = newAccountName;
            await db.SaveChangesAsync();

            return Results.Ok(RecNetResult.Ok());
        });

        app.MapGet("/account/{id}/bio", async (HttpRequest request, string id, AppDbContext db) =>
        {
            if (!int.TryParse(id, out var accountId))
                return Results.BadRequest();

            var bio = await db.PlayerBios.FirstOrDefaultAsync(b => b.accountId == accountId);
            
            if (bio == null)
            {
                return Results.Json(new { accountId = accountId, bio = "" });
            }

            return Results.Json(new { accountId = bio.accountId, bio = bio.bio });
        });

        app.MapPut("/account/me/bio", async (HttpRequest request, AppDbContext db) =>
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

            string newBio = "";
            
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

                                if (key == "bio")
                                    newBio = Uri.UnescapeDataString(value);
                            }
                        }
                    }
                    request.Body.Position = 0;
                }
                catch { }
            }

            var bio = await db.PlayerBios.FirstOrDefaultAsync(b => b.accountId == id);
            
            if (bio == null)
            {
                bio = new PlayerBio
                {
                    accountId = id,
                    bio = newBio
                };
                db.PlayerBios.Add(bio);
            }
            else
            {
                bio.bio = newBio;
                db.PlayerBios.Update(bio);
            }

            await db.SaveChangesAsync();

            return Results.Json(new { success = true });
        });
    }
}
