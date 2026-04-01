namespace CannedNet.Services;

public class ImageService
{
    public WebApplicationBuilder Initialize(string[]? args = null) => ServiceExtensions.CreateRecNetBuilder(args);

    public void MapEndpoints(WebApplication app)
    {
        app.MapGet("/{imageId}", (string imageId, HttpContext context) =>
        {
            var imagesDir = "Images";
            
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
    }
}