using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;

namespace TheChatbot.Services;

public class StatusService(AppDbContext database, DatabaseConfig databaseConfig) {
  private record OpenConnectionsQueryResult(int Count);

  public async Task<Status> GetStatus() {
    var version = await database.Sql<string>($"SHOW server_version;").ToListAsync();
    var maxConnections = await database.Sql<string>($"SHOW max_connections;").ToListAsync();
    var openConnections = await database.Sql<OpenConnectionsQueryResult>($@"
      SELECT count(*) FROM pg_stat_activity
      WHERE datname = {databaseConfig.DatabaseName};
    ").ToListAsync();
    return new Status(version[0], int.Parse(maxConnections[0]), openConnections[0].Count);
  }
}
