using System.Text.Json;

using TheChatbot.Entities.Extensions;

namespace TheChatbot.Resources;

public class TestWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  public List<Func<ReceiveTextMessageDTO, Task>> subscribers = [];
  public string phoneNumber = "5511984444444";
  public static string FixedIdProvider = Guid.NewGuid().ToString();

  public Task SendTextMessage(SendTextMessageDTO textMessage) {
    return Task.CompletedTask;
  }

  public Task SendInteractiveReplyButtonMessage(SendInteractiveReplyButtonMessageDTO buttonMessage) {
    return Task.CompletedTask;
  }


  public void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage, out ReceiveInteractiveButtonMessageDTO? receiveButtonReply) {
    receiveTextMessage = new() {
      From = phoneNumber,
      Text = messageReceived.ToString(),
      CreatedAt = DateTime.UtcNow.TruncateToMicroseconds(),
      IdProvider = messageReceived.ToString().Contains("Second duplicate") ? FixedIdProvider : Guid.NewGuid().ToString(),
    };
    receiveButtonReply = null;
  }

  public string GetVerifyToken() {
    return "ValidToken";
  }
}
