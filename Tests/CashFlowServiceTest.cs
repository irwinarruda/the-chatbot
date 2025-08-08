using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Services;

using Xunit.v3.Priority;

namespace Tests;

[TestCaseOrderer(typeof(PriorityOrderer))]
public class CashFlowServiceTest : IClassFixture<Orquestrator> {
  private readonly Orquestrator orquestrator;
  private readonly CashFlowService cashFlowService;
  private readonly AuthService authService;
  public CashFlowServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    cashFlowService = _orquestrator.cashFlowService;
    authService = _orquestrator.authService;
  }

  private async Task SetupUserWithSpreadsheet(string phoneNumber, string sheetId) {
    var encryption = new Encryption(orquestrator.encryptionConfig.Text32Bytes, orquestrator.encryptionConfig.Text16Bytes);
    await authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber), "rightCode");
    var url = $"https://docs.google.com/spreadsheets/d/{sheetId}/edit?gid=0#gid=0";
    await cashFlowService.AddSpreadsheetUrl(phoneNumber, url);
  }

  [Fact, Priority(0)]
  public async Task AddSpreadsheetUrl_ShouldValidateUrlParsing() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511980000000";
    // Ensure the user exists so we test URL parsing behavior, not user-not-found
    await orquestrator.CreateUser(phoneNumber: phoneNumber);

    Should.Throw<Exception>(() => cashFlowService.AddSpreadsheetUrl(phoneNumber, "WrongURL"));
    Should.Throw<Exception>(() => cashFlowService.AddSpreadsheetUrl(phoneNumber, "http://"));
    Should.Throw<Exception>(() => cashFlowService.AddSpreadsheetUrl(phoneNumber, "https://docs.google.com/spreadsheets/d"));
    Should.Throw<Exception>(() => cashFlowService.AddSpreadsheetUrl(phoneNumber, "https://docs.google.com/spreadsheets/d/"));

    var okUrl = $"https://docs.google.com/spreadsheets/d/{orquestrator.googleSheetsConfig.TestSheetId}/edit?gid=12345#gid=12345";
    await cashFlowService.AddSpreadsheetUrl(phoneNumber, okUrl);
  }

  [Fact, Priority(1)]
  public async Task GetAndDeleteTransactionShouldNotWorkWithoutData() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511984444444";
    await SetupUserWithSpreadsheet(phoneNumber, orquestrator.googleSheetsConfig.TestSheetId);

    var transactions = await cashFlowService.GetAllTransactions(phoneNumber);
    transactions.ShouldBeEmpty();

    var transaction = await cashFlowService.GetLastTransaction(phoneNumber);
    transaction.ShouldBeNull();

    await Should.ThrowAsync<ValidationException>(() => cashFlowService.DeleteLastTransaction(phoneNumber));
  }

  [Fact, Priority(2)]
  public async Task AddExpenseShouldWork() {
    var phoneNumber = "5511984444444"; // same user as previous to keep gateway state

    var addExpense = new AddExpenseInput {
      PhoneNumber = phoneNumber,
      Date = DateTime.Now,
      Value = 5.2,
      Category = "Delivery",
      Description = "UniqueExpense",
      BankAccount = "NuConta",
    };
    await cashFlowService.AddExpense(addExpense);

    var lastTransaction = await cashFlowService.GetLastTransaction(phoneNumber);
    lastTransaction.ShouldNotBeNull();
    lastTransaction!.Date.ShouldBe(addExpense.Date.Date);
    lastTransaction.Value.ShouldBe(-addExpense.Value);
    lastTransaction.Description.ShouldBe(addExpense.Description);
    lastTransaction.Category.ShouldBe(addExpense.Category);
    lastTransaction.BankAccount.ShouldBe(addExpense.BankAccount);
  }

  [Fact, Priority(3)]
  public async Task DeleteLastTransactionShouldWork() {
    var phoneNumber = "5511984444444"; // same user
    await cashFlowService.DeleteLastTransaction(phoneNumber);
    var lastTransaction = await cashFlowService.GetLastTransaction(phoneNumber);
    lastTransaction.ShouldBeNull();
  }

  [Fact, Priority(4)]
  public async Task GetTransactionsShouldWork() {
    var phoneNumber = "5511984444444"; // same user

    var expenseCategories = await cashFlowService.GetExpenseCategories(phoneNumber);
    var bankAccount = await cashFlowService.GetBankAccount(phoneNumber);

    var newTransactions = new List<AddExpenseInput> {
      new() {
        PhoneNumber = phoneNumber,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 1000,
        Category = "Supermercado",
        Description = "Compras do mês",
        BankAccount = "NuConta"
      },
      new() {
        PhoneNumber = phoneNumber,
        Date = DateTime.ParseExact("2025-01-01", "yyyy-MM-dd", null),
        Value = 600,
        Category = "Seguro do carro",
        Description = "Seguro do meu carro",
        BankAccount = "NuConta"
      },
    };

    foreach (var tx in newTransactions) {
      await cashFlowService.AddExpense(tx);
    }

    var transactions = await cashFlowService.GetAllTransactions(phoneNumber);
    transactions.Count.ShouldBe(2);
    transactions.ShouldNotBeEmpty();

    foreach (var transaction in transactions) {
      transaction.SheetId.ShouldBe(orquestrator.googleSheetsConfig.TestSheetId);
      var isCategoryValid = expenseCategories.Contains(transaction.Category) || bankAccount.Contains(transaction.BankAccount);
      isCategoryValid.ShouldBeTrue();
    }

    for (var i = 0; i < newTransactions.Count; i++) {
      await cashFlowService.DeleteLastTransaction(phoneNumber);
    }
  }

  [Fact]
  public async Task GetWrongSheetIdShouldNotWork() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511977777777";
    // user with credentials
    var encryption = new Encryption(orquestrator.encryptionConfig.Text32Bytes, orquestrator.encryptionConfig.Text16Bytes);
    await authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber), "rightCode");
    // add spreadsheet with wrong id
    var wrongUrl = "https://docs.google.com/spreadsheets/d/WrongSheet/edit?gid=0#gid=0";
    await cashFlowService.AddSpreadsheetUrl(phoneNumber, wrongUrl);

    await Should.ThrowAsync<ServiceException>(() => cashFlowService.GetAllTransactions(phoneNumber));
    await Should.ThrowAsync<ServiceException>(() => cashFlowService.DeleteLastTransaction(phoneNumber));
    await Should.ThrowAsync<ServiceException>(() => cashFlowService.AddExpense(new AddExpenseInput {
      PhoneNumber = phoneNumber,
      Date = DateTime.Now,
      Value = 1,
      Category = "Any",
      Description = "Any",
      BankAccount = "Any"
    }));
  }

  [Fact]
  public async Task GetExpenseCategoriesShouldWork() {
    await orquestrator.ClearDatabase();
    var okPhone = "5511966666666";
    await SetupUserWithSpreadsheet(okPhone, orquestrator.googleSheetsConfig.TestSheetId);

    var expenseCategories = await cashFlowService.GetExpenseCategories(okPhone);
    expenseCategories.ShouldNotBeEmpty();

    var wrongPhone = "5511955555555";
    await SetupUserWithSpreadsheet(wrongPhone, "WrongSheet");
    await Should.ThrowAsync<ServiceException>(() => cashFlowService.GetExpenseCategories(wrongPhone));
  }

  [Fact]
  public async Task GetEarningCategoriesShouldWork() {
    await orquestrator.ClearDatabase();
    var okPhone = "5511944444444";
    await SetupUserWithSpreadsheet(okPhone, orquestrator.googleSheetsConfig.TestSheetId);

    var earningCategories = await cashFlowService.GetEarningCategories(okPhone);
    earningCategories.ShouldNotBeEmpty();

    var wrongPhone = "5511933333333";
    await SetupUserWithSpreadsheet(wrongPhone, "WrongSheet");
    await Should.ThrowAsync<ServiceException>(() => cashFlowService.GetEarningCategories(wrongPhone));
  }

  [Fact]
  public async Task GetBankAccountShouldWork() {
    await orquestrator.ClearDatabase();
    var okPhone = "5511922222222";
    await SetupUserWithSpreadsheet(okPhone, orquestrator.googleSheetsConfig.TestSheetId);

    var bankAccount = await cashFlowService.GetBankAccount(okPhone);
    bankAccount.ShouldNotBeEmpty();

    var wrongPhone = "5511911111111";
    await SetupUserWithSpreadsheet(wrongPhone, "WrongSheet");
    await Should.ThrowAsync<ServiceException>(() => cashFlowService.GetBankAccount(wrongPhone));
  }
}
