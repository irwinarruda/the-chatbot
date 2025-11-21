using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
[EnableRateLimiting("MigrationPolicy")]
public class MigrationController(MigrationService migrationService) : ControllerBase {
  [HttpGet]
  public async Task<ActionResult> ListPendingMigrations() {
    var migrations = await migrationService.ListPendingMigrations();
    return Ok(migrations);
  }

  [HttpPost]
  public async Task<ActionResult> RunPendingMigrations() {
    var password = Request.Headers["X-Migration-Password"].ToString();
    await migrationService.RunPendingMigrations(password);
    return NoContent();
  }
}
