namespace TheChatbot.Entities;

public enum ConversationType { }

public class Conversation {
  public Guid Id { get; set; }
  public Guid IdUser { get; set; }
  public ConversationType Type { get; set; }
  public List<Message> Messages { get; set; }
  public Conversation() {
    Messages = [];
  }
}
