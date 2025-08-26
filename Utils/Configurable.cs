namespace TheChatbot.Utils;

public class Configurable {
  public static IConfigurationRoot Make() {
    var configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{Env.Value}.json", optional: true)
      .Build();
    return configuration;
  }

  public static void Enhance(ConfigurationManager configuration) {
    configuration.SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile($"appsettings.{Env.Value}.json", optional: true)
      .Build();
  }
}
