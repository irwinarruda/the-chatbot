using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Templates;
using TheChatbot.Utils;

//
namespace TheChatbot.Services;

public class MessagingService(AppDbContext database, IWhatsAppMessagingGateway whatsAppMessagingGateway, AuthService authService, IChatClient? chatClient) {
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

  private async Task<IMcpClient> GetMcpClient() {
    return await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions {
      Name = "PodcastMcpServer",
      Command = "dotnet",
      Arguments = ["run", "--project", "/Users/irwinarruda/Documents/PRO/the-chatbot/Mcp", "--no-build"],
    }), new McpClientOptions {
      Capabilities = new ClientCapabilities {
        Sampling = new SamplingCapability {
          SamplingHandler = chatClient!.CreateSamplingHandler(),
        }
      }
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
    var mcp = await GetMcpClient();
    var tools = await mcp.ListToolsAsync();
    var system = $"You are a chatbot for the app TheChatbot. You are allowed to talk to the person. You are friendly and you are sure of yourself. Your main goal is to use the tools provided to help the user perform some tasks. The user phone number is {receiveTextMessage.From}. Always use the phone number as a param for a tool and use it in the exact formating it is passed {receiveTextMessage.From}.";
    var messages = chat.Messages.Select(m => new ChatMessage {
      Role = m.UserType == MessageUserType.Bot ? ChatRole.Assistant : ChatRole.User,
      Contents = [new() { RawRepresentation = m.Text }],
    });
    var response = await chatClient!.GetResponseAsync([new () {
      Role = ChatRole.System,
      Contents = [new() { RawRepresentation = system }],
    }, ..messages], new() { Tools = [.. tools], AllowMultipleToolCalls = true });
    Console.WriteLine(Printable.Make(response));
    await SendTextMessage(receiveTextMessage.From, response.Text, chat);
  }

  public async Task ReceiveMessage(JsonElement data) {
    whatsAppMessagingGateway.ReceiveMessage(data, out var receiveTextMessage);
    if (receiveTextMessage != null) await ListenToTextMessage(receiveTextMessage);
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
