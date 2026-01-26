namespace TheChatbot.Resources;

public enum AiChatMessageType {
  Text,
  Button
}

public class AiChatResponse {
  public required string Text;
  public required AiChatMessageType Type;
  public required IEnumerable<string> Buttons;
}

public enum AiChatRole {
  System,
  Assistant,
  User
}

public class AiChatMessage {
  public required AiChatRole Role;
  public required string Text;
  public required AiChatMessageType Type;
  public IEnumerable<string> Buttons { get; set; } = [];
}

public interface IAiChatGateway {
  public Task<AiChatResponse> GetResponse(string phoneNumber, List<AiChatMessage> messages, bool allowMcp = true);
  public Task<string> GenerateSummary(List<AiChatMessage> messages, string? existingSummary);
}
