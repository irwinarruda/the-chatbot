using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class TestCashFlowSpreadsheetGateway : ICashFlowSpreadsheetGateway {
  static readonly List<Transaction> transactions = [];
  public string ValidSheetId;

  public TestCashFlowSpreadsheetGateway(GoogleSheetsConfig googleSheetsConfig) {
    ValidSheetId = googleSheetsConfig.TestSheetId;
  }

  private static void ValidateAccessToken(string? accessToken) {
    if (accessToken != "ya29.a0ARrdaM9test_access_token_123456789") throw new ValidationException("Invalid access token");
  }

  public Task AddTransaction(AddTransactionDTO transaction) {
    ValidateAccessToken(transaction.SheetAccessToken);
    if (transaction.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    var newTransaction = new Transaction {
      SheetId = transaction.SheetId,
      Date = transaction.Date.Date,
      Value = transaction.Value,
      Category = transaction.Category,
      Description = transaction.Description,
      BankAccount = transaction.BankAccount
    };
    transactions.Add(newTransaction);
    return Task.CompletedTask;
  }

  public async Task AddExpense(AddExpenseDTO expense) {
    ValidateAccessToken(expense.SheetAccessToken);
    expense.Value = Math.Abs(expense.Value) * -1;
    await AddTransaction(expense);
  }

  public async Task AddEarning(AddEarningDTO earning) {
    ValidateAccessToken(earning.SheetAccessToken);
    earning.Value = Math.Abs(earning.Value);
    await AddTransaction(earning);
  }

  public Task DeleteLastTransaction(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    if (transactions.Count == 0) {
      throw new ValidationException(
        "There are no items to be deleted",
        "Verify if deleting is the correct operation"
      );
    }
    transactions.RemoveAt(transactions.Count - 1);
    return Task.CompletedTask;
  }

  public Task<List<Transaction>> GetAllTransactions(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    return Task.FromResult(transactions);
  }

  public Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    if (transactions.Count == 0) return Task.FromResult<Transaction?>(null);
    var lastTransaction = transactions[^1];
    return Task.FromResult<Transaction?>(lastTransaction);
  }

  public Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    return Task.FromResult(new List<string> {
      "Telefone, internet e TV",
      "Delivery"
    });
  }

  public Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    return Task.FromResult(new List<string> {
      "Salário",
      "Outras Receitas"
    });
  }

  public Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig) {
    ValidateAccessToken(sheetConfig.SheetAccessToken);
    if (sheetConfig.SheetId != ValidSheetId) {
      throw new ServiceException(cause: null, "The provided sheet ID is not valid");
    }
    return Task.FromResult(new List<string> {
      "NuConta",
      "Caju"
    });
  }

  public string GetSpreadsheetIdByUrl(string url) {
    if (!url.Contains("docs.google.com/spreadsheets")) ThrowWrongUrlException();
    var split = url.Split("/");
    var id = split[5];
    if (string.IsNullOrEmpty(id)) ThrowWrongUrlException();
    return id;

    static void ThrowWrongUrlException() {
      throw new ValidationException("Invalid url", "Please provide a valid Google Sheets URL");
    }
  }
}
