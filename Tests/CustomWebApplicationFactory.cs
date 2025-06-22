using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TheChatbot.Resources;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IConfiguration configuration;
  public CustomWebApplicationFactory() {
    finantialPlanningSpreadsheet = new GoogleFinantialPlanningSpreadsheet();
    configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile("appsettings.Development.json", optional: false)
      .Build();
  }
  protected override IHost CreateHost(IHostBuilder builder) {
    builder.ConfigureAppConfiguration((context, configBuilder) => {
      configBuilder.AddInMemoryCollection([
        new KeyValuePair<string, string?>("IsTestingEnvironment", "true")
      ]);
    });
    return base.CreateHost(builder);
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder) {
    builder.UseEnvironment("Development");

    builder.ConfigureServices(services => {
      // You can replace services with test doubles here
      // For example, you could mock external dependencies
    });

    builder.ConfigureLogging(logging => {
      logging.ClearProviders();
      logging.AddConsole();
      logging.AddDebug();
    });
  }
}
