using System.ComponentModel;

using ModelContextProtocol.Server;

using TheChatbot.Infra;
using TheChatbot.Services;
using TheChatbot.Resources;
using TheChatbot.Utils;

namespace Mcp.Tools;

[McpServerToolType]
class CashFlowTool(CashFlowService cashFlowService, IAiChatGateway aiChatGateway) {
  [McpServerTool(Name = "add_cash_flow_spreadsheet_url")]
  [Description("Associate a Google financial planning spreadsheet with a user. Inputs: phone_number (E.164), url (Google Sheet link). Fails if user not found, already has a sheet, or URL invalid. Returns { message, phone_number } on success. " + ToolDocs.GenericError)]
  public async Task<string> AddCashFlowSpreadsheetUrl(string phone_number, string url) {
    try {
      await cashFlowService.AddSpreadsheetUrl(phone_number, url);
      return Printable.Make(new { message = "Spreadsheet linked successfully", phone_number });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_all_transactions")]
  [Description("Fetch all transactions for the user. Input: phone_number. Success: { count, transactions: [ { sheet_id, date, value, category, description, bank_account } ] }. " + ToolDocs.GenericError)]
  public async Task<string> GetAllTransactions(string phone_number) {
    try {
      var transactions = await cashFlowService.GetAllTransactions(phone_number);
      return Printable.Make(new { count = transactions.Count, transactions });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_last_transaction")]
  [Description("Get the most recently appended transaction. Input: phone_number. Success: { transaction | null }. " + ToolDocs.GenericError)]
  public async Task<string> GetLastTransaction(string phone_number) {
    try {
      var transaction = await cashFlowService.GetLastTransaction(phone_number);
      return Printable.Make(new { transaction });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "delete_last_transaction")]
  [Description("Delete the last (most recent) transaction. Input: phone_number. Success: { message }. " + ToolDocs.GenericError)]
  public async Task<string> DeleteLastTransaction(string phone_number) {
    try {
      await cashFlowService.DeleteLastTransaction(phone_number);
      return Printable.Make(new { message = "Last transaction deleted" });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "add_transaction")]
  [Description("Append a transaction specifying its type. Inputs: phone_number, type (Expense|Earning), description, value (number), date (ISO-8601 optional; defaults current UTC date). Category and bank_account are AUTOMATICALLY resolved by an internal classification prompt that fetches the proper categories list (expense or earning) plus bank accounts. Success: { message, type, category, bank_account, date, value }. " + ToolDocs.GenericError)]
  public async Task<string> AddTransaction(string phone_number, string type, string description, double value, DateTime? date) {
    try {
      var finalDate = date ?? DateTime.UtcNow;
      var (expenseCategories, earningCategories, bankAccounts) = await cashFlowService.GetCategoriesAndBankAccounts(phone_number);
      var categories = expenseCategories.Concat(earningCategories).Distinct().ToList();
      var parsed = await ClassifyWithRetry(phone_number, type, description, value, categories, bankAccounts);
      if (type == "Expense") {
        var dtoExpense = new CashFlowAddExpenseDTO { PhoneNumber = phone_number, Date = finalDate, Value = value, Category = parsed.Category, Description = description, BankAccount = parsed.BankAccount };
        await cashFlowService.AddExpense(dtoExpense);
      } else {
        var dtoEarning = new CashFlowAddEarningDTO { PhoneNumber = phone_number, Date = finalDate, Value = value, Category = parsed.Category, Description = description, BankAccount = parsed.BankAccount };
        await cashFlowService.AddEarning(dtoEarning);
      }
      return Printable.Make(new { message = "Transaction added", type, category = parsed.Category, bank_account = parsed.BankAccount, date = finalDate, value });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  private async Task<ClassificationResult> ClassifyWithRetry(string phoneNumber, string type, string description, double value, List<string> categories, List<string> bankAccounts, int attempt = 1) {
    try {
      var prompt = PromptLoader.GetTransactionClassification(PromptLocale.PtBr);
      var payload = new { type, description, value, available_categories = categories, available_bank_accounts = bankAccounts };
      var messages = new List<AiChatMessage> {
        new() { Role = AiChatRole.System, Type = AiChatMessageType.Text, Text = prompt },
        new() { Role = AiChatRole.User, Type = AiChatMessageType.Text, Text = Printable.Make(payload) }
      };
      var response = await aiChatGateway.GetResponse(phoneNumber, messages, allowMcp: false);
      var result = Printable.Convert<ClassificationResult>(response.Text);
      if (result == null) return await ClassifyWithRetry(phoneNumber, type, description, value, categories, bankAccounts, attempt + 1);
      return result;
    } catch (Exception err) {
      if (attempt > 5) throw new ValidationException("Could not determine classification or bank account." + err.Message + err.StackTrace);
      return await ClassifyWithRetry(phoneNumber, type, description, value, categories, bankAccounts, attempt + 1);
    }
  }

  [McpServerTool(Name = "get_expense_categories")]
  [Description("List expense categories. Input: phone_number. Success: { count, categories }. " + ToolDocs.GenericError)]
  public async Task<string> GetExpenseCategories(string phone_number) {
    try {
      var categories = await cashFlowService.GetExpenseCategories(phone_number);
      return Printable.Make(new { count = categories.Count, categories });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_earning_categories")]
  [Description("List earning categories. Input: phone_number. Success: { count, categories }. " + ToolDocs.GenericError)]
  public async Task<string> GetEarningCategories(string phone_number) {
    try {
      var categories = await cashFlowService.GetEarningCategories(phone_number);
      return Printable.Make(new { count = categories.Count, categories });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_bank_accounts")]
  [Description("List bank accounts referenced. Input: phone_number. Success: { count, bank_accounts }. " + ToolDocs.GenericError)]
  public async Task<string> GetBankAccounts(string phone_number) {
    try {
      var banks = await cashFlowService.GetBankAccount(phone_number);
      return Printable.Make(new { count = banks.Count, bank_accounts = banks });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }
}

class ClassificationResult {
  public required string Type { get; set; }
  public required string Category { get; set; }
  public required string BankAccount { get; set; }
}
