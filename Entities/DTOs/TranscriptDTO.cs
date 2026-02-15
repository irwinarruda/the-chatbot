namespace TheChatbot.Entities.DTOs;

public class TranscriptDTO {
  public Guid Id { get; set; }
  public required string Transcript { get; set; }
  public string? MediaUrl { get; set; }
  public string? MimeType { get; set; }
  public DateTime CreatedAt { get; set; }
}
