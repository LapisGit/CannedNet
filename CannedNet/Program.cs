using CannedNet;
using CannedNet.Services;
using Microsoft.EntityFrameworkCore;

var apps = new List<(WebApplication App, ServiceRegistry Service)>();

foreach (var service in Services.All)
{
    var builder = WebApplication.CreateBuilder();
    
    builder.Services.AddLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    });
    
    service.ConfigureBuilder(builder);
    builder.WebHost.UseUrls($"http://*:{service.Port}");
    builder.Configuration.AddJsonFile($"AppConfigs/appsettings.{service.Name}.json", optional: true, reloadOnChange: true);
    
    apps.Add((builder.Build(), service));
}

using var scope = apps[0].App.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<CannedNet.Data.AppDbContext>();
try
{
    dbContext.Database.Migrate();
}
catch (Exception ex)
{
    dbContext.Database.EnsureCreated();
}

// automatically fill out storefronts tables from the JSON's
var seedingService = scope.ServiceProvider.GetRequiredService<StorefrontFillService>();
await seedingService.FillStorefrontsAsync();

var jwtService = scope.ServiceProvider.GetRequiredService<JwtTokenService>();

foreach (var (app, service) in apps)
{
    service.MapEndpoints(app, jwtService);
    app.UseRequestLogging();
}

await Task.WhenAll(apps.Select(t => t.App.RunAsync()));
