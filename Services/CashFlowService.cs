using Microsoft.EntityFrameworkCore;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;

namespace TheChatbot.Services;

public class CashFlowService(AppDbContext database, AuthService authService, ICashFlowSpreadsheetGateway spreadsheetResource) {
  public async Task AddSpreadsheetUrl(string phoneNumber, string url) {
    var user = await authService.GetUserByPhoneNumber(phoneNumber) ?? throw new NotFoundException("User not found");
    var existing = await GetSpreadsheetByUserId(user.Id);
    if (existing != null) {
      throw new ValidationException("User already has a financial planning spreadsheet configured");
    }
    var sheetId = spreadsheetResource.GetSpreadsheetIdByUrl(url);
    var sheet = new CashFlowSpreadsheet {
      IdUser = user.Id,
      IdSheet = sheetId,
      Type = CashFlowSpreadsheetType.Google,
    };
    await CreateCashFlowSpreadsheet(sheet);
  }

  public async Task<List<Transaction>> GetAllTransactions(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    return await spreadsheetResource.GetAllTransactions(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task<Transaction?> GetLastTransaction(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    return await spreadsheetResource.GetLastTransaction(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task DeleteLastTransaction(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    await spreadsheetResource.DeleteLastTransaction(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task AddExpense(CashFlowAddExpenseDTO expense) {
    var (user, sheet) = await GetUserAndSheet(expense.PhoneNumber);
    var dto = new AddExpenseDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
      Date = expense.Date,
      Value = expense.Value,
      Category = expense.Category,
      Description = expense.Description,
      BankAccount = expense.BankAccount,
    };
    await spreadsheetResource.AddExpense(dto);
  }

  public async Task AddEarning(CashFlowAddEarningDTO earning) {
    var (user, sheet) = await GetUserAndSheet(earning.PhoneNumber);
    var dto = new AddEarningDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
      Date = earning.Date,
      Value = earning.Value,
      Category = earning.Category,
      Description = earning.Description,
      BankAccount = earning.BankAccount,
    };
    await spreadsheetResource.AddEarning(dto);
  }

  public async Task<List<string>> GetExpenseCategories(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    return await spreadsheetResource.GetExpenseCategories(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task<List<string>> GetEarningCategories(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    return await spreadsheetResource.GetEarningCategories(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task<List<string>> GetBankAccount(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    return await spreadsheetResource.GetBankAccount(new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    });
  }

  public async Task<(List<string> expenseCategories, List<string> earningCategories, List<string> bankAccounts)> GetCategoriesAndBankAccounts(string phoneNumber) {
    var (user, sheet) = await GetUserAndSheet(phoneNumber);
    var cfg = new SheetConfigDTO {
      SheetId = sheet.IdSheet,
      SheetAccessToken = user.GoogleCredential!.AccessToken,
    };
    var expenseTask = spreadsheetResource.GetExpenseCategories(cfg);
    var earningTask = spreadsheetResource.GetEarningCategories(cfg);
    var bankTask = spreadsheetResource.GetBankAccount(cfg);
    await Task.WhenAll(expenseTask, earningTask, bankTask);
    return (expenseTask.Result, earningTask.Result, bankTask.Result);
  }

  private async Task<(User user, CashFlowSpreadsheet sheet)> GetUserAndSheet(string phoneNumber) {
    var user = await authService.GetUserByPhoneNumber(phoneNumber) ?? throw new NotFoundException("User was not found");
    await EnsureSpreadsheetAccess(user);
    var sheet = await GetSpreadsheetByUserId(user.Id) ?? throw new ValidationException(
      "User does not have a financial planning spreadsheet configured",
      "Add a spreadsheet for this user first"
    );
    return (user, sheet);
  }

  private async Task EnsureSpreadsheetAccess(User user) {
    if (user.GoogleCredential == null) {
      throw new ValidationException("User is not connected to Google");
    }
    if (user.GoogleCredential.ExpirationDate <= DateTime.UtcNow) await authService.RefreshGoogleCredential(user);
  }

  private async Task CreateCashFlowSpreadsheet(CashFlowSpreadsheet sheet) {
    await database.Execute($@"
      INSERT INTO cash_flow_spreadsheets (id, id_user, id_sheet, type, created_at, updated_at)
      VALUES ({sheet.Id}, {sheet.IdUser}, {sheet.IdSheet}, {sheet.Type.ToString()}, {sheet.CreatedAt}, {sheet.UpdatedAt})
    ");
  }

  private async Task<CashFlowSpreadsheet?> GetSpreadsheetByUserId(Guid userId) {
    var dbEntity = await database.Query<DbCashFlowSpreadsheet>($@"
      SELECT * FROM cash_flow_spreadsheets
      WHERE id_user = {userId}
    ").FirstOrDefaultAsync();
    if (dbEntity == null) return null;
    return new CashFlowSpreadsheet {
      Id = dbEntity.Id,
      IdUser = dbEntity.IdUser,
      IdSheet = dbEntity.IdSheet,
      Type = Enum.Parse<CashFlowSpreadsheetType>(dbEntity.Type),
      CreatedAt = dbEntity.CreatedAt,
      UpdatedAt = dbEntity.UpdatedAt,
    };
  }

  private record DbCashFlowSpreadsheet(
    Guid Id,
    Guid IdUser,
    string IdSheet,
    string Type,
    DateTime CreatedAt,
    DateTime UpdatedAt
  );
}

public class CashFlowAddExpenseDTO {
  public required string PhoneNumber { get; set; }
  public DateTime Date { get; set; }
  public double Value { get; set; }
  public required string Category { get; set; }
  public required string Description { get; set; }
  public required string BankAccount { get; set; }
}
public class CashFlowAddEarningDTO {
  public required string PhoneNumber { get; set; }
  public DateTime Date { get; set; }
  public double Value { get; set; }
  public required string Category { get; set; }
  public required string Description { get; set; }
  public required string BankAccount { get; set; }
}
