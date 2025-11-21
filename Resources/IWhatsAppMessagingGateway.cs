using System.Text.Json;

namespace TheChatbot.Resources;

public class SendTextMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
}

public class SendInteractiveReplyButtonMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
  public required IEnumerable<string> Buttons { get; set; }
}

public abstract class ReceiveMessageDTO {
  public required string From;
  public required DateTime CreatedAt { get; set; }
  public required string IdProvider { get; set; }
}

public class ReceiveTextMessageDTO : ReceiveMessageDTO {
  public required string Text { get; set; }
}

public class ReceiveInteractiveButtonMessageDTO : ReceiveMessageDTO {
  public required string ButtonReply { get; set; }
}

public interface IWhatsAppMessagingGateway {
  Task SendTextMessage(SendTextMessageDTO textMessage);
  Task SendInteractiveReplyButtonMessage(SendInteractiveReplyButtonMessageDTO buttonMessage);
  ReceiveMessageDTO? ReceiveMessage(JsonElement messageReceived);
  bool ValidateSignature(string signature, string body);
  bool ValidateWebhook(string hubMode, string hubVerifyToken);
}
