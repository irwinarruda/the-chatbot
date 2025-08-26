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
      Text = new() {
        Body = textMessage.Text
      }
    });
  }

  public async Task SendInteractiveReplyButtonMessage(SendInteractiveReplyButtonMessageDTO buttonMessage) {
    await whatsAppBusinessClient.SendInteractiveReplyButtonMessageAsync(new InteractiveReplyButtonMessageRequest {
      To = buttonMessage.To,
      Interactive = new() {
        Body = new() { Text = buttonMessage.Text },
        Action = new() {
          Buttons = [.. buttonMessage.Buttons.Select((b, i) => new ReplyButton { Type = "reply", Reply = new() { Id = $"btn_{i + 1}", Title = b } })]
        }
      },
    });
  }

  public void ReceiveMessage(JsonElement messageReceived, out ReceiveTextMessageDTO? receiveTextMessage, out ReceiveInteractiveButtonMessageDTO? receiveButtonReply) {
    receiveTextMessage = null;
    receiveButtonReply = null;
    var jsonElement = messageReceived;
    if (TryDeserializeMessage<ReplyButtonMessage>(jsonElement, out var buttonMessageData) && buttonMessageData.Entry[0].Changes[0].Value.Messages[0].Interactive != null) {
      var message = buttonMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = buttonMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).UtcDateTime;
      receiveButtonReply = new() {
        IdProvider = message.Id,
        ButtonReply = message.Interactive.ButtonReply.Title,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt,
      };
      return;
    }
    if (TryDeserializeMessage<TextMessage>(jsonElement, out var textMessageData)) {
      var message = textMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = textMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).UtcDateTime;
      receiveTextMessage = new() {
        IdProvider = message.Id,
        Text = message.Text.Body,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt,
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
