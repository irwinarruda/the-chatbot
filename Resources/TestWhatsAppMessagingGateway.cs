using System.Text.Json;

using TheChatbot.Entities.Extensions;

namespace TheChatbot.Resources;

public class TestWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  public List<Func<ReceiveTextMessageDTO, Task>> subscribers = [];
  public static string PhoneNumber = "5511984444444";
  public static string FixedIdProvider = Guid.NewGuid().ToString();

  public Task SendTextMessage(SendTextMessageDTO textMessage) {
    return Task.CompletedTask;
  }

  public Task SendInteractiveReplyButtonMessage(SendInteractiveReplyButtonMessageDTO buttonMessage) {
    return Task.CompletedTask;
  }

  public ReceiveMessageDTO? ReceiveMessage(JsonElement messageReceived) {
    var receiveTextMessage = new ReceiveTextMessageDTO {
      From = PhoneNumber,
      Text = messageReceived.ToString(),
      CreatedAt = DateTime.UtcNow.TruncateToMicroseconds(),
      IdProvider = messageReceived.ToString().Contains("Second duplicate") ? FixedIdProvider : Guid.NewGuid().ToString(),
    };
    return receiveTextMessage;
  }

  public bool ValidateWebhook(string _, string __) {
    return true;
  }

  public bool ValidateSignature(string signature, string body) {
    return true;
  }

  public Task<Stream> DownloadMediaAsync(string mediaId) {
    return Task.FromResult<Stream>(new MemoryStream());
  }
}
