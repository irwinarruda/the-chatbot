
namespace TheChatbot.Resources;

public class TestWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  public List<Func<ReceiveTextMessageDTO, Task>> subscribers;
  public TestWhatsAppMessagingGateway() {
    subscribers = [];
  }

  public Task SendTextMessage(SendTextMessageDTO textMessage) {
    var phoneNumber = "+5511984444444";
    SendToSubscribers(new ReceiveTextMessageDTO {
      From = phoneNumber,
      Text = "The message sent was responded"
    });
    return Task.CompletedTask;
  }

  public async Task StartReceiveMessage() {
    var phoneNumber = "+5511984444444";
    var mockMessages = new[] {
      "First message",
      "Second message",
      "Third message",
      "Another one",
    };
    foreach (var message in mockMessages) {
      await Task.Delay(50);
      SendToSubscribers(new ReceiveTextMessageDTO {
        From = phoneNumber,
        Text = message
      });
    }
    SendToSubscribers(new ReceiveTextMessageDTO {
      From = "+556399999999",
      Text = "Phone number that does not exist"
    });
  }

  public Action SubscribeToReceiveMessage(Func<ReceiveTextMessageDTO, Task> onMessageReceived) {
    subscribers.Add(onMessageReceived);
    return () => subscribers.Remove(onMessageReceived);
  }

  private void SendToSubscribers(ReceiveTextMessageDTO receiveMessage) {
    foreach (var subscriber in subscribers) {
      subscriber.Invoke(receiveMessage);
    }
  }
}
