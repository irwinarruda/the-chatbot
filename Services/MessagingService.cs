using TheChatbot.Resources;

namespace TheChatbot.Services;

public class MessagingService {
  private readonly IWhatsAppMessagingGateway whatsAppMessagingGateway;
  public MessagingService(IWhatsAppMessagingGateway _whatsAppMessagingGateway) {
    whatsAppMessagingGateway = _whatsAppMessagingGateway;
    _whatsAppMessagingGateway.SubscribeToReceiveMessage(ListenToReceiveMessage);
  }
  async Task SendMessage(string phoneNumber, string text) {
    await whatsAppMessagingGateway.SendTextMessage(new SendTextMessageDTO {
      To = phoneNumber,
      Text = text
    });
  }

  async Task ListenToReceiveMessage(ReceiveTextMessageDTO receiveTextMessage) {
    throw new NotImplementedException();
  }
}
