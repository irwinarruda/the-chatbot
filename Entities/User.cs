using System.Text.RegularExpressions;

using TheChatbot.Entities.Extensions;
using TheChatbot.Infra;

namespace TheChatbot.Entities;

public class User {
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public bool IsInactive { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }
  public Credential? GoogleCredential { get; set; }
  public User() {
    Id = Guid.NewGuid();
    IsInactive = false;
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public User(string name, string phoneNumber) {
    if (name.Length >= 30) throw new ValidationException(
      "User name cannot have more than 29 characters",
      "Chose another name and continue"
    );
    phoneNumber = PhoneNumberUtils.Sanitize(phoneNumber);
    if (!PhoneNumberUtils.IsValid(phoneNumber)) throw new ValidationException(
      "User phone number is not valid",
      "Chose another phone number and continue"
    );
    Name = name;
    PhoneNumber = phoneNumber;
    Id = Guid.NewGuid();
    IsInactive = false;
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }

  public void CreateGoogleCredential(string accessToken, string refreshToken, long? expiresInSeconds) {
    var googleCredential = new Credential(expiresInSeconds) {
      IdUser = Id,
      AccessToken = accessToken,
      RefreshToken = refreshToken,
      Type = CredentialType.Google,
    };
    GoogleCredential = googleCredential;
  }

  public void AddGoogleCredential(Credential googleCredential) {
    if (googleCredential.Type != CredentialType.Google) throw new ValidationException("The credential must be from google");
    GoogleCredential = googleCredential;
  }

  public void UpdateGoogleCredential(string accessToken, string refreshToken, long? expiresInSeconds) {
    if (GoogleCredential == null) throw new ValidationException("The user does not have credentials to be updated");
    GoogleCredential.Update(accessToken, refreshToken, expiresInSeconds);
  }
}
