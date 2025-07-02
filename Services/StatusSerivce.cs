using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;

namespace TheChatbot.Services;

public class StatusService {
  private readonly AppDbContext context;
  private readonly DatabaseConfig databaseConfig;
  private record OpenConnectionsQueryResult(int Count);
  public StatusService(AppDbContext _context, DatabaseConfig _databaseConfig) {
    context = _context;
    databaseConfig = _databaseConfig;
  }

  public async Task<Status> GetStatus() {
    var version = await context.Sql<string>($"SHOW server_version;").ToListAsync();
    var maxConnections = await context.Sql<string>($"SHOW max_connections;").ToListAsync();
    var openConnections = await context.Sql<OpenConnectionsQueryResult>($"""
      SELECT count(*) FROM pg_stat_activity
      WHERE datname = {databaseConfig.DatabaseName};
    """).ToListAsync();
    return new Status(version[0], int.Parse(maxConnections[0]), openConnections[0].Count);
  }
}
