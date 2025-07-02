using Microsoft.AspNetCore.Mvc;

using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class StatusController : ControllerBase {
  public StatusService statusService;
  public StatusController(StatusService _statusService) {
    statusService = _statusService;
  }

  [HttpGet]
  public async Task<ActionResult> GetStatus() {
    var status = await statusService.GetStatus();
    return Ok(status);
  }
}
