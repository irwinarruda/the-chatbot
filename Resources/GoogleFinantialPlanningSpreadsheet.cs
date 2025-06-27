using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class GoogleFinantialPlanningSpreadsheet : IFinantialPlanningSpreadsheet {
  private readonly GoogleConfig googleConfig;
  private GoogleCredential credential;
  private SheetsService sheetsService;

  public GoogleFinantialPlanningSpreadsheet(IConfiguration _configuration) {
    googleConfig = _configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    var initializer = new ServiceAccountCredential.Initializer(googleConfig.ServiceAccountId) {
      Scopes = [SheetsService.Scope.Spreadsheets],
    };
    initializer.FromPrivateKey(googleConfig.ServiceAccountPrivateKey);
    credential = GoogleCredential.FromServiceAccountCredential(new ServiceAccountCredential(initializer));
    sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
  }

  public void FromAccessToken(string accessToken) {
    credential = GoogleCredential.FromAccessToken(accessToken);
    sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
  }

  public async Task AddTransaction(AddTransactionDTO transaction) {
    try {
      var query = "Diário!A:G";
      var sheet = await sheetsService.Spreadsheets.Values.Get(transaction.SheetId, query).ExecuteAsync();
      ThrowWrongSpreadsheetException(sheet);
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
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  public async Task AddExpense(AddExpenseDTO expense) {
    expense.Value = Math.Abs(expense.Value) * -1;
    await AddTransaction(expense);
  }

  public async Task AddEarning(AddEarningDTO earning) {
    earning.Value = Math.Abs(earning.Value);
    await AddTransaction(earning);
  }

  public async Task DeleteLastTransaction(SheetConfigDTO sheetConfig) {
    try {
      var query = "Diário!A:G";
      var sheet = await sheetsService.Spreadsheets.Values.Get(sheetConfig.SheetId, query).ExecuteAsync();
      ThrowWrongSpreadsheetException(sheet);
      var lastItemLine = sheet.Values.Count;
      if (lastItemLine <= 2) {
        throw new ValidationException(
          "There are no items to be deleted",
          "Verify if deleting is the correct operation"
        );
      }
      var batch = new BatchUpdateValuesRequest {
        Data = [
          new ValueRange { Values = [[""]], Range = $"Diário!B{lastItemLine}" },
          new ValueRange { Values = [["", "", "", ""]], Range = $"Diário!D{lastItemLine}:G{lastItemLine}"  },
        ],
        ValueInputOption = "USER_ENTERED",
      };
      await sheetsService.Spreadsheets.Values.BatchUpdate(batch, sheetConfig.SheetId).ExecuteAsync();
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  public string GetSpreadSheetIdByUrl(string url) {
    try {
      if (!url.Contains("docs.google.com/spreadsheets")) ThrowWrongUrlException();
      var split = url.Split("/");
      var id = split[5];
      if (string.IsNullOrEmpty(id)) ThrowWrongUrlException();
      return id;
    } catch (Exception ex) {
      throw HandleError(ex);
    }
    static void ThrowWrongUrlException() {
      throw new ValidationException("Invalid url", "Please provide a valid Google Sheets URL");
    }
  }

  public async Task<List<Transaction>> GetAllTransactions(SheetConfigDTO sheetConfig) {
    try {
      var query = "Diário!B:G";
      var sheet = await sheetsService.Spreadsheets.Values.Get(sheetConfig.SheetId, query).ExecuteAsync();
      ThrowWrongSpreadsheetException(sheet);
      if (sheet.Values.Count <= 2) return [];
      var items = sheet.Values.Skip(2);
      return [..items.Where(row => row != null && row.Count >= 5).Select((row) =>
        new Transaction {
          SheetId = sheetConfig.SheetId,
          Date = DateTime.ParseExact(row[0]?.ToString() ?? "", "dd/MM/yyyy", null),
          Value = double.Parse(row[2]?.ToString()?.Replace("R$ ", "")?.Replace(".", "")?.Replace(",", ".") ?? ""),
          Category = row[3]?.ToString() ?? "",
          Description = row[4]?.ToString() ?? "",
          BankAccount = row[5]?.ToString() ?? ""
        }
      )];
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  public async Task<Transaction?> GetLastTransaction(SheetConfigDTO sheetConfig) {
    var transactions = await GetAllTransactions(sheetConfig);
    if (transactions.Count == 0) return null;
    var lastTransaction = transactions[^1];
    return lastTransaction;
  }

  public async Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    try {
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
      ThrowWrongSpreadsheetException(result);
      return [.. result.ValueRanges
        .SelectMany(range => range.Values ?? [])
        .SelectMany(item => item)
        .Select(item => item?.ToString() ?? string.Empty)
        .Where(s => !string.IsNullOrEmpty(s))
      ];
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  public async Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    try {
      var sheet = sheetsService.Spreadsheets.Values.BatchGet(sheetConfig.SheetId);
      sheet.Ranges = new List<string> {
      "DADOS Gerais + Plano de Contas!B9:B14",
      "DADOS Gerais + Plano de Contas!B17:B19",
      "DADOS Gerais + Plano de Contas!B22:B23",
    };
      var result = await sheet.ExecuteAsync();
      ThrowWrongSpreadsheetException(result);
      return [.. result.ValueRanges
        .SelectMany(range => range.Values ?? [])
        .SelectMany(item => item)
        .Select(item => item?.ToString() ?? string.Empty)
        .Where(s => !string.IsNullOrEmpty(s))
      ];
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  public async Task<List<string>> GetBankAccount(SheetConfigDTO sheetConfig) {
    try {
      var sheet = await sheetsService.Spreadsheets.Values.Get(sheetConfig.SheetId, "DADOS Gerais + Plano de Contas!F9:F24").ExecuteAsync();
      ThrowWrongSpreadsheetException(sheet);
      return [.. sheet.Values
        .SelectMany(item => item)
        .Select(item => item?.ToString() ?? string.Empty)
        .Where(s => !string.IsNullOrEmpty(s))
      ];
    } catch (Exception ex) {
      throw HandleError(ex);
    }
  }

  private static void ThrowWrongSpreadsheetException<T>(T? sheet) where T : class {
    var exception = new ValidationException(
      "There is something wrong with your spreadsheet",
      "Either you have the wrong spreadsheet or it's breaking the default patterns"
    );
    if (sheet is ValueRange valueRange && (valueRange == null || valueRange.Values == null)) {
      throw exception;
    }
    if (sheet is BatchGetValuesResponse valuesResponse && (valuesResponse == null || valuesResponse.ValueRanges == null)) {
      throw exception;
    }
  }

  private static Exception HandleError(Exception ex) {
    if (ex is GoogleApiException) {
      return new ServiceException(ex, "Spreadsheet service is not working at the moment.");
    }
    return ex;
  }
}
