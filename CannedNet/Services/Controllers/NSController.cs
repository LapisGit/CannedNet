using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
[Route("ns")]
public class NSController : ControllerBase
{
    [HttpGet]
    public IResult Index()
    {
        var json = System.IO.File.ReadAllText("JSON/endpoints.json");
        return Results.Content(json, "application/json");
    }
}
