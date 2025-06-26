using Microsoft.Extensions.Configuration;

using Shouldly;

using TheChatbot.Dtos;
using TheChatbot.Resources;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class FinantialPlanningSpreadsheet : IClassFixture<CustomWebApplicationFactory> {
  private readonly CustomWebApplicationFactory factory;
  private readonly IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  private readonly GoogleSheetsConfig config;
  public FinantialPlanningSpreadsheet(CustomWebApplicationFactory _factory) {
    config = _factory.configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    factory = _factory;
    finantialPlanningSpreadsheet = factory.finantialPlanningSpreadsheet;
  }

  [Fact, Priority(1)]
  public async Task GetAndDeleteTransactionShouldNotWorkWithoutData() {
    var sheetConfig = new SheetConfigDTO { SheetId = config.MainId };
    var transactions = await finantialPlanningSpreadsheet.GetAllTransactions(sheetConfig);
    transactions.ShouldBeEmpty();
    var transaction = await finantialPlanningSpreadsheet.GetLastTransaction(sheetConfig);
    transaction.ShouldBeNull();
    await Should.ThrowAsync<Exception>(async () => await finantialPlanningSpreadsheet.DeleteLastTransaction(sheetConfig));
  }

  [Fact, Priority(2)]
  public async Task AddExpenseShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = config.MainId };
    var addExpense = new AddExpenseDTO {
      SheetId = config.MainId,
      Date = DateTime.Now,
      Value = 5.2,
      Category = "Delivery",
      Description = "UniqueExpense",
      BankAccount = "NuConta",
    };
    await finantialPlanningSpreadsheet.AddExpense(addExpense);
    var lastTransaction = await finantialPlanningSpreadsheet.GetLastTransaction(sheetConfig);
    lastTransaction.ShouldNotBeNull();
    lastTransaction.Date.ShouldBe(addExpense.Date.Date);
    lastTransaction.Value.ShouldBe(addExpense.Value);
    lastTransaction.Description.ShouldBe(addExpense.Description);
    lastTransaction.Category.ShouldBe(addExpense.Category);
    lastTransaction.BankAccount.ShouldBe(addExpense.BankAccount);
  }

  [Fact, Priority(3)]
  public async Task DeleteLastTransactionShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = config.MainId };
    await finantialPlanningSpreadsheet.DeleteLastTransaction(sheetConfig);
    var lastTransaction = await finantialPlanningSpreadsheet.GetLastTransaction(sheetConfig);
    lastTransaction.ShouldBeNull();
  }

  [Fact, Priority(4)]
  public async Task GetTransactionsShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = config.MainId };
    var result = await Task.WhenAll(
      finantialPlanningSpreadsheet.GetExpenseCategories(sheetConfig),
      finantialPlanningSpreadsheet.GetBankAccount(sheetConfig)
    );
    var expenseCategories = result[0];
    var bankAccount = result[1];
    var newTransactions = new List<AddExpenseDTO> {
      new() {
        SheetId = config.MainId,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 1000,
        Category = "Salário",
        Description = "Salário de Janeiro",
        BankAccount = "NuConta"
      },
      new() {
        SheetId = config.MainId,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 600,
        Category = "Salário",
        Description = "Vale alimentação",
        BankAccount = "Caju"
      },
    };
    foreach (var newTransaction in newTransactions) {
      await finantialPlanningSpreadsheet.AddExpense(newTransaction);
    }
    var transactions = await finantialPlanningSpreadsheet.GetAllTransactions(sheetConfig);
    transactions.Count.ShouldBe(2);
    transactions.ShouldNotBeEmpty();
    foreach (var transaction in transactions) {
      transaction.SheetId.ShouldBe(config.MainId);
      var isCategoryValid = expenseCategories.Contains(transaction.Category) || bankAccount.Contains(transaction.BankAccount);
      isCategoryValid.ShouldBeTrue();
    }
    for (var i = 0; i < newTransactions.Count; i++) {
      await finantialPlanningSpreadsheet.DeleteLastTransaction(sheetConfig);
    }
  }

  [Fact]
  public async Task GetExpenseCategoriesShouldWork() {
    var expenseCategories = await finantialPlanningSpreadsheet.GetExpenseCategories(new SheetConfigDTO {
      SheetId = config.MainId,
    });
    expenseCategories.ShouldNotBeEmpty();
  }

  [Fact]
  public async Task GetEarningCategoriesShouldWork() {
    var earningCategories = await finantialPlanningSpreadsheet.GetEarningCategories(new SheetConfigDTO {
      SheetId = config.MainId,
    });
    earningCategories.ShouldNotBeEmpty();
  }

  [Fact]
  public async Task GetBankAccountShouldWork() {
    var bankAccount = await finantialPlanningSpreadsheet.GetBankAccount(new SheetConfigDTO {
      SheetId = config.MainId,
    });
    bankAccount.ShouldNotBeEmpty();
  }

  [Fact]
  public void GetSpreadSheetIdByUrl() {
    Should.Throw<Exception>(() => finantialPlanningSpreadsheet.GetSpreadSheetIdByUrl("WrongURL"));
    Should.Throw<Exception>(() => finantialPlanningSpreadsheet.GetSpreadSheetIdByUrl("http://"));
    Should.Throw<Exception>(() => finantialPlanningSpreadsheet.GetSpreadSheetIdByUrl("https://docs.google.com/spreadsheets/d"));
    Should.Throw<Exception>(() => finantialPlanningSpreadsheet.GetSpreadSheetIdByUrl("https://docs.google.com/spreadsheets/d/"));
    var id = finantialPlanningSpreadsheet.GetSpreadSheetIdByUrl("https://docs.google.com/spreadsheets/d/newidhere/edit?gid=12345#gid=12345");
    id.ShouldBe("newidhere");
  }
}
