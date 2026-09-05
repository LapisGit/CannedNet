using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class CDNController : ControllerBase
{
    [HttpGet("/config/LoadingScreenTipData")]
    public IResult LoadingScreenTipData()
    {
        var json = System.IO.File.ReadAllText("JSON/loadingscreentipdata.json");
        return Results.Content(json, "application/json");
    }

    [HttpPost("/upload")]
    public async Task<IResult> Upload()
    {
        try
        {
            var context = HttpContext;

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
            var filePath = Path.Combine("Images", savedFileName);

            if (!Directory.Exists("Images"))
            {
                Directory.CreateDirectory("Images");
            }

            using (var fileStream = System.IO.File.Create(filePath))
            {
                await file.CopyToAsync(fileStream);
            }

            return Results.Ok(new
            {
                filename = savedFileName
            });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error uploading image: {ex.Message}");
        }
    }
}
