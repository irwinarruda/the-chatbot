using Microsoft.AspNetCore.Identity;

using TheChatbot.Entities.Extensions;
using TheChatbot.Infra;

namespace TheChatbot.Entities;

public enum ChatType {
  WhatsApp
}

public class Chat {
  public Guid Id { get; set; }
  public Guid? IdUser { get; set; }
  public required string PhoneNumber { get; set; }
  public ChatType Type { get; set; }
  public List<Message> Messages { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Chat() {
    Id = Guid.NewGuid();
    Type = ChatType.WhatsApp;
    Messages = [];
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public void AddUser(Guid idUser) {
    if (IdUser != null) {
      throw new ValidationException("There already is a user ID for this chat");
    }
    IdUser = idUser;
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public Message AddUserTextMessage(string text) {
    var message = new Message {
      IdChat = Id,
      UserType = MessageUserType.User,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddBotTextMessage(string text) {
    var message = new Message {
      IdChat = Id,
      UserType = MessageUserType.Bot,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }
}
