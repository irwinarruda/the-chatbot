using Shouldly;

using TheChatbot.Infra;
using TheChatbot.Resources;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class CashFlowSpreadsheetTest : IClassFixture<Orquestrator> {
  private readonly Orquestrator orquestrator;
  private readonly ICashFlowSpreadsheetGateway cashFlowSpreadsheetGateway;
  public CashFlowSpreadsheetTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    cashFlowSpreadsheetGateway = orquestrator.cashFlowSpreadsheetGateway;
  }

  [Fact, Priority(1)]
  public async Task GetAndDeleteTransactionShouldNotWorkWithoutData() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var transactions = await cashFlowSpreadsheetGateway.GetAllTransactions(sheetConfig);
    transactions.ShouldBeEmpty();
    var transaction = await cashFlowSpreadsheetGateway.GetLastTransaction(sheetConfig);
    transaction.ShouldBeNull();
    await Should.ThrowAsync<ValidationException>(() => cashFlowSpreadsheetGateway.DeleteLastTransaction(sheetConfig));
  }

  [Fact, Priority(2)]
  public async Task AddExpenseShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var addExpense = new AddExpenseDTO {
      SheetId = orquestrator.googleSheetsConfig.TestSheetId,
      Date = DateTime.Now,
      Value = 5.2,
      Category = "Delivery",
      Description = "UniqueExpense",
      BankAccount = "NuConta",
    };
    await cashFlowSpreadsheetGateway.AddExpense(addExpense);
    var lastTransaction = await cashFlowSpreadsheetGateway.GetLastTransaction(sheetConfig);
    lastTransaction.ShouldNotBeNull();
    lastTransaction.Date.ShouldBe(addExpense.Date.Date);
    lastTransaction.Value.ShouldBe(addExpense.Value);
    lastTransaction.Description.ShouldBe(addExpense.Description);
    lastTransaction.Category.ShouldBe(addExpense.Category);
    lastTransaction.BankAccount.ShouldBe(addExpense.BankAccount);
  }

  [Fact, Priority(3)]
  public async Task DeleteLastTransactionShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    await cashFlowSpreadsheetGateway.DeleteLastTransaction(sheetConfig);
    var lastTransaction = await cashFlowSpreadsheetGateway.GetLastTransaction(sheetConfig);
    lastTransaction.ShouldBeNull();
  }

  [Fact, Priority(4)]
  public async Task GetTransactionsShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var result = await Task.WhenAll(
      cashFlowSpreadsheetGateway.GetExpenseCategories(sheetConfig),
      cashFlowSpreadsheetGateway.GetBankAccount(sheetConfig)
    );
    var expenseCategories = result[0];
    var bankAccount = result[1];
    var newTransactions = new List<AddExpenseDTO> {
      new() {
        SheetId = orquestrator.googleSheetsConfig.TestSheetId,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 1000,
        Category = "Supermercado",
        Description = "Compras do mês",
        BankAccount = "NuConta"
      },
      new() {
        SheetId = orquestrator.googleSheetsConfig.TestSheetId,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 600,
        Category = "Seguro do carro",
        Description = "Seguro do meu carro",
        BankAccount = "NuConta"
      },
    };
    foreach (var newTransaction in newTransactions) {
      await cashFlowSpreadsheetGateway.AddExpense(newTransaction);
    }
    var transactions = await cashFlowSpreadsheetGateway.GetAllTransactions(sheetConfig);
    transactions.Count.ShouldBe(2);
    transactions.ShouldNotBeEmpty();
    foreach (var transaction in transactions) {
      transaction.SheetId.ShouldBe(orquestrator.googleSheetsConfig.TestSheetId);
      var isCategoryValid = expenseCategories.Contains(transaction.Category) || bankAccount.Contains(transaction.BankAccount);
      isCategoryValid.ShouldBeTrue();
    }
    for (var i = 0; i < newTransactions.Count; i++) {
      await cashFlowSpreadsheetGateway.DeleteLastTransaction(sheetConfig);
    }
  }

  [Fact]
  public async Task GetWorngSheetIdShouldNotWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = "WrongSheet" };
    var transactionDTO = new AddTransactionDTO {
      BankAccount = "",
      Category = "",
      Description = "",
      SheetId = "WrongSheet"
    };
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.GetAllTransactions(sheetConfig));
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.DeleteLastTransaction(sheetConfig));
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.AddTransaction(transactionDTO));
  }

  [Fact]
  public async Task GetExpenseCategoriesShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var expenseCategories = await cashFlowSpreadsheetGateway.GetExpenseCategories(sheetConfig);
    expenseCategories.ShouldNotBeEmpty();
    sheetConfig.SheetId = "WrongSheet";
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.GetExpenseCategories(sheetConfig));
  }

  [Fact]
  public async Task GetEarningCategoriesShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var earningCategories = await cashFlowSpreadsheetGateway.GetEarningCategories(new SheetConfigDTO {
      SheetId = orquestrator.googleSheetsConfig.TestSheetId,
    });
    earningCategories.ShouldNotBeEmpty();
    sheetConfig.SheetId = "WrongSheet";
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.GetEarningCategories(sheetConfig));
  }

  [Fact]
  public async Task GetBankAccountShouldWork() {
    var sheetConfig = new SheetConfigDTO { SheetId = orquestrator.googleSheetsConfig.TestSheetId };
    var bankAccount = await cashFlowSpreadsheetGateway.GetBankAccount(new SheetConfigDTO {
      SheetId = orquestrator.googleSheetsConfig.TestSheetId,
    });
    bankAccount.ShouldNotBeEmpty();
    sheetConfig.SheetId = "WrongSheet";
    await Should.ThrowAsync<ServiceException>(() => cashFlowSpreadsheetGateway.GetEarningCategories(sheetConfig));
  }

  [Fact]
  public void GetSpreadSheetIdByUrl() {
    Should.Throw<Exception>(() => cashFlowSpreadsheetGateway.GetSpreadsheetIdByUrl("WrongURL"));
    Should.Throw<Exception>(() => cashFlowSpreadsheetGateway.GetSpreadsheetIdByUrl("http://"));
    Should.Throw<Exception>(() => cashFlowSpreadsheetGateway.GetSpreadsheetIdByUrl("https://docs.google.com/spreadsheets/d"));
    Should.Throw<Exception>(() => cashFlowSpreadsheetGateway.GetSpreadsheetIdByUrl("https://docs.google.com/spreadsheets/d/"));
    var id = cashFlowSpreadsheetGateway.GetSpreadsheetIdByUrl("https://docs.google.com/spreadsheets/d/newidhere/edit?gid=12345#gid=12345");
    id.ShouldBe("newidhere");
  }
}
