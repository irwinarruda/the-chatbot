namespace TheChatbot.Entities;

public enum MessageUserType {
  User,
  Bot
}

public class Message {
  public Guid Id { get; set; }
  public Guid? IdUser { get; set; }
  public MessageUserType UserType { get; set; }
  public string? Text { get; set; }
}
