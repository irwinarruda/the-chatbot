using Microsoft.Extensions.Configuration;

using Shouldly;

using TheChatbot.Dtos;
using TheChatbot.Resources;

namespace Tests;

public class FinantialPlanningSpreadsheet : IClassFixture<CustomWebApplicationFactory> {
  private readonly CustomWebApplicationFactory factory;
  private readonly IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  private readonly GoogleSheetsConfig config;
  public FinantialPlanningSpreadsheet(CustomWebApplicationFactory _factory) {
    config = _factory.configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    factory = _factory;
    finantialPlanningSpreadsheet = factory.finantialPlanningSpreadsheet;
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
  public async Task GetTransactionsShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = config.MainId };
    var result = await Task.WhenAll(
      finantialPlanningSpreadsheet.GetExpenseCategories(sheetConfig),
      finantialPlanningSpreadsheet.GetBankAccount(sheetConfig)
    );
    var expenseCategories = result[0];
    var bankAccount = result[1];
    var transactions = await finantialPlanningSpreadsheet.GetAllTransactions(sheetConfig);
    transactions.ShouldNotBeEmpty();
    foreach (var transaction in transactions) {
      transaction.SheetId.ShouldBe(config.MainId);
      var isCategoryValid = expenseCategories.Contains(transaction.Category) || bankAccount.Contains(transaction.BankAccount);
      isCategoryValid.ShouldBeTrue();
    }
  }

  [Fact]
  public async Task GetLastTransactionShouldWork() {
    var category = await finantialPlanningSpreadsheet.GetLastTransaction(new SheetConfigDTO {
      SheetId = config.MainId,
    });
    category.ShouldNotBeNull();
    category.Date.ToString("dd/MM/yyyy").ShouldBe("04/01/2025");
    category.Value.ShouldBe(-120.20);
    category.Description.ShouldBe("Conta do celular Vivo");
  }

  [Fact]
  public async Task AddExpenseAndDeleteLastTransactionShouldWork() {
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
    await finantialPlanningSpreadsheet.DeleteLastTransaction(sheetConfig);
    lastTransaction = await finantialPlanningSpreadsheet.GetLastTransaction(sheetConfig);
    lastTransaction.ShouldNotBeNull();
    lastTransaction.Description.ShouldNotBe(addExpense.Description);
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
