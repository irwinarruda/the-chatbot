namespace TheChatbot.Resources;

public class Transaction {
  public required string SheetId { get; set; }
  public DateTime Date { get; set; }
  public double Value { get; set; }
  public required string Category { get; set; }
  public required string Description { get; set; }
  public required string BankAccount { get; set; }
}

public class AddTransactionDTO : Transaction {
  public required string SheetAccessToken { get; set; }
}

public class AddExpenseDTO : AddTransactionDTO { }

public class AddEarningDTO : AddTransactionDTO { }

public class SheetConfigDTO {
  public required string SheetId { get; set; }
  public required string SheetAccessToken { get; set; }
}


public interface ICashFlowSpreadsheetGateway {
  Task AddTransaction(AddTransactionDTO transaction);
  Task AddExpense(AddExpenseDTO expense);
  Task AddEarning(AddEarningDTO earning);
  Task DeleteLastTransaction(SheetConfigDTO sheetConfig);
  string GetSpreadsheetIdByUrl(string url);
  Task<List<Transaction>> GetAllTransactions(SheetConfigDTO sheetConfig);
  Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig);
  Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig);
  Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig);
  Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig);
}
