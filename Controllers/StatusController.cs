using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using TheChatbot.Infra;

namespace TheChatbot.Controllers;


public class Status {
  public DateTime UpdatedAt { get; set; }
  public required DatabaseStatus Database { get; set; }
}

public class DatabaseStatus {
  public required string ServerVersion { get; set; }
  public required int MaxConnections { get; set; }
  public required int OpenConnections { get; set; }
}

[ApiController]
[Route("[controller]")]
public class StatusController : ControllerBase {
  private readonly AppDbContext context;
  private DatabaseConfig databaseConfig;
  private record OpenConnectionsQuery(int Count);
  public StatusController(AppDbContext _context, DatabaseConfig _databaseConfig) {
    context = _context;
    databaseConfig = _databaseConfig;
  }

  [HttpGet]
  public async Task<ActionResult> GetStatus() {
    FormattableString versionQuery = $"SHOW server_version;";
    FormattableString maxConnectionsQuery = $"SHOW max_connections;";
    FormattableString openConnectionsQuery = $"""
      SELECT count(*) FROM pg_stat_activity
      WHERE datname = {databaseConfig.DatabaseName}
    """;
    var version = await context.Database.SqlQuery<string>(versionQuery).ToListAsync();
    var connections = await context.Database.SqlQuery<string>(maxConnectionsQuery).ToListAsync();
    var openConnections = await context.Database.SqlQuery<OpenConnectionsQuery>(openConnectionsQuery).ToListAsync();
    var status = new Status {
      UpdatedAt = DateTime.UtcNow,
      Database = new DatabaseStatus {
        ServerVersion = version[0],
        MaxConnections = int.Parse(connections[0]),
        OpenConnections = openConnections[0].Count,
      }
    };
    return Ok(status);
  }
}
