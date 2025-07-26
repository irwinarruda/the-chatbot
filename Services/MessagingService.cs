using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;

namespace TheChatbot.Services;

public class MessagingService {
  private readonly AppDbContext database;
  private readonly IWhatsAppMessagingGateway whatsAppMessagingGateway;
  private readonly AuthService authService;

  public MessagingService(AppDbContext _database, IWhatsAppMessagingGateway _whatsAppMessagingGateway, AuthService _authService) {
    _whatsAppMessagingGateway.SubscribeToReceiveMessage(ListenToReceiveMessage);
    whatsAppMessagingGateway = _whatsAppMessagingGateway;
    authService = _authService;
    database = _database;
  }

  public async Task SendMessage(string phoneNumber, string text) {
    var chat = await EnsureCreatedChat(phoneNumber);
    var message = chat.AddBotTextMessage(text);
    await CreateMessage(message);
    await whatsAppMessagingGateway.SendTextMessage(new SendTextMessageDTO {
      To = phoneNumber,
      Text = text
    });
  }

  public async Task ListenToReceiveMessage(ReceiveTextMessageDTO receiveTextMessage) {
    var chat = await EnsureCreatedChat(receiveTextMessage.From);
    var message = chat.AddUserTextMessage(receiveTextMessage.Text);
    await CreateMessage(message);
    await SendMessage(receiveTextMessage.From, "Response to: " + receiveTextMessage.Text);
  }

  public async Task<Chat> EnsureCreatedChat(string phoneNumber) {
    var chat = await GetChatByUserPhoneNumber(phoneNumber);
    if (chat == null) {
      var user = await authService.GetUserByPhoneNumber(phoneNumber) ?? throw new NotFoundException(
        $"The user with the phone number {phoneNumber} was not found",
        "Please create tell the user to first login with Google"
      );
      chat = new Chat { IdUser = user.Id };
      await CreateChat(chat);
    }
    return chat;
  }

  public async Task<Chat?> GetChatByUserPhoneNumber(string phoneNumber) {
    var dbChat = await database.Query<DbChat>($@"
      SELECT c.id, c.id_user, c.type FROM chats c
      INNER JOIN users u ON u.id = c.id_user
      WHERE u.phone_number = {phoneNumber}
    ").FirstOrDefaultAsync();
    if (dbChat == null) return null;
    var dbMessages = await database.Query<DbMessage>($@"
      SELECT * FROM messages
      WHERE id_chat = {dbChat.Id}
    ").ToListAsync();

    return new Chat {
      Id = dbChat.Id,
      IdUser = dbChat.IdUser,
      Type = Enum.Parse<ChatType>(dbChat.Type),
      Messages = [..dbMessages.Select((m) => new Message {
        Id = m.Id,
        IdChat = m.IdChat,
        IdUser = m.IdUser,
        Text = m.Text,
        UserType = Enum.Parse<MessageUserType>(m.UserType),
      })]
    };
  }

  public async Task CreateChat(Chat chat) {
    await database.Execute($@"
      INSERT INTO chats (id, id_user, type, created_at, updated_at)
      VALUES ({chat.Id}, {chat.IdUser}, {Enum.GetName(chat.Type)}, {chat.CreatedAt}, {chat.UpdatedAt})
    ");
    if (chat.Messages.Count == 0) return;
    foreach (var message in chat.Messages) {
      await database.Execute($@"
        INSERT INTO messages (id, id_user, id_chat, user_type, text, created_at, updated_at)
        VALUES ({message.Id}, {message.IdUser}, {message.IdChat}, {Enum.GetName(message.UserType)}, {message.Text}, {message.CreatedAt}, {message.UpdatedAt})
      ");
    }
  }

  public async Task CreateMessage(Message message) {
    await database.Execute($@"
      INSERT INTO messages (id, id_user, id_chat, user_type, text, created_at, updated_at)
      VALUES ({message.Id}, {message.IdUser}, {message.IdChat}, {Enum.GetName(message.UserType)}, {message.Text}, {message.CreatedAt}, {message.UpdatedAt})
    ");
  }

  public record DbChat(
    Guid Id,
    Guid IdUser,
    string Type
  );

  public record DbMessage(
    Guid Id,
    Guid? IdUser,
    Guid IdChat,
    string UserType,
    string? Text
  );
}
