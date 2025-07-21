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
  public DateTime CreatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;
  public DateTime UpdatedAt { get; set; } = DatePrecision.SixDigitPrecisionUtcNow;

  public Credential() {
    Id = Guid.NewGuid();
  }

  public Credential(long? expiresInSeconds) {
    Id = Guid.NewGuid();
    ExpiresInSeconds = expiresInSeconds;
    if (expiresInSeconds != null) {
      ExpirationDate = DatePrecision.SixDigitPrecisionUtcNow.AddSeconds((double)expiresInSeconds);
    }
  }

  public void UpdateCredential(string accessToken, string refreshToken, long? expiresInSeconds) {
    AccessToken = accessToken;
    RefreshToken = refreshToken;
    UpdatedAt = DatePrecision.SixDigitPrecisionUtcNow;
    ExpiresInSeconds = expiresInSeconds;
    if (expiresInSeconds != null) {
      ExpirationDate = DatePrecision.SixDigitPrecisionUtcNow.AddSeconds((double)expiresInSeconds);
    }
  }
}
