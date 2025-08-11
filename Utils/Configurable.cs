namespace TheChatbot.Utils;

public class Configurable {
  public static IConfigurationRoot Make() {
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{env}.json", optional: true)
      .Build();
    return configuration;
  }

  public static void Enhance(ConfigurationManager configuration) {
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    configuration.SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{env}.json", optional: true)
      .Build();
  }
}
