using Newtonsoft.Json;

namespace TheChatbot.Utils;

public class Printable {
  public static string Make(object? obj) {
    return JsonConvert.SerializeObject(obj, Formatting.Indented);
  }
}
