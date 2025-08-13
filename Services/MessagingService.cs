using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Templates;
using TheChatbot.Utils;

namespace TheChatbot.Services;

public class MessagingService(AppDbContext database, IWhatsAppMessagingGateway whatsAppMessagingGateway, AuthService authService, IAiChatGateway aiChatGateway) {
  public async Task SendSignedInMessage(string phoneNumber) {
    await SendTextMessage(phoneNumber, SignedInMessage.Get());
  }

  public string GetAllowedDomain() {
    return whatsAppMessagingGateway.GetAllowedDomain();
  }

  public async Task SendTextMessage(string phoneNumber, string text, Chat? chat = null) {
    chat ??= await GetChatByPhoneNumber(phoneNumber);
    if (chat == null) {
      throw new ValidationException(
        "The user does not have an open chat",
        "Please create a chat first before continuing"
      );
    }
    var message = chat.AddBotTextMessage(text);
    await CreateMessage(message);
    await whatsAppMessagingGateway.SendTextMessage(new SendTextMessageDTO {
      To = phoneNumber,
      Text = text
    });
  }

  public async Task ListenToTextMessage(ReceiveTextMessageDTO receiveTextMessage) {
    var chat = await GetChatByPhoneNumber(receiveTextMessage.From);
    if (chat == null) {
      chat = new Chat { PhoneNumber = receiveTextMessage.From };
      await CreateChat(chat);
    }
    var message = chat.AddUserTextMessage(receiveTextMessage.Text);
    await CreateMessage(message);
    if (chat.IdUser == null) {
      var user = await authService.GetUserByPhoneNumber(receiveTextMessage.From);
      if (user == null) {
        await SendTextMessage(
          receiveTextMessage.From,
          ThankYouMessage.Get(authService.GetAppLoginUrl(receiveTextMessage.From)),
          chat
        );
        return;
      }
      chat.AddUser(user.Id);
      await SaveChat(chat);
    }
    var messages = chat.Messages.Select(m => new AiChatMessage {
      Role = m.UserType == MessageUserType.Bot ? AiChatRole.Assistant : AiChatRole.User,
      Text = m.Text ?? "No message from the user",
    });
    var response = await aiChatGateway.GetResponse(receiveTextMessage.From, [.. messages]);
    if (response.Type == AiChatResponseType.Button) {
      var m = chat.AddBotTextMessage(response.Text);
      await CreateMessage(m);
      await whatsAppMessagingGateway.SendInteractiveButtonMessage(new SendInteractiveButtonMessageDTO {
        To = receiveTextMessage.From,
        Text = response.Text,
        Buttons = response.Buttons!
      });
    } else await SendTextMessage(receiveTextMessage.From, response.Text, chat);
  }

  public async Task ReceiveMessage(JsonElement data) {
    whatsAppMessagingGateway.ReceiveMessage(data, out var receiveTextMessage, out var receiveButtonReply);
    if (receiveTextMessage != null) await ListenToTextMessage(receiveTextMessage);
    if (receiveButtonReply != null) await ListenToTextMessage(new() { Text = receiveButtonReply.Text, From = receiveButtonReply.From, CreatedAt = receiveButtonReply.CreatedAt });
  }

  public void ValidateWebhook(string hubMode, string hubVerifyToken) {
    if (hubMode != "subscribe" || hubVerifyToken != whatsAppMessagingGateway.GetVerifyToken()) {
      throw new ValidationException("The provided token did not match");
    }
  }

  public async Task<Chat?> GetChatByPhoneNumber(string phoneNumber) {
    var dbChat = await database.Query<DbChat>($@"
      SELECT * FROM chats
      WHERE phone_number = {phoneNumber}
      ORDER BY created_at DESC
    ").FirstOrDefaultAsync();
    if (dbChat == null) return null;
    var dbMessages = await database.Query<DbMessage>($@"
      SELECT * FROM messages
      WHERE id_chat = {dbChat.Id}
      ORDER BY created_at ASC
    ").ToListAsync();
    return new Chat {
      Id = dbChat.Id,
      IdUser = dbChat.IdUser,
      Type = Enum.Parse<ChatType>(dbChat.Type),
      PhoneNumber = dbChat.PhoneNumber,
      Messages = [..dbMessages.Select((m) => new Message {
        Id = m.Id,
        IdChat = m.IdChat,
        Text = m.Text,
        UserType = Enum.Parse<MessageUserType>(m.UserType),
        CreatedAt = m.CreatedAt,
        UpdatedAt = m.UpdatedAt,
      })],
      CreatedAt = dbChat.CreatedAt,
      UpdatedAt = dbChat.UpdatedAt,
    };
  }

  private async Task CreateChat(Chat chat) {
    await database.Execute($@"
      INSERT INTO chats (id, id_user, type, phone_number, created_at, updated_at)
      VALUES ({chat.Id}, {chat.IdUser}, {chat.Type.ToString()}, {chat.PhoneNumber}, {chat.CreatedAt}, {chat.UpdatedAt})
    ");
    if (chat.Messages.Count == 0) return;
    foreach (var message in chat.Messages) {
      await CreateMessage(message);
    }
  }

  private async Task CreateMessage(Message message) {
    await database.Execute($@"
      INSERT INTO messages (id, id_chat, user_type, text, created_at, updated_at)
      VALUES ({message.Id}, {message.IdChat}, {message.UserType.ToString()}, {message.Text}, {message.CreatedAt}, {message.UpdatedAt})
    ");
  }

  private async Task SaveChat(Chat chat) {
    await database.Execute($@"
      UPDATE chats SET
        id_user = {chat.IdUser},
        type = {chat.Type},
        phone_number = {chat.PhoneNumber},
        updated_at = {chat.UpdatedAt}
      WHERE id = {chat.Id}
    ");
  }

  public record DbChat(
    Guid Id,
    Guid? IdUser,
    string Type,
    string PhoneNumber,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );

  public record DbMessage(
    Guid Id,
    Guid? IdUser,
    Guid IdChat,
    string UserType,
    string? Text,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
