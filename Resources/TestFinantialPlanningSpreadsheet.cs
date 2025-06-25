
namespace TheChatbot.Resources;

public class TestFinantialPlanningSpreadsheet : IFinantialPlanningSpreadsheet {
  static readonly List<Transaction> transactions = [
    new Transaction {
      SheetId = "uniqueId",
      Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
      Value = 3000,
      Category = "Salário",
      Description = "Salário de Janeiro",
      BankAccount = "NuConta"
    },
    new Transaction {
      SheetId = "uniqueId",
      Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
      Value = 600,
      Category = "Salário",
      Description = "Vale alimentação",
      BankAccount = "Caju"
    },
    new Transaction {
      SheetId = "uniqueId",
      Date = DateTime.ParseExact("2025-01-04", "yyyy-MM-dd", null),
      Value = -120,
      Category = "Telefone, internet e TV",
      Description = "Conta de internet TIM",
      BankAccount = "NuConta"
    },
    new Transaction {
      SheetId = "uniqueId",
      Date = DateTime.ParseExact("2025-01-04", "yyyy-MM-dd", null),
      Value = -120.20,
      Category = "Telefone, internet e TV",
      Description = "Conta do celular TIM",
      BankAccount = "NuConta"
    },
  ];

  public async Task AddEarning(AddEarningDTO earning) {
    earning.Value = Math.Abs(earning.Value);
    await AddTransaction(earning);
  }

  public async Task AddExpense(AddExpenseDTO expense) {
    expense.Value = Math.Abs(expense.Value) * -1;
    await AddTransaction(expense);
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

  public Task DeleteLastTransaction(SheetConfigDTO sheetConfig) {
    transactions.RemoveAt(transactions.Count - 1);
    return Task.CompletedTask;
  }

  public void FromAccessToken(string accessToken) {
    throw new NotImplementedException();
  }

  public Task<List<Transaction>> GetAllTransactions(SheetConfigDTO sheetConfig) {
    return Task.FromResult(transactions);
  }

  public Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "NuConta",
      "Caju"
    });
  }

  public Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "Salário",
      "Outras Receitas"
    });
  }

  public Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    return Task.FromResult(new List<string> {
      "Telefone, internet e TV",
      "Delivery"
    });
  }

  public Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig) {
    var lastTransaction = transactions[^1];
    return Task.FromResult<Transaction?>(lastTransaction);
  }

  public string GetSpreadSheetIdByUrl(string url) {
    if (!url.Contains("docs.google.com/spreadsheets")) throw new Exception("Invalid URL");
    var split = url.Split("/");
    var id = split[5] ?? throw new Exception("Invalid URL");
    return id;
  }
}
