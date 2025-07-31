using System.Text.Json;
using System.Text.RegularExpressions;

using TheChatbot.Entities;

using WhatsappBusiness.CloudApi.Configurations;
using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;

namespace TheChatbot.Resources;

public class WhatsAppMessagingGateway(WhatsAppBusinessCloudApiConfig _whatsAppBusinessCloudApiConfig, IWhatsAppBusinessClient _whatsAppBusinessClient) : IWhatsAppMessagingGateway {
  private readonly WhatsAppBusinessCloudApiConfig whatsAppBusinessCloudApiConfig = _whatsAppBusinessCloudApiConfig;
  private readonly IWhatsAppBusinessClient whatsAppBusinessClient = _whatsAppBusinessClient;

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
      receiveTextMessage = new ReceiveTextMessageDTO {
        Text = message.Text.Body,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId)
      };
      return;
    }
  }

  public string GetVerifyToken() {
    return whatsAppBusinessCloudApiConfig.WebhookVerifyToken;
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
