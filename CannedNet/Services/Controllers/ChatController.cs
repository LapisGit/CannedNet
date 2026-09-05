using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class ChatController : ControllerBase
{
    [HttpGet("/thread")]
    public IResult Thread()
    {
        return Results.Content("[]", "application/json");
    }
}
