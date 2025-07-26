using Microsoft.AspNetCore.Mvc;

using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase {
  private readonly AuthService authService;

  public GoogleController(AuthService _authService) {
    authService = _authService;
  }

  [HttpGet("redirect")]
  public async Task<ContentResult> GetLogin([FromQuery] string state, [FromQuery] string code) {
    var template = await authService.HandleGoogleRedirect(state, code);
    return Content(template, "text/html");
  }

  [HttpGet("login")]
  public async Task<ActionResult> GetRedirect([FromQuery] string phoneNumber) {
    var result = await authService.HandleGoogleLogin(phoneNumber);
    if (result.IsRedirect) {
      return Redirect(result.Content);
    }
    return Content(result.Content, "text/html");
  }

  [HttpGet("refresh")]
  public async Task<ActionResult> GetRefresh([FromQuery] string idUser) {
    await authService.RefreshGoogleCredential(Guid.Parse(idUser));
    return StatusCode(201);
  }
}

