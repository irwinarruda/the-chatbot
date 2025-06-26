using System.Runtime.Serialization;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TheChatbot.Utils;

public class Printable {
  public static string Make(object? obj) {
    var settings = new JsonSerializerSettings {
      Formatting = Formatting.Indented,
      ContractResolver = new DefaultContractResolver {
        NamingStrategy = new SnakeCaseNamingStrategy(),
      }
    };
    return JsonConvert.SerializeObject(obj, settings);
  }
}
