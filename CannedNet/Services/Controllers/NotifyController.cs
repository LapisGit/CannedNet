using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CannedNet.Services.Controllers;

[ApiController]
public class NotifyController : ControllerBase
{
    [HttpGet("/hub/v1")]
    public void HubEndpoint()
    {
    }
}
