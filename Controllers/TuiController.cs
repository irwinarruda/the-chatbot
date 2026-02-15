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

public class TuiSendAudioRequest {
  public required string FilePath { get; set; }
  public required string PhoneNumber { get; set; }
  public string? MimeType { get; set; }
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

  [HttpPost("audio")]
  public async Task<ActionResult> SendAudioMessage([FromBody] TuiSendAudioRequest request) {
    if (whatsAppGateway is not TuiWhatsAppMessagingGateway tuiGateway) {
      return BadRequest(new { error = "Not in Tui mode" });
    }
    var filePath = NormalizeFilePath(request.FilePath);
    if (!Path.IsPathRooted(filePath)) {
      return BadRequest(new { error = "File path must be absolute (or start with ~/)" });
    }
    if (!System.IO.File.Exists(filePath)) {
      return BadRequest(new { error = "Audio file not found" });
    }
    var mimeType = request.MimeType ?? GetMimeType(filePath);
    if (!mimeType.StartsWith("audio/")) {
      return BadRequest(new { error = "MimeType must be an audio type" });
    }
    await using var stream = System.IO.File.OpenRead(filePath);
    var mediaId = await tuiGateway.SaveMediaAsync(stream);
    var dto = new ReceiveAudioMessageDTO {
      From = request.PhoneNumber,
      MimeType = mimeType,
      MediaId = mediaId,
      IdProvider = Guid.NewGuid().ToString(),
      CreatedAt = DateTime.UtcNow.TruncateToMicroseconds()
    };
    await messagingService.ListenToMessage(dto);
    return Ok(new { status = "ok", media_id = mediaId });
  }

  [HttpGet("transcripts")]
  public async Task<ActionResult> GetTranscripts([FromQuery] string phoneNumber) {
    if (whatsAppGateway is not TuiWhatsAppMessagingGateway) {
      return BadRequest(new { error = "Not in Tui mode" });
    }
    var transcripts = await messagingService.GetTranscripts(phoneNumber);
    return Ok(transcripts);
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

  private static string GetMimeType(string filePath) {
    var extension = Path.GetExtension(filePath).ToLowerInvariant();
    return extension switch {
      ".ogg" => "audio/ogg",
      ".flac" => "audio/flac",
      ".wav" => "audio/wav",
      ".mp3" => "audio/mpeg",
      ".m4a" => "audio/mp4",
      ".aac" => "audio/aac",
      ".amr" => "audio/amr",
      ".webm" => "audio/webm",
      _ => "application/octet-stream"
    };
  }

  private static string NormalizeFilePath(string filePath) {
    var trimmedPath = filePath.Trim();
    if (trimmedPath == "~") {
      return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
    if (trimmedPath.StartsWith("~/")) {
      var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
      return Path.Combine(home, trimmedPath[2..]);
    }
    return trimmedPath;
  }
}

