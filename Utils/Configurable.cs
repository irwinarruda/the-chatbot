namespace TheChatbot.Utils;

public class Configurable {
  public static IConfigurationRoot Make() {
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Directory.GetCurrentDirectory())
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{env}.json", optional: true)
      .Build();
    return configuration;
  }
}
