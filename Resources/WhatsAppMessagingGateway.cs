using System.Text.Json;

using TheChatbot.Entities;

using WhatsappBusiness.CloudApi.Configurations;
using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;

namespace TheChatbot.Resources;

public class WhatsAppMessagingGateway(WhatsAppBusinessCloudApiConfig whatsAppBusinessCloudApiConfig, IWhatsAppBusinessClient whatsAppBusinessClient) : IWhatsAppMessagingGateway {
  public async Task SendTextMessage(SendTextMessageDTO textMessage) {
    await whatsAppBusinessClient.SendTextMessageAsync(new TextMessageRequest {
      To = textMessage.To,
      Text = new WhatsAppText {
        Body = textMessage.Text
      }
    });
  }

  public void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage) {
    receiveTextMessage = null;
    var jsonElement = messageReceived;
    if (TryDeserializeMessage<TextMessage>(jsonElement, out var textMessageData)) {
      var message = textMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = textMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).DateTime;
      receiveTextMessage = new ReceiveTextMessageDTO {
        Text = message.Text.Body,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt
      };
      return;
    }
  }

  public string GetVerifyToken() {
    return whatsAppBusinessCloudApiConfig.WebhookVerifyToken;
  }

  public string GetAllowedDomain() {
    return "https://graph.facebook.com";
  }

  private static bool TryDeserializeMessage<T>(JsonElement jsonElement, out MessageReceived<T> messageData) where T : IGenericMessage {
    try {
      messageData = jsonElement.Deserialize<MessageReceived<T>>()!;
      return messageData?.Entry?[0]?.Changes?[0]?.Value?.Messages?.Count > 0;
    } catch {
      messageData = new MessageReceived<T>();
      return false;
    }
  }
}
