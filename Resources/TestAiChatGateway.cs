namespace TheChatbot.Resources;

public class TestAiChatGateway : IAiChatGateway {
  public Task<AiChatResponse> GetResponse(string phoneNumber, List<AiChatMessage> messages, bool allowMcp = false) {
    var lastMessage = messages.LastOrDefault(m => m.Role == AiChatRole.User)?.Text ?? "";

    return Task.FromResult(new AiChatResponse {
      Text = "Response to: " + lastMessage.Trim(),
      Type = AiChatMessageType.Text,
      Buttons = [],
    });
  }

  public Task<string> GenerateSummary(List<AiChatMessage> messages, string? existingSummary) {
    var summary = $"Summary of {messages.Count} messages";
    if (!string.IsNullOrEmpty(existingSummary)) {
      summary = $"{existingSummary} + {summary}";
    }
    return Task.FromResult(summary);
  }
}
