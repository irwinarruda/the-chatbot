using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using TheChatbot.Infra;
using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class WhatsAppController(MessagingService messagingService) : ControllerBase {

  [HttpGet("webhook")]
  public ActionResult<string> ConfigureWhatsAppMessageWebhook(
    [FromQuery(Name = "hub.mode")] string hubMode,
    [FromQuery(Name = "hub.challenge")] string hubChallenge,
    [FromQuery(Name = "hub.verify_token")] string hubVerifyToken
  ) {
    if (!IsValidMetaDomain()) {
      throw new ForbiddenException("Request not from authorized Meta domain");
    }
    messagingService.ValidateWebhook(hubMode, hubVerifyToken);
    return Ok(hubChallenge);
  }
  [HttpPost("webhook")]
  public async Task<ActionResult> ReceiveWhatsAppTextMessage([FromBody] JsonElement messageReceived) {
    if (!IsValidMetaDomain()) {
      throw new ForbiddenException("Request not from authorized Meta domain");
    }
    await messagingService.ReceiveMessage(messageReceived);
    return Ok();
  }
  private bool IsValidMetaDomain() {
    var userAgent = Request.Headers.UserAgent.ToString();
    var allowedDomain = messagingService.GetAllowedDomain();
    return userAgent.Contains("facebookplatform") ||
      userAgent.Contains("facebookexternalua") ||
      userAgent.Contains(allowedDomain);
  }
}

