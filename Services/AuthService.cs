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

  public async Task SaveUserByGoogleCredential(string state, string code) {
    var encryption = new Encryption(encryptionConfig.Text32Bytes, encryptionConfig.Text16Bytes);
    var phoneNumber = encryption.Decrypt(state);
    var userToken = await googleAuthGateway.ExchangeCodeForTokenAsync(code);
    var userinfo = await googleAuthGateway.GetUserinfo(userToken);
    var user = await GetUserByPhoneNumber(phoneNumber);
    if (user == null) {
      user = new User(userinfo.Name, phoneNumber);
      user.CreateGoogleCredential(
        accessToken: userToken.AccessToken,
        refreshToken: userToken.RefreshToken,
        expiresInSeconds: userToken.ExpiresInSeconds
      );
      await CreateUser(user);
      return;
    }
    if (user.GoogleCredential == null) {
      throw new ValidationException("Something went wrong with your request");
    }
    user.UpdateGoogleCredential(
      accessToken: userToken.AccessToken,
      refreshToken: userToken.RefreshToken,
      expiresInSeconds: userToken.ExpiresInSeconds
    );
    await SaveGoogleCredential(user.GoogleCredential);
  }

  public async Task RefreshGoogleCredential(Guid userId) {
    var user = await GetUserById(userId);
    if (user == null || user.GoogleCredential == null) {
      throw new ValidationException("Something went wrong refreshing user credentials.");
    }
    var userToken = await googleAuthGateway.RefreshToken(user.GoogleCredential.AccessToken, user.GoogleCredential.RefreshToken);
    user.GoogleCredential.Update(
      accessToken: userToken.AccessToken,
      refreshToken: userToken.RefreshToken,
      expiresInSeconds: userToken.ExpiresInSeconds
    );
    await SaveGoogleCredential(user.GoogleCredential);
  }
  public async Task<string> GetThankYouPageHtmlString() {
    var template = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), "Templates", "ThankYouPage.html"));
    return template;
  }
  public async Task<string> GetAlreadySignedInPageHtmlString() {
    var template = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), "Templates", "AlreadySignedIn.html"));
    return template;
  }

  public async Task<(bool IsRedirect, string Content)> HandleGoogleLogin(string phoneNumber) {
    var user = await GetUserByPhoneNumber(phoneNumber);
    if (user?.GoogleCredential != null) {
      await RefreshGoogleCredential(user.Id);
      var template = await GetAlreadySignedInPageHtmlString();
      return (IsRedirect: false, Content: template);
    }
    var url = GetGoogleLoginUrl(phoneNumber);
    return (IsRedirect: true, Content: url);
  }

  public async Task<string> HandleGoogleRedirect(string state, string code) {
    await SaveUserByGoogleCredential(state, code);
    var template = await GetThankYouPageHtmlString();
    return template;
  }

  public async Task<User> CreateUser(User user) {
    await database.Execute($@"
      INSERT INTO users (id, name, phone_number, created_at, updated_at)
      VALUES ({user.Id}, {user.Name}, {user.PhoneNumber}, {user.CreatedAt}, {user.UpdatedAt})
    ");
    if (user.GoogleCredential != null) {
      var credential = user.GoogleCredential;
      await database.Execute($@"
        INSERT INTO google_credentials (id, id_user, access_token, refresh_token, expires_in_seconds, expiration_date, created_at, updated_at)
        VALUES ({credential.Id}, {user.Id}, {credential.AccessToken}, {credential.RefreshToken}, {credential.ExpiresInSeconds}, {credential.ExpirationDate}, {credential.CreatedAt}, {credential.UpdatedAt})
      ");
    }
    return user;
  }

  public async Task<User?> GetUserByPhoneNumber(string phoneNumber) {
    var dbUsers = await database.Query<DbUser>($@"
      SELECT * FROM users
      WHERE phone_number = {phoneNumber}
    ").FirstOrDefaultAsync();
    if (dbUsers == null) return null;
    var user = new User {
      Id = dbUsers.Id,
      Name = dbUsers.Name,
      PhoneNumber = dbUsers.PhoneNumber,
      IsInactive = dbUsers.IsInactive,
      CreatedAt = dbUsers.CreatedAt,
      UpdatedAt = dbUsers.UpdatedAt,
    };
    var dbGoogleCredential = await database.Query<DbGoogleCredential>($@"
      SELECT * FROM google_credentials
      WHERE id_user = {user.Id}
    ").FirstOrDefaultAsync();
    if (dbGoogleCredential != null) {
      var credential = new Credential {
        Id = dbGoogleCredential.Id,
        IdUser = dbGoogleCredential.IdUser,
        AccessToken = dbGoogleCredential.AccessToken,
        RefreshToken = dbGoogleCredential.RefreshToken,
        ExpiresInSeconds = dbGoogleCredential.ExpiresInSeconds,
        ExpirationDate = dbGoogleCredential.ExpirationDate,
        CreatedAt = dbGoogleCredential.CreatedAt,
        UpdatedAt = dbGoogleCredential.UpdatedAt,
        Type = CredentialType.Google,
      };
      user.GoogleCredential = credential;
    }
    return user;
  }

  public async Task<User?> GetUserById(Guid id) {
    var dbUsers = await database.Query<DbUser>($@"
      SELECT * FROM users
      WHERE id = {id}
    ").FirstOrDefaultAsync();
    if (dbUsers == null) return null;

    var user = new User {
      Id = dbUsers.Id,
      Name = dbUsers.Name,
      PhoneNumber = dbUsers.PhoneNumber,
      IsInactive = dbUsers.IsInactive,
      CreatedAt = dbUsers.CreatedAt,
      UpdatedAt = dbUsers.UpdatedAt,
    };
    var dbGoogleCredential = await database.Query<DbGoogleCredential>($@"
      SELECT * FROM google_credentials
      WHERE id_user = {user.Id}
    ").FirstOrDefaultAsync();
    if (dbGoogleCredential != null) {
      var credential = new Credential {
        Id = dbGoogleCredential.Id,
        IdUser = dbGoogleCredential.IdUser,
        AccessToken = dbGoogleCredential.AccessToken,
        RefreshToken = dbGoogleCredential.RefreshToken,
        ExpiresInSeconds = dbGoogleCredential.ExpiresInSeconds,
        ExpirationDate = dbGoogleCredential.ExpirationDate,
        CreatedAt = dbGoogleCredential.CreatedAt,
        UpdatedAt = dbGoogleCredential.UpdatedAt,
        Type = CredentialType.Google,
      };
      user.GoogleCredential = credential;
    }
    return user;
  }

  public async Task SaveGoogleCredential(Credential googleCredential) {
    await database.Execute($@"
      UPDATE google_credentials
      SET
        access_token = {googleCredential.AccessToken},
        refresh_token = {googleCredential.RefreshToken},
        expires_in_seconds = {googleCredential.ExpiresInSeconds},
        expiration_date = {googleCredential.ExpirationDate},
        updated_at = {googleCredential.UpdatedAt}
      WHERE id = {googleCredential.Id}
    ");
  }

  public async Task<List<User>> GetUsers() {
    var dbUsers = await database.Query<DbUser>($"SELECT * FROM users").ToListAsync();
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
      ").FirstOrDefaultAsync();
      if (dbGoogleCredential == null) continue;
      var credential = new Credential {
        Id = dbGoogleCredential.Id,
        IdUser = dbGoogleCredential.IdUser,
        AccessToken = dbGoogleCredential.AccessToken,
        RefreshToken = dbGoogleCredential.RefreshToken,
        ExpiresInSeconds = dbGoogleCredential.ExpiresInSeconds,
        ExpirationDate = dbGoogleCredential.ExpirationDate,
        CreatedAt = dbGoogleCredential.CreatedAt,
        UpdatedAt = dbGoogleCredential.UpdatedAt,
        Type = CredentialType.Google,
      };
      user.GoogleCredential = credential;
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
    DateTime? ExpirationDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}
