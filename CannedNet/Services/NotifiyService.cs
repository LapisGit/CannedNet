using CannedNet.Hubs;

namespace CannedNet.Services;

public class NotifyService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        app.MapHub<NotificationsHub>("/hub/v1");
    }
}
