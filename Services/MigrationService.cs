using Microsoft.EntityFrameworkCore;

using TheChatbot.Infra;

namespace TheChatbot.Services;

public class MigrationService(AppDbContext database, AuthConfig authConfig) {
  public async Task<List<string>> ListPendingMigrations() {
    var migrations = await database.Database.GetPendingMigrationsAsync();
    return [.. migrations];
  }

  public async Task RunPendingMigrations(string hashPassword) {
    if (authConfig.HashPassword != hashPassword) {
      throw new UnauthorizedException("Invalid password");
    }
    await database.Database.MigrateAsync();
  }

  public async Task ResetMigrations(string hashPassword) {
    if (authConfig.HashPassword != hashPassword) {
      throw new UnauthorizedException("Invalid password");
    }
    await database.Database.MigrateAsync("0");
  }
}
