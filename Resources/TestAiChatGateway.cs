namespace TheChatbot.Resources;

public class TestAiChatGateway : IAiChatGateway {
  public Task<AiChatResponse> GetResponse(string phoneNumber, List<AiChatMessage> messages) {
    var lastMessage = messages.LastOrDefault(m => m.Role == AiChatRole.User)?.Text ?? "";

    return Task.FromResult(new AiChatResponse {
      Text = "Response to: " + lastMessage.Trim(),
      Type = AiChatResponseType.Text,
      Buttons = Array.Empty<string>()
    });
  }
}
