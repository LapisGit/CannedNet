namespace CannedNet.Services;

public class ChatService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/thread", () => Results.Content("[]", "application/json"));
    }
}
