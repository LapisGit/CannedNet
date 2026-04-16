using CannedNet.Services.Infrastructure;

namespace CannedNet.Services.Controllers;

public class ImageController
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        var imagesDir = "Images";
        if (!Directory.Exists(imagesDir))
        {
            Directory.CreateDirectory(imagesDir);
        }

        app.MapGet("/{imageId}", (string imageId) =>
        {
            if (!Directory.Exists(imagesDir))
            {
                return Results.NotFound();
            }
            
            var files = Directory.GetFiles(imagesDir, $"{imageId}.*");
            var filePath = files.FirstOrDefault();
            
            if (filePath == null)
            {
                return Results.NotFound();
            }
            
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
            
            var fileStream = File.OpenRead(filePath);
            return Results.File(fileStream, contentType);
        });

        app.MapGet("/", (HttpContext context) =>
        {
            var imageId = context.Request.Query["imageId"].FirstOrDefault();
            var sig = context.Request.Query["sig"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(imageId))
            {
                return Results.BadRequest();
            }
            
            if (!Directory.Exists(imagesDir))
            {
                return Results.NotFound();
            }
            
            var files = Directory.GetFiles(imagesDir, $"{imageId}.*");
            var filePath = files.FirstOrDefault();
            
            if (filePath == null)
            {
                return Results.NotFound();
            }
            
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
            
            var fileStream = File.OpenRead(filePath);
            return Results.File(fileStream, contentType);
        });
    }
}