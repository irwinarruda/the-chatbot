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
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    if (env == "Development") {
      finantialPlanningSpreadsheet = new TestFinantialPlanningSpreadsheet();
    } else {
      finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet(configuration);
    }
  }
}
