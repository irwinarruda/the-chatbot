using Microsoft.AspNetCore.Mvc;

using TheChatbot.Entities.Extensions;
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
    messagingService.ValidateWebhook(hubMode, hubVerifyToken);
    return Ok(hubChallenge);
  }
  [HttpPost("webhook")]
  public async Task<ActionResult> ReceiveWhatsAppTextMessage() {
    var xHubSignature256 = HttpContext.Request.Headers["X-Hub-Signature-256"].ToString();
    var stringifiedBody = await HttpContext.Request.GetRawBodyAsync();
    await messagingService.ReceiveMessage(stringifiedBody, xHubSignature256);
    return Ok();
  }
}
