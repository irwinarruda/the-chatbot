using System.Text.Json;
using System.Threading.Channels;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class TuiOutgoingMessage {
  public required string Type { get; set; }
  public required string Text { get; set; }
  public required string To { get; set; }
  public List<string>? Buttons { get; set; }
}

public class TuiWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  private readonly Channel<TuiOutgoingMessage> channel = Channel.CreateUnbounded<TuiOutgoingMessage>();
  private readonly Dictionary<string, byte[]> mediaById = [];

  public async Task SendTextMessage(SendTextMessageDTO textMessage) {
    await channel.Writer.WriteAsync(new TuiOutgoingMessage {
      Type = "text",
      Text = textMessage.Text,
      To = textMessage.To,
    });
  }

  public async Task SendInteractiveReplyButtonMessage(SendInteractiveReplyButtonMessageDTO buttonMessage) {
    await channel.Writer.WriteAsync(new TuiOutgoingMessage {
      Type = "button",
      Text = buttonMessage.Text,
      To = buttonMessage.To,
      Buttons = [.. buttonMessage.Buttons],
    });
  }

  public ChannelReader<TuiOutgoingMessage> GetOutgoingMessages() {
    return channel.Reader;
  }

  public ReceiveMessageDTO? ReceiveMessage(JsonElement messageReceived) {
    return null;
  }

  public bool ValidateWebhook(string hubMode, string hubVerifyToken) {
    return true;
  }

  public bool ValidateSignature(string signature, string body) {
    return true;
  }

  public async Task<string> SaveMediaAsync(Stream mediaStream) {
    var mediaId = Guid.NewGuid().ToString();
    using var memoryStream = new MemoryStream();
    await mediaStream.CopyToAsync(memoryStream);
    mediaById[mediaId] = memoryStream.ToArray();
    return mediaId;
  }

  public Task<Stream> DownloadMediaAsync(string mediaId) {
    if (!mediaById.TryGetValue(mediaId, out var mediaBytes)) {
      throw new NotFoundException("Audio file was not found in the TUI media store");
    }
    return Task.FromResult<Stream>(new MemoryStream(mediaBytes));
  }
}
