using System.Text.Json;
using System.Text.Json.Serialization;

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

        app.MapPost("/player/heartbeat", async (HttpRequest request) =>
        {
            
        });
    }
}

public class HeartbeatRequest
{
    [JsonPropertyName("playerId")]
    public int playerId { get; set; }

    [JsonPropertyName("statusVisibility")]
    public int statusVisibility { get; set; }

    [JsonPropertyName("deviceClass")]
    public int deviceClass { get; set; }

    [JsonPropertyName("vrMovementMode")]
    public int vrMovementMode { get; set; }

    [JsonPropertyName("roomInstance")]
    public RoomInstanceInfo? roomInstance { get; set; }

    [JsonPropertyName("isOnline")]
    public bool isOnline { get; set; }

    [JsonPropertyName("appVersion")]
    public string? appVersion { get; set; }

    [JsonPropertyName("platform")]
    public int platform { get; set; }
}

public class RoomInstanceInfo
{
    [JsonPropertyName("roomInstanceId")]
    public int roomInstanceId { get; set; }

    [JsonPropertyName("roomId")]
    public int roomId { get; set; }

    [JsonPropertyName("subRoomId")]
    public int subRoomId { get; set; }

    [JsonPropertyName("roomInstanceType")]
    public int roomInstanceType { get; set; }

    [JsonPropertyName("location")]
    public string? location { get; set; }

    [JsonPropertyName("dataBlob")]
    public string? dataBlob { get; set; }

    [JsonPropertyName("eventId")]
    public int eventId { get; set; }

    [JsonPropertyName("clubId")]
    public int clubId { get; set; }

    [JsonPropertyName("roomCode")]
    public string? roomCode { get; set; }

    [JsonPropertyName("photonRegionId")]
    public string? photonRegionId { get; set; }

    [JsonPropertyName("photonRoomId")]
    public string? photonRoomId { get; set; }

    [JsonPropertyName("name")]
    public string? name { get; set; }

    [JsonPropertyName("maxCapacity")]
    public int maxCapacity { get; set; }

    [JsonPropertyName("isFull")]
    public bool isFull { get; set; }

    [JsonPropertyName("isPrivate")]
    public bool isPrivate { get; set; }

    [JsonPropertyName("isInProgress")]
    public bool isInProgress { get; set; }

    [JsonPropertyName("EncryptVoiceChat")]
    public bool EncryptVoiceChat { get; set; }
}
