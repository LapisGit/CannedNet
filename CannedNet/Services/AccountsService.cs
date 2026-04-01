using CannedNet.Data;

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

            return Results.Ok(RecNetResultWithValue<Account>.Ok(account));
        });
    }
}
