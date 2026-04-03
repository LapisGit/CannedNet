namespace CannedNet.Services;

public class CommerceService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/purchase/v1/hasspentmoney", (HttpRequest request) =>
        {
            return "true";
        });
    }
}