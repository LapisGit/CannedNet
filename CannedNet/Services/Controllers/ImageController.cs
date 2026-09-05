using System.Net.Mime;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class ImageController : ControllerBase
{
    private static readonly byte[] PlaceholderJpeg;
    private static readonly string ImagesDir;

    static ImageController()
    {
        Signatures.Init();

        using var image = new Image<Rgba32>(64, 64);
        image.Mutate(x => x.BackgroundColor(Color.FromRgb(32, 32, 32)));
        using var ms = new MemoryStream();
        image.SaveAsJpeg(ms, new JpegEncoder { Quality = 50 });
        PlaceholderJpeg = ms.ToArray();

        var dir = "Images";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        ImagesDir = dir;
    }

    [HttpGet("/{imageName}")]
    public async Task<IResult> GetImage(string imageName)
    {
        var context = HttpContext;

        var filePath = Path.Combine(ImagesDir, imageName);

        byte[] imageBytes;
        string contentType;

        if (System.IO.File.Exists(filePath))
        {
            imageBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var ext = Path.GetExtension(imageName).ToLowerInvariant();
            contentType = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".bmp" => "image/bmp",
                _ => "image/png"
            };
        }
        else
        {
            imageBytes = PlaceholderJpeg;
            contentType = "image/jpeg";
        }

        var cropSquare = context.Request.Query["cropSquare"].FirstOrDefault();
        var widthStr = context.Request.Query["width"].FirstOrDefault();
        var heightStr = context.Request.Query["height"].FirstOrDefault();

        if (string.IsNullOrEmpty(widthStr) && string.IsNullOrEmpty(heightStr) && string.IsNullOrEmpty(cropSquare))
        {
            SignImageResponse(context, ref imageBytes);
            return Results.File(imageBytes, contentType);
        }

        using var loadedImage = Image.Load(imageBytes);

        var resizeWidth = 0;
        var resizeHeight = 0;

        if (!string.IsNullOrEmpty(cropSquare) && cropSquare != "0" && cropSquare != "false")
        {
            var size = Math.Min(loadedImage.Width, loadedImage.Height);
            var x = (loadedImage.Width - size) / 2;
            var y = (loadedImage.Height - size) / 2;
            loadedImage.Mutate(img => img.Crop(new Rectangle(x, y, size, size)));
        }

        if (int.TryParse(widthStr, out var w))
            resizeWidth = w;

        if (int.TryParse(heightStr, out var h))
            resizeHeight = h;

        if (resizeWidth > 0 || resizeHeight > 0)
        {
            loadedImage.Mutate(x => x.Resize(resizeWidth, resizeHeight));
        }

        using var output = new MemoryStream();
        await loadedImage.SaveAsJpegAsync(output, new JpegEncoder { Quality = 85 });
        imageBytes = output.ToArray();
        SignImageResponse(context, ref imageBytes);
        return Results.File(imageBytes, "image/jpeg");
    }

    private static void SignImageResponse(HttpContext context, ref byte[] imageBytes)
    {
        if (context.Request.Query["sig"] != "p1") return;

        var signature = Signatures.Sign(imageBytes);
        if (signature != null)
        {
            context.Response.Headers["Content-Signature"] = $"key-id=KEY:RSA:p1.rec.net; data=ZnVjayB5b3Ugcmo=";
        }
    }
}
