using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using TheChatbot.Dtos;

namespace TheChatbot.Resources;

public class TestGoogleFinantialPlanningSpreadsheet : IFinantialPlanningSpreadsheet {
  readonly GoogleCredential credential;

  public TestGoogleFinantialPlanningSpreadsheet(IConfiguration _configuration) {
    var googleConfig = _configuration.GetSection("GoogleOAuthConfig").Get<GoogleOAuthConfig>()!;
    var initializer = new ServiceAccountCredential.Initializer(googleConfig.ServiceAccountId) {
      Scopes = [SheetsService.Scope.Spreadsheets],
    };
    initializer.FromPrivateKey(googleConfig.ServiceAccountPrivateKey);
    credential = GoogleCredential.FromServiceAccountCredential(new ServiceAccountCredential(initializer));
  }

  public async Task AddTransaction(AddTransactionDTO transaction) {
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var query = "Diário!A:G";
    var sheet = await sheetsService.Spreadsheets.Values.Get(transaction.SheetId, query).ExecuteAsync();
    var nextLine = sheet.Values.Count + 1;
    var transactionDate = transaction.Date.ToString("dd/MM/yyyy");
    var transactionValue = transaction.Value.ToString().Replace(".", ",");
    var batch = new BatchUpdateValuesRequest {
      Data = [
        new ValueRange { Values = [[transactionDate]], Range = $"Diário!B{nextLine}" },
        new ValueRange { Values = [[transactionValue, transaction.Category, transaction.Description, transaction.BankAccount]], Range = $"Diário!D{nextLine}:G{nextLine}"  },
      ],
      ValueInputOption = "USER_ENTERED",
    };
    await sheetsService.Spreadsheets.Values.BatchUpdate(batch, transaction.SheetId).ExecuteAsync();
  }

  public async Task AddEarning(AddEarningDTO earning) {
    earning.Value = Math.Abs(earning.Value);
    await AddTransaction(earning);
  }

  public async Task AddExpense(AddExpenseDTO expense) {
    expense.Value = Math.Abs(expense.Value) * -1;
    await AddTransaction(expense);
  }

  public string GetSpreadSheetIdByUrl(string url) {
    var split = url.Split("/");
    var id = split[5] ?? throw new Exception("Invalid URL");
    return id;
  }

  public async Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig) {
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var query = "Diário!B:G";
    var sheet = await sheetsService.Spreadsheets.Values.Get(sheetConfig.SheetId, query).ExecuteAsync();
    if (sheet == null || sheet.Values == null) {
      throw new Exception("There is something wrong with your SpreadSheet");
    }
    if (sheet.Values.Count == 0) {
      return null;
    }
    var lastRow = sheet.Values[^1];
    return new Transaction {
      SheetId = sheetConfig.SheetId,
      Date = DateTime.ParseExact(lastRow[0]?.ToString() ?? "", "dd/MM/yyyy", null),
      Value = double.Parse(lastRow[2]?.ToString()?.Replace(",", ".") ?? ""),
      Category = lastRow[3]?.ToString() ?? "",
      Description = lastRow[4]?.ToString() ?? "",
      BankAccount = lastRow[5]?.ToString() ?? ""
    };
  }

  public async Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var sheet = sheetsService.Spreadsheets.Values.BatchGet(sheetConfig.SheetId);
    sheet.Ranges = new List<string> {
      "DADOS Gerais + Plano de Contas!D9:D12",
      "DADOS Gerais + Plano de Contas!D15:D26",
      "DADOS Gerais + Plano de Contas!D29:D35",
      "DADOS Gerais + Plano de Contas!D38:D44",
      "DADOS Gerais + Plano de Contas!D47:D58",
      "DADOS Gerais + Plano de Contas!D61:D65",
      "DADOS Gerais + Plano de Contas!D68:D80",
    };
    var result = await sheet.ExecuteAsync();
    if (result == null || result.ValueRanges == null) {
      throw new Exception("There is something wrong with your SpreadSheet");
    }
    return [.. result.ValueRanges
      .SelectMany(range => range.Values ?? [])
      .SelectMany(item => item)
      .Select(item => item?.ToString() ?? string.Empty)
      .Where(s => !string.IsNullOrEmpty(s))
    ];
  }

  public async Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var sheet = sheetsService.Spreadsheets.Values.BatchGet(sheetConfig.SheetId);
    sheet.Ranges = new List<string> {
      "DADOS Gerais + Plano de Contas!B9:B14",
      "DADOS Gerais + Plano de Contas!B17:B19",
      "DADOS Gerais + Plano de Contas!B22:B23",
    };
    var result = await sheet.ExecuteAsync();
    if (result == null || result.ValueRanges == null) {
      throw new Exception("There is something wrong with your SpreadSheet");
    }
    return [.. result.ValueRanges
      .SelectMany(range => range.Values ?? [])
      .SelectMany(item => item)
      .Select(item => item?.ToString() ?? string.Empty)
      .Where(s => !string.IsNullOrEmpty(s))
    ];
  }

  public async Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig) {
    var credential = GoogleCredential.FromAccessToken(sheetConfig.SheetAccessToken);
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var result = await sheetsService.Spreadsheets.Values.Get(sheetConfig.SheetId, "DADOS Gerais + Plano de Contas!F9:F24").ExecuteAsync();
    if (result == null || result.Values == null) {
      throw new Exception("There is something wrong with your SpreadSheet");
    }
    return [.. result.Values
      .SelectMany(item => item)
      .Select(item => item?.ToString() ?? string.Empty)
      .Where(s => !string.IsNullOrEmpty(s))
    ];
  }
}
