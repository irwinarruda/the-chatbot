using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
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
  public async Task<ActionResult> GetRedirect([Required][FromQuery(Name = "phone_number")] string phoneNumber) {
    var result = await authService.HandleGoogleLogin(phoneNumber);
    if (result.IsRedirect) {
      return Redirect(result.Content);
    }
    return Content(result.Content, "text/html");
  }

  [HttpGet("refresh")]
  public async Task<ActionResult> GetRefresh([Required][FromQuery(Name = "id_user")] string idUser) {
    var user = await authService.GetUserById(Guid.Parse(idUser)) ?? throw new Exception();
    await authService.RefreshGoogleCredential(user);
    return StatusCode(201);
  }
}

