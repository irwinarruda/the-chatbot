using TheChatbot.Entities.Extensions;
namespace TheChatbot.Entities;

public enum MessageType {
  Text,
  ButtonReply
}

public enum MessageUserType {
  User,
  Bot
}

public class Message {
  public Guid Id { get; set; }
  public required Guid IdChat { get; set; }
  public required MessageType Type { get; set; }
  public required MessageUserType UserType { get; set; }
  public string? Text { get; set; }
  public string? ButtonReply { get; set; }
  public List<string>? ButtonReplyOptions { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Message() {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }
}
