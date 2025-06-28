using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

using TheChatbot.Resources;
using TheChatbot.Utils;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IConfiguration configuration;
  public CustomWebApplicationFactory() {
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    configuration = Configurable.Make();
    if (env == "Development") {
      finantialPlanningSpreadsheet = new TestFinantialPlanningSpreadsheet();
    } else {
      finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet(configuration);
    }
  }
}
