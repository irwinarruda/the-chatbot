namespace TheChatbot.Resources;

public class SendTextMessageDTO {
  public required string Text { get; set; }
  public required string To { get; set; }
}

public class ReceiveTextMessageDTO {
  public required string Text { get; set; }
  public required string From { get; set; }
}

public interface IWhatsAppMessagingGateway {
  Task SendTextMessage(SendTextMessageDTO textMessage);
  Action SubscribeToReceiveMessage(Func<ReceiveTextMessageDTO, Task> onMessageReceived);
  Task StartReceiveMessage();
}
