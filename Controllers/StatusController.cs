using Microsoft.AspNetCore.Mvc;

using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class StatusController(StatusService statusService) : ControllerBase {
  [HttpGet]
  public async Task<ActionResult> GetStatus() {
    var status = await statusService.GetStatus();
    return Ok(status);
  }
}
