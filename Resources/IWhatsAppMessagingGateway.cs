using System.Text.Json;

namespace TheChatbot.Resources;

public class SendTextMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
}

public class SendInteractiveButtonMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
  public required IEnumerable<string> Buttons { get; set; }
}

public class ReceiveTextMessageDTO {
  public required string Text { get; set; }
  public required string From { get; set; }
  public required DateTime CreatedAt { get; set; }
}

public class ReceiveInteractiveButtonMessageDTO {
  public required string Text { get; set; }
  public required string From { get; set; }
  public required DateTime CreatedAt { get; set; }
}

public interface IWhatsAppMessagingGateway {
  Task SendTextMessage(SendTextMessageDTO textMessage);
  Task SendInteractiveButtonMessage(SendInteractiveButtonMessageDTO buttonMessage);
  void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage, out ReceiveInteractiveButtonMessageDTO? receiveButtonReply);
  string GetVerifyToken();
  string GetAllowedDomain();
}
