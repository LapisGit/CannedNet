using CannedNet.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class AuthController : ControllerBase
{
    [HttpGet("/eac/challenge")]
    public async Task<IResult> EacChallenge()
    {
        string file = await System.IO.File.ReadAllTextAsync("JSON/eacchallenge.txt");
        return Results.Content(file, "text/plain");
    }

    [HttpGet("/cachedlogin/forplatformid/{platform}/{id}")]
    public async Task<IResult> CachedLoginForPlatformId(string platform, string id, AppDbContext db)
    {
        int platformType = int.Parse(platform);
        List<CachedLogin> logins = await db.CachedLogins
            .Where(c => c.Platform == (PlatformType)platformType && c.PlatformID == id)
            .ToListAsync();

        return Results.Json(logins.Any() ? logins : new List<object>());
    }

    [HttpPost("/connect/token")]
    public async Task<IResult> ConnectToken(HttpRequest httpRequest, AppDbContext db, JwtTokenService jwtService)
    {
        string accountId = "";
        string platformId = "";
        string platform = "";

        if (httpRequest.ContentLength.HasValue && httpRequest.ContentLength > 0)
        {
            try
            {
                httpRequest.EnableBuffering();
                using var reader = new StreamReader(httpRequest.Body, leaveOpen: true);
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

                            if (key == "account_id")
                                accountId = value;
                            else if (key == "platform_id")
                                platformId = value;
                        }
                    }
                }
                httpRequest.Body.Position = 0;
            }
            catch { }
        }

        var accessToken = jwtService.GenerateToken(accountId, platformId, platform);

        if (!string.IsNullOrEmpty(accountId) && int.TryParse(accountId, out var id))
        {
            var roomInstance = await db.RoomInstances.FirstOrDefaultAsync(r => r.OwnerAccountId == id);
            if (roomInstance != null)
            {
                db.RoomInstances.Remove(roomInstance);
                await db.SaveChangesAsync();
            }
        }

        return Results.Json(new
        {
            access_token = accessToken,
            expires_in = 3600,
            token_type = "Bearer",
            refresh_token = Guid.NewGuid().ToString("N").ToUpper() + "-1",
            scope = "offline_access profile rn rn.accounts rn.accounts.gc rn.api rn.chat rn.clubs rn.commerce rn.match.read rn.match.write rn.notify rn.rooms rn.storage",
            key = "8oQ+e+WQaOBPbEcakhqs3dwZZdOmmyDUmJSD9u4AHMY="
        });
    }

    [HttpGet("/role/developer/{id}")]
    public IResult GetDeveloperRole(string id)
    {
        return Results.Ok(RecNetResult.Ok());
    }
}
