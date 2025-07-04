using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Utils;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IConfiguration configuration;
  readonly public DatabaseConfig databaseConfig;
  readonly public GoogleSheetsConfig googleSheetsConfig;

  public CustomWebApplicationFactory() {
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    if (googleSheetsConfig.TestSheetId != "uniqueId") {
      finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet(configuration);
    } else {
      finantialPlanningSpreadsheet = new TestFinantialPlanningSpreadsheet();
    }
  }
}
