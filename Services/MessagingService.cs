using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

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
    await whatsAppMessagingGateway.SendTextMessage(new SendTextMessageDTO {
      To = phoneNumber,
      Text = text
    });
  }

  public async Task ListenToReceiveMessage(ReceiveTextMessageDTO receiveTextMessage) {
    await authService.GetUserByPhoneNumber(receiveTextMessage.From);
  }

  public async Task<Conversation?> GetConversationByUserPhoneNumber(string phoneNumber) {
    return new Conversation { };
  }
}
