namespace CannedNet.Services;

public class MatchmakingService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        app.MapPost("/player/login", () => Results.Ok());
        
        app.MapGet("/player", (HttpRequest request) =>
        {
            var id = request.Query["id"];
            var accounts = new List<Account>();

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

            return Results.Json(accounts);
        });
    }
}
