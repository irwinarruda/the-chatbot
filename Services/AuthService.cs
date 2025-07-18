using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;

namespace TheChatbot.Services;

public class AuthService(AppDbContext database, EncryptionConfig encryptionConfig, IGoogleAuthGateway googleAuthGateway) {
  public string GetGoogleLoginUrl(string phoneNumber) {
    if (string.IsNullOrEmpty(phoneNumber)) {
      throw new ValidationException("Phone number has no length");
    }
    var encryption = new Encryption(encryptionConfig.Text32Bytes, encryptionConfig.Text16Bytes);
    var state = encryption.Encrypt(phoneNumber);
    return googleAuthGateway.CreateAuthorizationCodeUrl(state);
  }
  public async Task SaveGoogleCredentials(string state, string code) {
    var encryption = new Encryption(encryptionConfig.Text32Bytes, encryptionConfig.Text16Bytes);
    var phoneNumber = encryption.Decrypt(state);
    var userToken = await googleAuthGateway.ExchangeCodeForTokenAsync(code);
    var userinfo = await googleAuthGateway.GetUserinfo(userToken);

    var user = new User(userinfo.Name, phoneNumber);
    var credential = new Credential {
      Type = CredentialType.Google,
      AccessToken = userToken.AccessToken,
      RefreshToken = userToken.RefreshToken,
      ExpiresInSeconds = userToken.ExpiresInSeconds,
    };
    user.AddGoogleCredential(credential);

    await CreateUser(user);
  }
  public async Task<string> GetThankYouPageHtmlString() {
    var template = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), "Templates", "ThankYouPage.html"));
    return template;
  }
  public async Task<User> CreateUser(User user) {
    await database.Execute($@"
      INSERT INTO users (id, name, phone_number, created_at, updated_at)
      VALUES ({user.Id}, {user.Name}, {user.PhoneNumber}, {user.CreatedAt}, {user.UpdatedAt});
    ");
    if (user.GoogleCredential != null) {
      var credential = user.GoogleCredential;
      await database.Execute($@"
        INSERT INTO google_credentials (id, id_user, access_token, refresh_token, expires_in_seconds, created_at, updated_at)
        VALUES ({credential.Id}, {user.Id}, {credential.AccessToken}, {credential.RefreshToken}, {credential.ExpiresInSeconds}, {credential.CreatedAt}, {credential.UpdatedAt});
      ");
    }
    return user;
  }
  public async Task<List<User>> GetUsers() {
    var dbUsers = await database.Query<DbUser>($"SELECT * FROM users;").ToListAsync();
    var users = dbUsers.Select((u) => new User {
      Id = u.Id,
      Name = u.Name,
      PhoneNumber = u.PhoneNumber,
      IsInactive = u.IsInactive,
      CreatedAt = u.CreatedAt,
      UpdatedAt = u.UpdatedAt,
    }).ToList();
    foreach (var user in users) {
      var dbGoogleCredential = await database.Query<DbGoogleCredential>($@"
        SELECT * FROM google_credentials
        WHERE id_user = {user.Id}
      ").ToListAsync();
      if (dbGoogleCredential.Count == 0) continue;
      var credential = new Credential {
        Id = dbGoogleCredential[0].Id,
        IdUser = dbGoogleCredential[0].IdUser,
        AccessToken = dbGoogleCredential[0].AccessToken,
        RefreshToken = dbGoogleCredential[0].RefreshToken,
        ExpiresInSeconds = dbGoogleCredential[0].ExpiresInSeconds,
        CreatedAt = dbGoogleCredential[0].CreatedAt,
        UpdatedAt = dbGoogleCredential[0].UpdatedAt,
        Type = CredentialType.Google,
      };
      user.AddGoogleCredential(credential);
    }
    return users;
  }
  private record DbUser(
    Guid Id,
    string Name,
    string PhoneNumber,
    bool IsInactive,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
  private record DbGoogleCredential(
    Guid Id,
    Guid IdUser,
    string AccessToken,
    string RefreshToken,
    long? ExpiresInSeconds,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
