using CannedNet.Data;
using Microsoft.EntityFrameworkCore;

namespace CannedNet.Services;

public static class ServiceExtensions
{
    public static void ConfigureRecNetServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.PropertyNamingPolicy = null);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=cannednet;Username=postgres;Password=postgres";

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddScoped<JwtTokenService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddScoped<StorefrontFillService>();
        builder.Services.AddSignalR();
    }

    public static void ConfigureHostUrl(this WebApplicationBuilder builder, string defaultUrl)
    {
        var url = builder.Configuration["Hosting:Url"];
        builder.WebHost.UseUrls(string.IsNullOrWhiteSpace(url) ? defaultUrl : url);
    }

    public static WebApplicationBuilder CreateRecNetBuilder(string[]? args) => 
        WebApplication.CreateBuilder(args ?? []);
}
