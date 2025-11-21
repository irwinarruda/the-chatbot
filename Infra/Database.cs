using Microsoft.EntityFrameworkCore;

using TheChatbot.Utils;

namespace TheChatbot.Infra;

public class DatabaseConfig {
  public string ConnectionString { get; set; } = string.Empty;
  public string DatabaseName { get; set; } = string.Empty;
  public string ServerVersion { get; set; } = string.Empty;
}

public class AppDbContext : DbContext {
  private readonly DatabaseConfig databaseConfig;
  public AppDbContext() {
    var configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
  }
  public IQueryable<TResult> Query<TResult>(FormattableString sql) => Database.SqlQuery<TResult>(sql);
  public Task<int> Execute(FormattableString sql) => Database.ExecuteSqlAsync(sql);

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
    optionsBuilder.UseNpgsql(databaseConfig.ConnectionString).UseSnakeCaseNamingConvention();
  }
}
