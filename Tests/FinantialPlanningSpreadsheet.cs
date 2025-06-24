using Microsoft.Extensions.Configuration;

using TheChatbot.Dtos;
using TheChatbot.Resources;

namespace Tests;

public class FinantialPlanningSpreadsheet : IClassFixture<CustomWebApplicationFactory> {
  private readonly CustomWebApplicationFactory factory;
  private readonly ITestOutputHelper console;
  private readonly IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  private readonly GoogleSheetsConfig config;
  public FinantialPlanningSpreadsheet(CustomWebApplicationFactory _factory, ITestOutputHelper _console) {
    config = _factory.configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    factory = _factory;
    console = _console;
    finantialPlanningSpreadsheet = factory.finantialPlanningSpreadsheet;
  }

  [Fact]
  public async Task GetExpenseCategoriesShouldWork() {
    var categories = await finantialPlanningSpreadsheet.GetExpenseCategories(new SheetConfigDTO {
      SheetId = config.MainId,
      SheetAccessToken = config.MainToken
    });
    Assert.NotEmpty(categories);
    Assert.Equal(60, categories.Count);
  }

  [Fact]
  public async Task GetEarningCategoriesShouldWork() {
    var categories = await finantialPlanningSpreadsheet.GetEarningCategories(new SheetConfigDTO {
      SheetId = config.MainId,
      SheetAccessToken = config.MainToken
    });
    Assert.NotEmpty(categories);
    Assert.Equal(11, categories.Count);
  }

  [Fact]
  public async Task AddExpense() {
    await finantialPlanningSpreadsheet.AddExpense(new AddExpenseDTO {
      SheetId = config.MainId,
      SheetAccessToken = config.MainToken,
      Date = DateTime.Now,
      Value = 5.2,
      Category = "Delivery",
      Description = "Test",
      BankAccount = "NuConta",
    });
    console.WriteLine("PLEEEEEASE");
  }
}
