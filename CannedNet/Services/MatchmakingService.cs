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

        app.MapPost("/goto/room/{room}", (HttpRequest request, string room) =>
        {
            // dormroom location id : 76d98498-60a1-430c-ab76-b54a29b7a163
            
            return Results.Json(new
            {
                errorCode = 0,
                roomInstance = new
                {
                    roomInstanceId = 1,
                    roomId = 1,
                    subRoomId = 1,
                    roomInstanceType = 2,
                    location = "76d98498-60a1-430c-ab76-b54a29b7a163",
                    dataBlob = "",
                    eventId = 0,
                    clubId=0,
                    photonRegionId = "us",
                    photonRoomId = "CannedNet_"+Guid.NewGuid(),
                    name = "DormRoom",
                    maxCapacity = 4,
                    isFull = false,
                    isPrivate = true,
                    isInProgress = false,
                    EncryptVoiceChat=false
                }
            });
        });
    }
}
