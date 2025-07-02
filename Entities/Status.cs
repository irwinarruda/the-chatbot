namespace TheChatbot.Entities;

public class Status {
  public DateTime UpdatedAt { get; set; }
  public DatabaseStatus Database { get; set; }
  public Status(string version, int maxConnections, int openConnections) {
    UpdatedAt = DateTime.UtcNow;
    Database = new DatabaseStatus {
      ServerVersion = version,
      MaxConnections = maxConnections,
      OpenConnections = openConnections
    };
  }
}

public class DatabaseStatus {
  public required string ServerVersion { get; set; }
  public required int MaxConnections { get; set; }
  public required int OpenConnections { get; set; }
}
