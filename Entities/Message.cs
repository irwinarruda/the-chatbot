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
  public DateTime CreatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;
  public DateTime UpdatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;
  public Message() {
    Id = Guid.NewGuid();
  }
}
