namespace TheChatbot.Entities;

public class Status {
  public DateTime UpdatedAt { get; set; }
  public DatabaseStatus Database { get; set; }
  public McpStatus Mcp { get; set; }
  public Status(string version, int maxConnections, int openConnections, string modelName) {
    UpdatedAt = DateTime.UtcNow;
    Database = new() {
      ServerVersion = version,
      MaxConnections = maxConnections,
      OpenConnections = openConnections,
    };
    Mcp = new() {
      ModelName = modelName,
    };
  }
}

public class DatabaseStatus {
  public required string ServerVersion { get; set; }
  public required int MaxConnections { get; set; }
  public required int OpenConnections { get; set; }
}

public class McpStatus {
  public required string ModelName { get; set; }
}
