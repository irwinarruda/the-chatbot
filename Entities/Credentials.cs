namespace TheChatbot.Entities;

public enum CredentialType {
  Google,
}

public class Credentials {
  public Guid Id { get; set; }
  public Guid IdUser { get; set; }
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public long? ExpiresInSeconds { get; set; }
  public required CredentialType Type { get; set; }
}
