using CannedNet;
using CannedNet.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// TODO: organize this into multiple files
// TODO: cdn stuff soon so i can not just have 0 images for this lmao
// TODO: setup signalr and get websockets running, currently should focus on notifications as logging in is being blocked by that

// needed so it won't try to auto capitalize random stuff that may be case-sensitive
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
});

// postgresql db, will probably have it configurable later on but for now just hardcode it in
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=cannednet;Username=postgres;Password=postgres";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<JwtTokenService>();

var app = builder.Build();

// apply db migrations on startup so we don't have to worry about it later bc i hate doing db things manually lol
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

var jwtService = app.Services.GetRequiredService<JwtTokenService>();

// request logging
app.Use(async (context, next) =>
{
    var request = context.Request;
    Console.WriteLine($"\n{request.Method} {request.Path}{request.QueryString}");
    
    foreach (var header in request.Headers)
        Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
    
    if (request.ContentLength > 0)
    {
        request.EnableBuffering();
        using (var reader = new StreamReader(request.Body, leaveOpen: true))
        {
            var body = await reader.ReadToEndAsync();
            Console.WriteLine($"  Body: {body}");
        }
        request.Body.Position = 0;
    }
    
    await next(context);
});

app.MapGet("/", () =>
{
    var json = File.ReadAllText("endpoints.json");
    return Results.Content(json, "application/json");
});

app.MapGet("/api/config/v1/amplitude", () =>
{
    var config = new
    {
        AmplitudeKey = "fd846cd67066d19eec62fbdd00be9258",
        StatSigKey = "client-mnew4Ggjuu3Fbh86Ed3tRgWUxapp0D7PPB0i3MXUEao",
        RudderStackKey = "37qVlzZyXqdmZckXJ61vQeEJp8Y",
        UseRudderStack = true
    };

    return Results.Ok(config);
});

app.MapGet("/api/config/v2", () =>
{
    var json = File.ReadAllText("configv2.json");
    return Results.Content(json, "application/json");
});

app.MapGet("/api/versioncheck/v4", () =>
{
    return Results.Content("{\"VersionStatus\":0}","application/json");
});

app.MapGet("/api/gameconfigs/v1/all", () =>
{
    var json = File.ReadAllText("gameconfigs.json");
    return Results.Content(json, "application/json");
});

app.MapGet("/photon", () =>
{
    var json = File.ReadAllText("photonsettings.json");
    return Results.Content(json, "application/json");
});

app.MapGet("/eac/challenge", () =>
{
    var file = File.ReadAllText("eacchallenge.txt");
    return Results.Content(file,"text/plain");
});

app.MapGet("/api/playerReputation/v1/{id}", (string id) =>
{
    // TODO: make rep actually work
    return Results.Content($"{{\"AccountId\":{id},\"Noteriety\":0,\"CheerGeneral\":0,\"CheerHelpful\":0,\"CheerCreative\":0,\"CheerGreatHost\":0,\"CheerSportsman\":0,\"CheerCredit\":20,\"SelectedCheer\":null}}","application/json");
});

app.MapGet("/api/players/v1/progression/{id}", (string id) =>
{
    // TODO: make progression actually work
    return Results.Content($"{{\"PlayerId\":{id},\"Level\":1,\"XP\":0}}","application/json");
});

app.MapGet("/api/PlayerReporting/v1/moderationBlockDetails", () =>
{
    // TODO: make this actually work
    return Results.Content($"{{\"ReportCategory\":0,\"Duration\":0,\"GameSessionId\":0,\"IsHostKick\":false,\"Message\":\"\",\"PlayerIdReporter\":null,\"IsBan\":false}}","application/json");
});

app.MapPost("/player/login", () =>
{
    return Results.Ok();
});

app.MapGet("/cachedlogin/forplatformid/{platform}/{id}", async (string platform, string id, AppDbContext db) =>
{
    var platformType = int.Parse(platform);
    var logins = await db.CachedLogins
        .Where(c => c.Platform == (PlatformType)platformType && c.PlatformID == id)
        .ToListAsync();
    
    return Results.Json(logins.Any() ? logins : new List<object>());
});

app.MapGet("/account/me", (HttpRequest request, AppDbContext db) =>
{
    var authHeader = request.Headers.Authorization.ToString();
    
    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        return Results.Unauthorized();

    var token = authHeader.Substring("Bearer ".Length);
    var accountId = jwtService.ValidateAndGetAccountId(token);

    if (string.IsNullOrEmpty(accountId) || !int.TryParse(accountId, out var id))
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
            accounts.Add(account);
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
            
            using (var reader = new StreamReader(httpRequest.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                
                if (!string.IsNullOrWhiteSpace(body))
                {
                    if (contentType.Contains("application/x-www-form-urlencoded"))
                    {
                        var pairs = body.Split('&');
                        foreach (var pair in pairs)
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
                }
            }
            httpRequest.Body.Position = 0;
        }
        catch {  }
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
        var loginRecord = new CachedLogin
        {
            AccountId = accountId,
            Platform = (PlatformType)platform,
            PlatformID = platformId,
            LastLoginTime = DateTime.UtcNow,
            RequirePassword = false
        };

        db.CachedLogins.Add(loginRecord);
    }

    await db.SaveChangesAsync();

    return Results.Ok(RecNetResultWithValue<Account>.Ok(account));
});

app.MapPost("/connect/token", async (HttpRequest httpRequest) =>
{
    string accountId = "";
    string platformId = "";
    string platform = "";

    if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength > 0)
    {
        try
        {
            httpRequest.EnableBuffering();
            using (var reader = new StreamReader(httpRequest.Body, leaveOpen: true))
            {
                var body = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    var pairs = body.Split('&');
                    foreach (var pair in pairs)
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            var key = Uri.UnescapeDataString(keyValue[0]);
                            var value = Uri.UnescapeDataString(keyValue[1]);

                            if (key == "account_id")
                                accountId = value;
                            else if (key == "platform_id")
                                platformId = value;
                        }
                    }
                }
            }
            httpRequest.Body.Position = 0;
        }
        catch { }
    }

    var accessToken = jwtService.GenerateToken(accountId, platformId, platform);

    var response = new
    {
        access_token = accessToken,
        expires_in = 3600,
        token_type = "Bearer",
        refresh_token = Guid.NewGuid().ToString("N").ToUpper() + "-1",
        scope = "offline_access profile rn rn.accounts rn.accounts.gc rn.api rn.chat rn.clubs rn.commerce rn.match.read rn.match.write rn.notify rn.rooms rn.storage",
        key = "8oQ+e+WQaOBPbEcakhqs3dwZZdOmmyDUmJSD9u4AHMY="
    };

    return Results.Json(response);
});

app.Run();


