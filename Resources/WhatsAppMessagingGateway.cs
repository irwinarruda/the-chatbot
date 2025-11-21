using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

using TheChatbot.Entities;

using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;
using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class WhatsAppMessagingGateway(WhatsAppConfig whatsAppConfig, IWhatsAppBusinessClient whatsAppBusinessClient) : IWhatsAppMessagingGateway {
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

  public ReceiveMessageDTO? ReceiveMessage(JsonElement messageReceived) {
    // whatsAppBusinessClient.VerifyCode();
    var jsonElement = messageReceived;
    if (TryDeserializeMessage<GenericMessage>(jsonElement, out var genericMessageData)) {
      var phoneNumberId = genericMessageData.Entry[0].Changes[0].Value.Metadata?.PhoneNumberId;
      if (phoneNumberId != whatsAppConfig.WhatsAppBusinessPhoneNumberId) return null;
    }
    if (TryDeserializeMessage<ReplyButtonMessage>(jsonElement, out var buttonMessageData) && buttonMessageData.Entry[0].Changes[0].Value.Messages[0].Interactive != null) {
      var message = buttonMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = buttonMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).UtcDateTime;
      var receiveButtonReply = new ReceiveInteractiveButtonMessageDTO {
        IdProvider = message.Id,
        ButtonReply = message.Interactive.ButtonReply.Title,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt,
      };
      return receiveButtonReply;
    }
    if (TryDeserializeMessage<TextMessage>(jsonElement, out var textMessageData)) {
      var message = textMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = textMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).UtcDateTime;
      var receiveTextMessage = new ReceiveTextMessageDTO {
        IdProvider = message.Id,
        Text = message.Text.Body,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt,
      };
      return receiveTextMessage;
    }
    return null;
  }

  public bool ValidateWebhook(string hubMode, string hubVerifyToken) {
    return hubMode == "subscribe" && hubVerifyToken == whatsAppConfig.WebhookVerifyToken;
  }

  public bool ValidateSignature(string signature, string body) {
    var appSecret = whatsAppConfig.AppSecret;
    if (string.IsNullOrEmpty(appSecret)) return false;
    if (string.IsNullOrEmpty(signature)) return false;
    var signatureParts = signature.Split('=');
    if (signatureParts.Length != 2 || signatureParts[0] != "sha256") return false;
    var hash = signatureParts[1];
    using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(appSecret));
    var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
    var computedHashString = Convert.ToHexStringLower(computedHash);
    return CryptographicOperations.FixedTimeEquals(
      Encoding.UTF8.GetBytes(hash),
      Encoding.UTF8.GetBytes(computedHashString)
    );
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
