using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;

namespace TheChatbot.Services;

public class AuthService(AppDbContext database, EncryptionConfig encryptionConfig, IGoogleAuthGateway googleAuthGateway) {
  public string GetGoogleLoginUrl(string phoneNumber) {
    if (phoneNumber.Length == 0) {
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
    await CreateUser(userinfo.Name, phoneNumber);
  }
  public async Task<string> GetThankYouPageHtmlString() {
    var template = await File.ReadAllTextAsync(Path.Join(Directory.GetCurrentDirectory(), "Templates", "ThankYouPage.html"));
    return template;
  }
  public async Task<User> CreateUser(string name, string phoneNumber) {
    var user = new User(name, phoneNumber);
    await database.Execute($@"
      INSERT INTO users (id, name, phone_number, created_at, updated_at)
      VALUES ({user.Id}, {user.Name}, {user.PhoneNumber}, {user.CreatedAt}, {user.UpdatedAt});
    ");
    return user;
  }
  public async Task<List<User>> GetUsers() {
    var dbUsers = await database.Query<DbUser>($@"SELECT * FROM users;").ToListAsync();
    var users = dbUsers.Select((u) => new User {
      Id = u.Id,
      Name = u.Name,
      PhoneNumber = u.PhoneNumber,
      IsInactive = u.IsInactive,
      CreatedAt = u.CreatedAt,
      UpdatedAt = u.UpdatedAt,
    }).ToList();
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
}
