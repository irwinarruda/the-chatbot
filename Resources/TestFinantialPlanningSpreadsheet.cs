namespace TheChatbot.Resources;

public class TestFinantialPlanningSpreadsheet : IFinantialPlanningSpreadsheet {
  static readonly List<Transaction> transactions = [];

  public void FromAccessToken(string accessToken) {
    throw new NotImplementedException();
  }

  public Task AddTransaction(AddTransactionDTO transaction) {
    if (transaction.SheetId != "uniqueId") throw new Exception("The sheet does not exist.");
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
    expense.Value = Math.Abs(expense.Value) * -1;
    await AddTransaction(expense);
  }

  public async Task AddEarning(AddEarningDTO earning) {
    earning.Value = Math.Abs(earning.Value);
    await AddTransaction(earning);
  }

  public Task DeleteLastTransaction(SheetConfigDTO sheetConfig) {
    if (transactions.Count == 0) throw new Exception("There is no transaction to be deleted");
    transactions.RemoveAt(transactions.Count - 1);
    return Task.CompletedTask;
  }

  public string GetSpreadSheetIdByUrl(string url) {
    if (!url.Contains("docs.google.com/spreadsheets")) throw new Exception("Invalid URL");
    var split = url.Split("/");
    var id = split[5];
    if (string.IsNullOrEmpty(id)) throw new Exception("Invalid URL");
    return id;
  }

  public Task<List<Transaction>> GetAllTransactions(SheetConfigDTO sheetConfig) {
    return Task.FromResult(transactions);
  }

  public Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig) {
    if (transactions.Count == 0) return Task.FromResult<Transaction?>(null);
    var lastTransaction = transactions[^1];
    return Task.FromResult<Transaction?>(lastTransaction);
  }

  public Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "Telefone, internet e TV",
      "Delivery"
    });
  }

  public Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "Salário",
      "Outras Receitas"
    });
  }

  public Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "NuConta",
      "Caju"
    });
  }
}
