using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class ClubsController : ControllerBase
{
    [HttpGet("/club/home/me")]
    public IResult ClubHomeMe()
    {
        return Results.NotFound();
    }
}
