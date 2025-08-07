using TheChatbot.Entities.Extensions;

namespace TheChatbot.Entities;

public enum CashFlowSpreadsheetType {
  Google,
}

public class CashFlowSpreadsheet {
  public Guid Id { get; set; }
  public required Guid IdUser { get; set; }
  public required string IdSheet { get; set; } = string.Empty;
  public CashFlowSpreadsheetType Type { get; set; } = CashFlowSpreadsheetType.Google;
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public CashFlowSpreadsheet() {
    Id = Guid.NewGuid();
    CreatedAt = DateTime.UtcNow.TruncateToMicroseconds();
    UpdatedAt = DateTime.UtcNow.TruncateToMicroseconds();
  }
}
