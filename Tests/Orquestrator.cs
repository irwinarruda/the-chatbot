using Bogus;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class Orquestrator : WebApplicationFactory<Program> {
  readonly public AuthService authService;
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IGoogleAuthGateway googleAuthGateway;
  readonly public IConfiguration configuration;
  readonly public AppDbContext database;
  readonly public DatabaseConfig databaseConfig;
  readonly public GoogleSheetsConfig googleSheetsConfig;
  readonly public GoogleConfig googleConfig;

  public Orquestrator() {
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    googleConfig = configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    database = new AppDbContext();
    if (googleSheetsConfig.TestSheetId != "TestSheetId") {
      finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet(googleConfig);
    } else {
      finantialPlanningSpreadsheet = new TestFinantialPlanningSpreadsheet(googleSheetsConfig);
    }
    googleAuthGateway = new GoogleAuthGateway(googleConfig);
    authService = new AuthService(database, googleAuthGateway);
  }

  public async Task ClearDatabase() {
    await database.Execute($"DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
  }

  public async Task RunPendingMigrations() {
    await database.Database.MigrateAsync();
  }

  public async Task<User> CreateUser(string? name = null, string? phoneNumber = null) {
    var faker = new Faker();
    var user = await authService.CreateUser(name ?? faker.Name.FullName(), phoneNumber ?? faker.Phone.PhoneNumber("+55###########"));
    return user;
  }
}
