using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program> {
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
