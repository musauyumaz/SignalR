using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalR.API.Hubs;

namespace SignalR.API.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ValuesController(IHubContext<MyHub> hubContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string message)
    {
        await hubContext.Clients.All.SendAsync("BroadcastMessageToAllClient", message);
        return Ok();
    }
}