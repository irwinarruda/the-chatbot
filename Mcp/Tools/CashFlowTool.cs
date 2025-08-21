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
  [Description("Associate a Google financial planning spreadsheet with a user. Fails if user not found, already has a sheet, or URL invalid. Returns { message }. " + ToolDocs.GenericError)]
  public async Task<string> AddCashFlowSpreadsheetUrl(
    [Description("User phone number in E.164 format")] string phone_number,
    [Description("Google Spreadsheet URL")] string url
  ) {
    try {
      await cashFlowService.AddSpreadsheetUrl(phone_number, url);
      return Printable.Make(new { Message = "Spreadsheet linked successfully" });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_all_transactions")]
  [Description("Fetch all transactions for the user. Returns { count, transactions: [ { sheet_id, date, value, category, description, bank_account } ] }. " + ToolDocs.GenericError)]
  public async Task<string> GetAllTransactions(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      var transactions = await cashFlowService.GetAllTransactions(phone_number);
      return Printable.Make(new { transactions.Count, transactions });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_last_transaction")]
  [Description("Get the most recently appended transaction. Returns { transaction | null }. " + ToolDocs.GenericError)]
  public async Task<string> GetLastTransaction(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      var transaction = await cashFlowService.GetLastTransaction(phone_number);
      return Printable.Make(new { transaction });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "delete_last_transaction")]
  [Description("Delete the last (most recent) transaction. Returns { message }. " + ToolDocs.GenericError)]
  public async Task<string> DeleteLastTransaction(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      await cashFlowService.DeleteLastTransaction(phone_number);
      return Printable.Make(new { Message = "Last transaction deleted" });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "add_transaction")]
  [Description("Append a transaction specifying its type. Category and bank_account are automatically resolved via classification using available categories and bank accounts. Returns { message, type, category, bank_account, date, value }. " + ToolDocs.GenericError)]
  public async Task<string> AddTransaction(
    [Description("User phone number in E.164 format")] string phone_number,
    [Description("Transaction type: Expense or Earning")] string type,
    [Description("Full original user message text with all context and nuances; pass exactly what the user sent")] string user_message,
    [Description("Monetary value (positive number)")] double value,
    [Description("ISO-8601 date (if not explicit, pass in null)")] DateTime? date
  ) {
    try {
      date ??= DateTime.UtcNow;
      var (categories, bankAccounts) = await cashFlowService.GetCategoriesAndBankAccounts(phone_number);
      var parsed = await ClassifyWithRetry(phone_number, type, user_message, value, categories, bankAccounts);
      if (type == "Expense") {
        var dtoExpense = new CashFlowAddExpenseDTO { PhoneNumber = phone_number, Date = date.Value, Value = value, Category = parsed.Category, Description = parsed.Description, BankAccount = parsed.BankAccount };
        await cashFlowService.AddExpense(dtoExpense);
      } else {
        var dtoEarning = new CashFlowAddEarningDTO { PhoneNumber = phone_number, Date = date.Value, Value = value, Category = parsed.Category, Description = parsed.Description, BankAccount = parsed.BankAccount };
        await cashFlowService.AddEarning(dtoEarning);
      }
      return Printable.Make(new { Message = "Transaction added", type, parsed.Category, parsed.BankAccount, parsed.Description, date, value });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  private async Task<ClassificationResult> ClassifyWithRetry(string phoneNumber, string type, string userMessage, double value, List<string> categories, List<string> bankAccounts, int attempt = 1) {
    try {
      var prompt = PromptLoader.GetTransactionClassification(PromptLocale.PtBr);
      var payload = Printable.Make(new { type, description = userMessage, value, categories, bankAccounts });
      var messages = new List<AiChatMessage> {
        new() { Role = AiChatRole.System, Type = AiChatMessageType.Text, Text = prompt },
        new() { Role = AiChatRole.User, Type = AiChatMessageType.Text, Text = payload }
      };
      var response = await aiChatGateway.GetResponse(phoneNumber, messages, allowMcp: false);
      var result = Printable.Convert<ClassificationResult>(response.Text);
      if (result == null) return await ClassifyWithRetry(phoneNumber, type, userMessage, value, categories, bankAccounts, attempt + 1);
      return result;
    } catch (Exception err) {
      if (attempt > 5) throw new ValidationException("Could not determine classification or bank account." + err.Message + err.StackTrace);
      return await ClassifyWithRetry(phoneNumber, type, userMessage, value, categories, bankAccounts, attempt + 1);
    }
  }

  [McpServerTool(Name = "get_expense_categories")]
  [Description("List expense categories. Returns { count, categories }. " + ToolDocs.GenericError)]
  public async Task<string> GetExpenseCategories(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      var categories = await cashFlowService.GetExpenseCategories(phone_number);
      return Printable.Make(new { categories.Count, categories });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_earning_categories")]
  [Description("List earning categories. Returns { count, categories }. " + ToolDocs.GenericError)]
  public async Task<string> GetEarningCategories(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      var categories = await cashFlowService.GetEarningCategories(phone_number);
      return Printable.Make(new { categories.Count, categories });
    } catch (Exception ex) {
      var response = ExceptionResponse.Handle(ex);
      return Printable.Make(response);
    }
  }

  [McpServerTool(Name = "get_bank_accounts")]
  [Description("List bank accounts referenced. Returns { count, bank_accounts }. " + ToolDocs.GenericError)]
  public async Task<string> GetBankAccounts(
    [Description("User phone number in E.164 format")] string phone_number
  ) {
    try {
      var banks = await cashFlowService.GetBankAccount(phone_number);
      return Printable.Make(new { banks.Count, banks });
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
  public required string Description { get; set; }
}
