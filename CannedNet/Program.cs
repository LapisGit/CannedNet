using CannedNet;
using CannedNet.Services;
using Microsoft.EntityFrameworkCore;

var apps = new List<(WebApplication App, ServiceRegistry Service)>();

foreach (var service in Services.All)
{
    var builder = WebApplication.CreateBuilder();
    
    service.ConfigureBuilder(builder);
    builder.WebHost.UseUrls($"http://*:{service.Port}");
    builder.Configuration.AddJsonFile($"AppConfigs/appsettings.{service.Name}.json", optional: true, reloadOnChange: true);
    
    apps.Add((builder.Build(), service));
}

using var scope = apps[0].App.Services.CreateScope();
scope.ServiceProvider.GetRequiredService<CannedNet.Data.AppDbContext>().Database.Migrate();
var jwtService = scope.ServiceProvider.GetRequiredService<JwtTokenService>();

foreach (var (app, service) in apps)
    service.MapEndpoints(app, jwtService);

await Task.WhenAll(apps.Select(t => t.App.RunAsync()));
