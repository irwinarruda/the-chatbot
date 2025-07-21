using Microsoft.AspNetCore.Mvc;

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
    await authService.SaveUserByGoogleCredential(state, code);
    var template = await authService.GetThankYouPageHtmlString();
    return Content(template, "text/html");
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect([FromQuery] string phoneNumber) {
    var url = authService.GetGoogleLoginUrl(phoneNumber);
    return Redirect(url);
  }

  [HttpGet("refresh")]
  public async Task<ActionResult> GetRefresh([FromQuery] string idUser) {
    await authService.RefreshGoogleCredential(Guid.Parse(idUser));
    return StatusCode(201);
  }
}

