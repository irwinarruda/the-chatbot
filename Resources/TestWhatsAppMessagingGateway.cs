using System.Text.Json;

using TheChatbot.Entities.Extensions;

namespace TheChatbot.Resources;

public class TestWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  public List<Func<ReceiveTextMessageDTO, Task>> subscribers = [];
  public string phoneNumber = "5511984444444";

  public Task SendTextMessage(SendTextMessageDTO textMessage) {
    return Task.CompletedTask;
  }

  public void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage) {
    receiveTextMessage = new ReceiveTextMessageDTO {
      From = phoneNumber,
      Text = messageReceived.ToString(),
      CreatedAt = DateTime.UtcNow.TruncateToMicroseconds()
    };
  }

  public string GetVerifyToken() {
    return "ValidToken";
  }

  public string GetAllowedDomain() {
    return "https://graph.facebook.com";
  }
}
