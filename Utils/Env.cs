namespace TheChatbot.Utils;

public class Env {
  public static string Key() {
    return "ASPNETCORE_ENVIRONMENT";
  }
  public static string Value() {
    return Environment.GetEnvironmentVariable(Key()) ?? "Development";
  }
}

