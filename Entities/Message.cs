using TheChatbot.Entities.Extensions;
namespace TheChatbot.Entities;

public enum MessageUserType {
  User,
  Bot
}

public class Message {
  public Guid Id { get; set; }
  public Guid? IdUser { get; set; }
  public Guid IdChat { get; set; }
  public MessageUserType UserType { get; set; }
  public string? Text { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Message() {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }
}
