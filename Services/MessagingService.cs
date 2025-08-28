using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Utils;

namespace TheChatbot.Services;

public class MessagingService(AppDbContext database, AuthService authService, IWhatsAppMessagingGateway whatsAppMessagingGateway, IAiChatGateway aiChatGateway) {
  public async Task ReceiveMessage(JsonElement data) {
    whatsAppMessagingGateway.ReceiveMessage(data, out var receiveTextMessage, out var receiveButtonReply);
    if (receiveTextMessage != null) await ListenToMessage(receiveTextMessage);
    if (receiveButtonReply != null) await ListenToMessage(receiveButtonReply);
  }

  public async Task ListenToMessage<T>(T receiveMessage) where T : ReceiveMessageDTO {
    if (await IsMessageDuplicate(receiveMessage.IdProvider)) return;
    var chat = await GetChatByPhoneNumber(receiveMessage.From);
    if (chat == null) {
      chat = new Chat { PhoneNumber = receiveMessage.From };
      await CreateChat(chat);
    }
    var message = receiveMessage switch {
      ReceiveTextMessageDTO m => chat.AddUserTextMessage(m.Text, m.IdProvider),
      ReceiveInteractiveButtonMessageDTO m => chat.AddUserButtonReply(m.ButtonReply, m.IdProvider),
      _ => chat.AddUserTextMessage("", "")
    };
    await CreateMessage(message);
    if (chat.IdUser == null) {
      var user = await authService.GetUserByPhoneNumber(chat.PhoneNumber);
      if (user == null) {
        await SendTextMessage(chat.PhoneNumber, MessageLoader.GetMessage(MessageTemplate.ThankYou, new() {
          LoginUrl = authService.GetAppLoginUrl(chat.PhoneNumber),
        }));
        return;
      }
      chat.AddUser(user.Id);
      await SaveChat(chat);
    }
    var messages = chat.Messages.Select(m => new AiChatMessage {
      Role = m.UserType == MessageUserType.Bot ? AiChatRole.Assistant : AiChatRole.User,
      Type = m.Type == MessageType.ButtonReply ? AiChatMessageType.Button : AiChatMessageType.Text,
      Text = m.ButtonReply ?? m.Text ?? string.Empty,
      Buttons = m.ButtonReplyOptions ?? []
    });
    var response = await aiChatGateway.GetResponse(chat.PhoneNumber, [.. messages]);
    await (response.Type switch {
      AiChatMessageType.Text => SendTextMessage(chat.PhoneNumber, response.Text, chat),
      AiChatMessageType.Button => SendButtonReplyMessage(chat.PhoneNumber, response.Text, [.. response.Buttons], chat),
      _ => Task.CompletedTask
    });
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
    await whatsAppMessagingGateway.SendTextMessage(new() {
      To = phoneNumber,
      Text = text
    });
  }

  public async Task SendButtonReplyMessage(string phoneNumber, string text, List<string> options, Chat? chat = null) {
    chat ??= await GetChatByPhoneNumber(phoneNumber);
    if (chat == null) {
      throw new ValidationException(
        "The user does not have an open chat",
        "Please create a chat first before continuing"
      );
    }
    var message = chat.AddBotButtonReply(text, options);
    await CreateMessage(message);
    await whatsAppMessagingGateway.SendInteractiveReplyButtonMessage(new() {
      To = chat.PhoneNumber,
      Text = text,
      Buttons = options
    });
  }

  public async Task SendSignedInMessage(string phoneNumber) {
    await SendTextMessage(phoneNumber, MessageLoader.GetMessage(MessageTemplate.SignedIn));
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
        Type = Enum.Parse<MessageType>(m.Type),
        ButtonReply = m.ButtonReply,
        ButtonReplyOptions = m.ButtonReplyOptions != null ? [..m.ButtonReplyOptions.Split(",")] : null,
        UserType = Enum.Parse<MessageUserType>(m.UserType),
        IdProvider = m.IdProvider,
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
    var buttonOptions = message.ButtonReplyOptions != null ? string.Join(",", message.ButtonReplyOptions) : null;
    await database.Execute($@"
      INSERT INTO messages (id, id_chat, type, user_type, text, button_reply, button_reply_options, id_provider, created_at, updated_at)
      VALUES ({message.Id}, {message.IdChat}, {message.Type.ToString()}, {message.UserType.ToString()}, {message.Text}, {message.ButtonReply}, {buttonOptions}, {message.IdProvider}, {message.CreatedAt}, {message.UpdatedAt})
    ");
  }

  private async Task SaveChat(Chat chat) {
    await database.Execute($@"
      UPDATE chats SET
        id_user = {chat.IdUser},
        type = {chat.Type.ToString()},
        phone_number = {chat.PhoneNumber},
        updated_at = {chat.UpdatedAt}
      WHERE id = {chat.Id}
    ");
  }

  private async Task<bool> IsMessageDuplicate(string idProvider) {
    return await database.Query<bool>($@"
      SELECT EXISTS(
        SELECT 1 FROM messages
        WHERE id_provider = {idProvider}
      ) AS ""Value""
    ").SingleOrDefaultAsync();
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
    Guid IdChat,
    string UserType,
    string Type,
    string? Text,
    string? ButtonReply,
    string? ButtonReplyOptions,
    string? IdProvider,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
