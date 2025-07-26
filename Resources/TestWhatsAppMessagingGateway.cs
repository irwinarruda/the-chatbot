
namespace TheChatbot.Resources;

public class TestWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  public List<Func<ReceiveTextMessageDTO, Task>> subscribers;
  public string phoneNumber = "+5511984444444";
  public TestWhatsAppMessagingGateway() {
    subscribers = [];
  }

  public Task SendTextMessage(SendTextMessageDTO textMessage) {
    return Task.CompletedTask;
  }

  public Task StartReceiveMessage() {
    NotifySubscribers(new ReceiveTextMessageDTO {
      From = phoneNumber,
      Text = "User 1"
    });
    return Task.CompletedTask;
  }

  public Action SubscribeToReceiveMessage(Func<ReceiveTextMessageDTO, Task> onMessageReceived) {
    subscribers.Add(onMessageReceived);
    return () => subscribers.Remove(onMessageReceived);
  }

  private void NotifySubscribers(ReceiveTextMessageDTO receiveMessage) {
    foreach (var subscriber in subscribers) {
      subscriber.Invoke(receiveMessage);
    }
  }
}
