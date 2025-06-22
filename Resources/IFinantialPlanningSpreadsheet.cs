namespace TheChatbot.Resources;

public class SheetConfigDTO {
  public required string SheetId { get; set; }
  public required string SheetAccessToken { get; set; }
}

public class AddExpenseDTO : SheetConfigDTO {
  public DateTime Date { get; set; }
  public double Value { get; set; }
  public required string Category { get; set; }
  public required string Description { get; set; }
  public required string BankAccount { get; set; }
}

public class AddEarningDTO : SheetConfigDTO {

  public DateTime Date { get; set; }
  public double Value { get; set; }
  public required string Category { get; set; }
  public required string Description { get; set; }
  public required string BankAccount { get; set; }
}


public interface IFinantialPlanningSpreadsheet {
  Task AddExpense(AddExpenseDTO expense);
  Task AddEarning(AddEarningDTO earning);
  Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig);
  Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig);
  Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig);
}
