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
  public static T? Convert<T>(string json) where T : class {
    var settings = new JsonSerializerSettings {
      ContractResolver = new DefaultContractResolver {
        NamingStrategy = new SnakeCaseNamingStrategy {
          ProcessDictionaryKeys = true,
          OverrideSpecifiedNames = false
        }
      },
      MissingMemberHandling = MissingMemberHandling.Ignore
    };
    try {
      return JsonConvert.DeserializeObject<T>(json, settings);
    } catch {
      return null;
    }
  }
}
