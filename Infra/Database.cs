using Microsoft.EntityFrameworkCore;

using TheChatbot.Utils;

namespace TheChatbot.Infra;

public class DatabaseConfig {
  public string ConnectionString { get; set; } = string.Empty;
  public string ServerVersion { get; set; } = string.Empty;
}

public class AppDbContext : DbContext {
  private readonly DatabaseConfig databaseConfig;
  public AppDbContext() {
    var configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
  }
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    optionsBuilder.UseNpgsql(databaseConfig.ConnectionString);
  }
}
