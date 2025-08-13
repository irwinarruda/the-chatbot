namespace TheChatbot.Resources;

public enum AiChatResponseType {
  Text,
  Button
}

public class AiChatResponse {
  public required string Text;
  public required AiChatResponseType Type;
  public List<string>? Buttons;
}

public enum AiChatRole {
  System,
  Assistant,
  User
}

public class AiChatMessage {
  public required AiChatRole Role;
  public required string Text;
}

public interface IAiChatGateway {
  public Task<AiChatResponse> GetResponse(string phoneNumber, List<AiChatMessage> messages);
}
