using TheChatbot.Entities.Extensions;

namespace TheChatbot.Entities;

public enum CredentialType {
  Google,
}

public class Credential {
  public Guid Id { get; set; }
  public Guid IdUser { get; set; }
  public string AccessToken { get; set; } = string.Empty;
  public string RefreshToken { get; set; } = string.Empty;
  public long? ExpiresInSeconds { get; set; }
  public DateTime? ExpirationDate { get; set; }
  public required CredentialType Type { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public Credential() {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public Credential(long? expiresInSeconds) {
    ExpiresInSeconds = expiresInSeconds;
    if (expiresInSeconds != null) {
      ExpirationDate = DateTime.UtcNow.TruncateToMicroseconds().AddSeconds((double)expiresInSeconds);
    }
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public void Update(string accessToken, string refreshToken, long? expiresInSeconds) {
    AccessToken = accessToken;
    RefreshToken = refreshToken;
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    ExpiresInSeconds = expiresInSeconds;
    if (expiresInSeconds != null) {
      ExpirationDate = DateTime.UtcNow.TruncateToMicroseconds().AddSeconds((double)expiresInSeconds);
    }
  }
}
