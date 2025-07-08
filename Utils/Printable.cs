using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TheChatbot.Utils;

public class Printable {
  public static string Make(object? obj) {
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
    var settings = new JsonSerializerSettings {
      Formatting = env == "Production" ? Formatting.None : Formatting.Indented,
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
    return JsonConvert.DeserializeObject<T>(json, settings);
  }
}
