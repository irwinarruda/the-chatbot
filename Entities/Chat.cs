namespace TheChatbot.Entities;

public enum ChatType {
  WhatsApp
}

public class Chat {
  public Guid Id { get; set; }
  public Guid IdUser { get; set; }
  public ChatType Type { get; set; }
  public List<Message> Messages { get; set; }
  public DateTime CreatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;
  public DateTime UpdatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;
  public Chat() {
    Id = Guid.NewGuid();
    Type = ChatType.WhatsApp;
    Messages = [];
  }

  public Message AddUserTextMessage(string text) {
    var message = new Message {
      IdChat = Id,
      IdUser = IdUser,
      UserType = MessageUserType.User,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddBotTextMessage(string text) {
    var message = new Message {
      IdChat = Id,
      IdUser = null,
      UserType = MessageUserType.Bot,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }
}
