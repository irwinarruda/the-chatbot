using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
  readonly public AuthService authService;
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IGoogleAuthGateway googleAuthGateway;
  readonly public IConfiguration configuration;
  readonly public DatabaseConfig databaseConfig;
  readonly public GoogleSheetsConfig googleSheetsConfig;
  readonly public GoogleConfig googleConfig;

  public CustomWebApplicationFactory() {
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    googleConfig = configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    if (googleSheetsConfig.TestSheetId != "TestSheetId") {
      finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet(googleConfig);
    } else {
      googleAuthGateway = new GoogleAuthGateway(googleConfig);
      finantialPlanningSpreadsheet = new TestFinantialPlanningSpreadsheet(googleSheetsConfig);
    }
    authService = new AuthService(googleAuthGateway!);
  }
}
