using System.Text.Json;

using Microsoft.AspNetCore.Mvc;

using TheChatbot.Entities.Extensions;
using TheChatbot.Resources;
using TheChatbot.Services;

namespace TheChatbot.Controllers;

public class TuiSendMessageRequest {
  public required string Text { get; set; }
  public required string PhoneNumber { get; set; }
}

[ApiController]
[Route("/api/v1/[controller]")]
public class TuiController(MessagingService messagingService, IWhatsAppMessagingGateway whatsAppGateway) : ControllerBase {
  [HttpPost("messages")]
  public async Task<ActionResult> SendMessage([FromBody] TuiSendMessageRequest request) {
    if (whatsAppGateway is not TuiWhatsAppMessagingGateway) {
      return BadRequest(new { error = "Not in Tui mode" });
    }
    var dto = new ReceiveTextMessageDTO {
      From = request.PhoneNumber,
      Text = request.Text,
      IdProvider = Guid.NewGuid().ToString(),
      CreatedAt = DateTime.UtcNow.TruncateToMicroseconds()
    };
    await messagingService.ListenToMessage(dto);
    return Ok(new { status = "ok" });
  }

  [HttpGet("messages/stream")]
  public async Task GetMessageStream(CancellationToken cancellationToken) {
    if (whatsAppGateway is not TuiWhatsAppMessagingGateway tuiGateway) {
      HttpContext.Response.StatusCode = 400;
      await HttpContext.Response.WriteAsync("Not in Tui mode");
      return;
    }
    HttpContext.Response.ContentType = "text/event-stream";
    HttpContext.Response.Headers["Cache-Control"] = "no-cache";
    HttpContext.Response.Headers["Connection"] = "keep-alive";
    await HttpContext.Response.Body.FlushAsync(cancellationToken);
    var reader = tuiGateway.GetOutgoingMessages();
    try {
      await foreach (var msg in reader.ReadAllAsync(cancellationToken)) {
        var json = JsonSerializer.Serialize(msg);
        await HttpContext.Response.WriteAsync($"data: {json}\n\n", cancellationToken);
        await HttpContext.Response.Body.FlushAsync(cancellationToken);
      }
    } catch (OperationCanceledException) { }
  }
}
