using Microsoft.AspNetCore.Mvc;

namespace CannedNet.Services.Controllers;

[ApiController]
public class CommerceController : ControllerBase
{
    [HttpGet("/purchase/v1/hasspentmoney")]
    public IResult HasSpentMoney()
    {
        return Results.NotFound();
    }
}
