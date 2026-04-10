using CannedNet.Services.Infrastructure;

namespace CannedNet.Services.Controllers;

public class NSController
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        // Rec Room automatically calls this and sets the endpoints that correspond to services in the JSON file. These will be moved to the main server config once I get there.
        app.MapGet("/", () =>
        {
            var json = File.ReadAllText("JSON/endpoints.json");
            return Results.Content(json, "application/json");
        });
    }
}
