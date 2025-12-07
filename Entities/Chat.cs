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
  public bool IsDeleted { get; set; }
  public Chat() {
    Id = Guid.NewGuid();
    Type = ChatType.WhatsApp;
    Messages = [];
    IsDeleted = false;
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

  public void DeleteChat() {
    if (IsDeleted) {
      throw new ValidationException("The chat is already deleted");
    }
    IsDeleted = true;
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public Message AddUserTextMessage(string text, string? idProvider = null) {
    var message = new Message {
      IdChat = Id,
      IdProvider = idProvider,
      Type = MessageType.Text,
      UserType = MessageUserType.User,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddBotTextMessage(string text, string? idProvider = null) {
    var message = new Message {
      IdChat = Id,
      IdProvider = idProvider,
      Type = MessageType.Text,
      UserType = MessageUserType.Bot,
      Text = text,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddUserButtonReply(string reply, string? idProvider = null) {
    var message = new Message {
      IdChat = Id,
      IdProvider = idProvider,
      Type = MessageType.ButtonReply,
      UserType = MessageUserType.User,
      ButtonReply = reply,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddBotButtonReply(string replyText, List<string> buttons, string? idProvider = null) {
    var message = new Message {
      IdChat = Id,
      IdProvider = idProvider,
      Type = MessageType.ButtonReply,
      UserType = MessageUserType.Bot,
      Text = replyText,
      ButtonReplyOptions = buttons,
    };
    Messages.Add(message);
    return message;
  }

  public Message AddUserAudioMessage(string mediaId, string mimeType, string? idProvider = null) {
    var message = new Message {
      IdChat = Id,
      IdProvider = idProvider,
      Type = MessageType.Audio,
      UserType = MessageUserType.User,
      MediaId = mediaId,
      MimeType = mimeType,
    };
    Messages.Add(message);
    return message;
  }
}
