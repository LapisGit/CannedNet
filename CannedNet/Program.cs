using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using CannedNet.Data;
using CannedNet.Hubs;
using CannedNet.Services;
using CannedNet.Services.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CannedNet;

public static class Program
{
    public static async Task Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=cannednet;Username=postgres;Password=postgres";

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddScoped<StorefrontFillService>();
        builder.Services.AddSingleton<JwtTokenService>();
        builder.Services.AddSingleton<ConfigService>();
        builder.Services.AddSignalR();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var rsa = JwtTokenService.GetRsaInstance();
                
                // TODO: make validissuer and validaudiences/validaudience configurable
                // TODO: make this key load from config, DO NOT USE THIS KEY IN PROD SERVERS!!!
                var securityKey = new RsaSecurityKey(rsa) { KeyId = "7C2F041398671515B0862CB23FAF95B03" };

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidIssuer = "https://auth.lapis.codes",
                    ValidateAudience = false,
                    ValidAudiences = new[]
                    {
                        "https://auth.lapis.codes",
                        "https://auth.lapis.codes/resources"
                    },
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorization();

        WebApplication app = builder.Build();

        app.UseRequestLogging();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<NotificationsHub>("/hub/v1");

        IHubContext<NotificationsHub> hubContext = app.Services.GetRequiredService<IHubContext<NotificationsHub>>();
        NotificationService.SetHubContext(hubContext);

        using IServiceScope scope = app.Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            await db.Database.MigrateAsync();
        }
        catch
        {
            await db.Database.EnsureCreatedAsync();
        }

        StorefrontFillService seedingService = scope.ServiceProvider.GetRequiredService<StorefrontFillService>();
        await seedingService.FillStorefrontsAsync();

        Signatures.Init();

        await app.RunAsync();
    }
}
