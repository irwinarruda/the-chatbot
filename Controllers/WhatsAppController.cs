using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using TheChatbot.Entities;
using TheChatbot.Services;

namespace TheChatbot.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class WhatsAppController(MessagingService messagingService, AuthService authService) : ControllerBase {
  [HttpGet("configure")]
  public ActionResult<string> ConfigureWhatsAppMessageWebhook(
    [FromQuery(Name = "hub.mode")] string hubMode,
    [FromQuery(Name = "hub.challenge")] int hubChallenge,
    [FromQuery(Name = "hub.verify_token")] string hubVerifyToken
  ) {
    messagingService.ValidateWebhook(hubMode, hubVerifyToken);
    return Ok(hubChallenge);
  }
  [HttpPost("configure")]
  public async Task<ActionResult> ReceiveWhatsAppTextMessage([FromBody] JsonElement messageReceived) {
    await messagingService.ReceiveMessage(messageReceived);
    return Ok();
  }
  [HttpGet("message")]
  public async Task<ActionResult> SendMessage([FromQuery] string phoneNumber) {
    await messagingService.SendTextMessage(phoneNumber, "Message test sending here");
    return Ok();
  }
  [HttpGet("user")]
  public async Task<ActionResult> CreateUser([FromQuery] string phoneNumber) {
    var user = new User("Irwin Arruda", phoneNumber);
    await authService.CreateUser(user);
    return Ok();
  }
}

