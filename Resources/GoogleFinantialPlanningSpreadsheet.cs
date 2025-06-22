using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace TheChatbot.Resources;

public class GoogleFinantialPlanningSpreadsheet : IFinantialPlanningSpreadsheet {
  public Task AddEarning(AddEarningDTO earning) {
    throw new NotImplementedException();
  }

  public async Task AddExpense(AddExpenseDTO expense) {
    var credential = GoogleCredential.FromAccessToken(expense.SheetAccessToken);
    var sheetsService = new SheetsService(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot",
    });
    var query = "Diário!A:G";
    var sheet = await sheetsService.Spreadsheets.Values.Get(expense.SheetId, query).ExecuteAsync();
    var nextLine = sheet.Values.Count + 1;
    var batch = new BatchUpdateValuesRequest {
      Data = [
        new ValueRange { Values = [[expense.Date.ToString("dd/MM/yyyy")]], Range = $"Diário!B{nextLine}" },
        new ValueRange { Values = [[(expense.Value * -1).ToString().Replace(".", ","), expense.Category, expense.Description, expense.BankAccount]], Range = $"Diário!D{nextLine}:G{nextLine}"  },
      ],
      ValueInputOption = "USER_ENTERED",
    };
    await sheetsService.Spreadsheets.Values.BatchUpdate(batch, expense.SheetId).ExecuteAsync();
  }

  public async Task<List<string>> GetExpenseCategories(SheetConfigDTO sheetConfig) {
    var credential = GoogleCredential.FromAccessToken(sheetConfig.SheetAccessToken);
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
    return [.. result.ValueRanges
      .SelectMany(range => range.Values ?? [])
      .SelectMany(item => item)
      .Select(item => item?.ToString() ?? string.Empty)
      .Where(s => !string.IsNullOrEmpty(s))
    ];
  }

  public async Task<List<string>> GetEarningCategories(SheetConfigDTO sheetConfig) {
    var credential = GoogleCredential.FromAccessToken(sheetConfig.SheetAccessToken);
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
    return [.. result.Values
      .SelectMany(item => item)
      .Select(item => item?.ToString() ?? string.Empty)
      .Where(s => !string.IsNullOrEmpty(s))
    ];
  }
}
