using System.Net.Mime;

namespace CannedNet.Services;

public class ServiceRegistry
{
    public required string Name { get; init; }
    public required int Port { get; init; }
    public required Type ServiceType { get; init; }
    public required Action<WebApplicationBuilder> ConfigureBuilder { get; init; }
    public required Action<WebApplication, JwtTokenService> MapEndpoints { get; init; }
}

public static class Services
{
    public static readonly List<ServiceRegistry> All =
    [
        new() { Name = "NS", Port = 5001, ServiceType = typeof(NSService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new NSService().MapEndpoints(app) },
        new() { Name = "API", Port = 5000, ServiceType = typeof(APIService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new APIService().MapEndpoints(app) },
        new() { Name = "Auth", Port = 5002, ServiceType = typeof(AuthService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, jwt) => new AuthService().MapEndpoints(app) },
        new() { Name = "Chat", Port = 5003, ServiceType = typeof(ChatService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new ChatService().MapEndpoints(app) },
        new() { Name = "Match", Port = 5004, ServiceType = typeof(MatchmakingService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new MatchmakingService().MapEndpoints(app) },
        new() { Name = "Accounts", Port = 5005, ServiceType = typeof(AccountsService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, jwt) => new AccountsService().MapEndpoints(app, jwt) },
        new() { Name = "Notify", Port = 5006, ServiceType = typeof(NotifyService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new NotifyService().MapEndpoints(app) },
        new() { Name = "CDN", Port = 5007, ServiceType = typeof(CDNService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new CDNService().MapEndpoints(app) },
        new() { Name = "Image", Port = 5008, ServiceType = typeof(ImageService), ConfigureBuilder = b => b.ConfigureRecNetServices(), MapEndpoints = (app, _) => new ImageService().MapEndpoints(app) }
    ];
}
