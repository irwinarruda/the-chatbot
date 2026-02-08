using System.Text.Json;
using System.Threading.Channels;

namespace TheChatbot.Resources;

public class TuiOutgoingMessage {
  public required string Type { get; set; }
  public required string Text { get; set; }
  public required string To { get; set; }
  public List<string>? Buttons { get; set; }
}

public class TuiWhatsAppMessagingGateway : IWhatsAppMessagingGateway {
  private readonly Channel<TuiOutgoingMessage> channel = Channel.CreateUnbounded<TuiOutgoingMessage>();

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

  public Task<Stream> DownloadMediaAsync(string mediaId) {
    return Task.FromResult<Stream>(new MemoryStream());
  }
}
