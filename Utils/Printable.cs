using System.Text.Json;

namespace TheChatbot.Utils;

public class Printable {
  static JsonSerializerOptions Options(bool indented = false) {
    return new JsonSerializerOptions {
      PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
      DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
      WriteIndented = indented,
      IndentSize = 2
    };
  }
  public static string Make(object? obj) {
    if (obj == null) return string.Empty;
    var env = Env.Value();
    return JsonSerializer.Serialize(obj, Options(env != "Production"));
  }
  public static T? Convert<T>(string json) where T : class {
    if (string.IsNullOrWhiteSpace(json)) return default;
    return JsonSerializer.Deserialize<T>(json, Options());
  }
}
