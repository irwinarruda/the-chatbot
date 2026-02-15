using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

using TheChatbot.Entities;

using WhatsappBusiness.CloudApi.Interfaces;
using WhatsappBusiness.CloudApi.Messages.Requests;
using WhatsappBusiness.CloudApi.Webhook;
using TheChatbot.Infra;
using TheChatbot.Utils;

namespace TheChatbot.Resources;

public class WhatsAppMessagingGateway(WhatsAppConfig whatsAppConfig, IWhatsAppBusinessClient whatsAppBusinessClient) : IWhatsAppMessagingGateway {
  public async Task SendTextMessage(SendTextMessageDTO textMessage) {
    var chunks = WhatsAppTextChunker.Chunk(textMessage.Text);
    foreach (var chunk in chunks) {
      await whatsAppBusinessClient.SendTextMessageAsync(new TextMessageRequest {
        To = textMessage.To,
        Text = new() {
          Body = chunk
        }
      });
    }
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
    var jsonElement = messageReceived;
    if (TryDeserializeMessage<GenericMessage>(jsonElement, out var genericMessageData)) {
      var phoneNumberId = genericMessageData.Entry[0].Changes[0].Value.Metadata?.PhoneNumberId;
      if (phoneNumberId != whatsAppConfig.WhatsAppBusinessPhoneNumberId) return null;
    }
    if (TryDeserializeMessage<AudioMessage>(jsonElement, out var audioMessageData) && audioMessageData.Entry[0].Changes[0].Value.Messages[0].Audio != null) {
      var message = audioMessageData.Entry[0].Changes[0].Value.Messages[0];
      var contact = audioMessageData.Entry[0].Changes[0].Value.Contacts[0];
      var createdAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(message.Timestamp)).UtcDateTime;
      var receiveAudioMessage = new ReceiveAudioMessageDTO {
        IdProvider = message.Id,
        MediaId = message.Audio.Id,
        MimeType = message.Audio.MimeType,
        From = PhoneNumberUtils.AddDigitNine(contact.WaId),
        CreatedAt = createdAt,
      };
      return receiveAudioMessage;
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

  public async Task<Stream> DownloadMediaAsync(string mediaId) {
    var mediaUrlResponse = await whatsAppBusinessClient.GetMediaUrlAsync(mediaId);
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", whatsAppConfig.AccessToken);
    var response = await httpClient.GetAsync(mediaUrlResponse.Url);
    response.EnsureSuccessStatusCode();
    var memoryStream = new MemoryStream();
    await response.Content.CopyToAsync(memoryStream);
    memoryStream.Position = 0;
    return memoryStream;
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
