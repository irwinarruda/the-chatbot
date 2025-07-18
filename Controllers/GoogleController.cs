using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using TheChatbot.Infra;
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
    await authService.SaveGoogleCredentials(state, code);
    var template = await authService.GetThankYouPageHtmlString();
    return Content(template, "text/html");
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect([FromQuery] string phoneNumber) {
    var url = authService.GetGoogleLoginUrl(phoneNumber);
    return Redirect(url);
  }

  [HttpGet("refresh")]
  public async Task<ActionResult> GetRefresh() {
    // Implement
    return Ok("");
  }
}

