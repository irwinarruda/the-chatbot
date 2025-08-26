namespace TheChatbot.Utils;

public class Env {
  public static string Key => "ASPNETCORE_ENVIRONMENT";
  public static string Value => Environment.GetEnvironmentVariable(Key) ?? "Development";
}

