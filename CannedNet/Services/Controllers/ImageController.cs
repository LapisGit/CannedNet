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

        app.MapGet("/{imageId}", (string imageId, HttpContext context) =>
        {
            if (!Directory.Exists(imagesDir))
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            }
            
            var files = Directory.GetFiles(imagesDir, $"{imageId}.*");
            var filePath = files.FirstOrDefault();
            
            if (filePath == null)
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
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
            
            context.Response.ContentType = contentType;
            return File.OpenRead(filePath).CopyToAsync(context.Response.Body);
        });

        app.MapGet("/", (HttpContext context) =>
        {
            var imageId = context.Request.Query["imageId"].FirstOrDefault();
            var sig = context.Request.Query["sig"].FirstOrDefault();
            
            if (string.IsNullOrEmpty(imageId))
            {
                context.Response.StatusCode = 400;
                return Task.CompletedTask;
            }
            
            if (!Directory.Exists(imagesDir))
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
            }
            
            var files = Directory.GetFiles(imagesDir, $"{imageId}.*");
            var filePath = files.FirstOrDefault();
            
            if (filePath == null)
            {
                context.Response.StatusCode = 404;
                return Task.CompletedTask;
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
            
            context.Response.ContentType = contentType;
            return File.OpenRead(filePath).CopyToAsync(context.Response.Body);
        });

        app.MapPost("/api/images/v4/uploadsaved", async (HttpContext context) =>
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var file = form.Files.FirstOrDefault();

                if (file == null)
                {
                    return Results.BadRequest(new { error = "No file found in request" });
                }

                var imageId = Guid.NewGuid().ToString("N");
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp" };
                if (string.IsNullOrEmpty(extension) || !validExtensions.Contains(extension))
                {
                    extension = ".png";
                }

                var savedFileName = imageId + extension;
                var filePath = Path.Combine(imagesDir, savedFileName);

                using (var fileStream = File.Create(filePath))
                {
                    await file.CopyToAsync(fileStream);
                }

                return Results.Ok(new
                {
                    imageId = imageId,
                    imageUrl = $"/{imageId}{extension}",
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error uploading image: {ex.Message}");
            }
        });
    }
}