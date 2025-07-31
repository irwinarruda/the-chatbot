using System.Text.Json;

namespace TheChatbot.Resources;

public class SendTextMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
}

public class ReceiveTextMessageDTO {
  public required string Text { get; set; }
  public required string From { get; set; }
}

public class ReceiveButtonReplyDTO {
  public required string ButtonId { get; set; }
  public required string ButtonText { get; set; }
  public required string From { get; set; }
}

public interface IWhatsAppMessagingGateway {
  Task SendTextMessage(SendTextMessageDTO textMessage);
  void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage);
  string GetVerifyToken();
}
