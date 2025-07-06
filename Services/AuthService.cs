using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;

namespace TheChatbot.Services;

public class AuthService(AppDbContext database, IGoogleAuthGateway googleAuthGateway) {
  public string GetGoogleLoginUrl() {
    return googleAuthGateway.CreateAuthorizationCodeUrl();
  }
  public async Task<object?> SaveGoogleCredentials(string userId) {
    return await Task.FromResult("");
  }
  public async Task<string> GetThankYouPageHtmlString() {
    return await Task.FromResult("");
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
}
public record DbUser(
  Guid Id,
  string Name,
  string PhoneNumber,
  bool IsInactive,
  DateTime CreatedAt,
  DateTime UpdatedAt
);
