using System.Text.RegularExpressions;

using TheChatbot.Infra;

namespace TheChatbot.Utils;

public enum PromptLocale { En, PtBr }

public record AiChatGatewayParams(string PhoneNumber);

public static class PromptLoader {
  private static readonly string PromptsRoot = Path.Combine(AppContext.BaseDirectory, "Templates", "Prompts");
  private static readonly object CacheLock = new();
  private static readonly Dictionary<string, string> Cache = [];

  private static string ReadFile(string fileName) {
    var filePath = Path.Combine(PromptsRoot, fileName);
    if (!File.Exists(filePath))
      throw new NotFoundException($"Prompt file not found: {filePath}");
    var key = filePath;
    lock (CacheLock) {
      if (Cache.TryGetValue(key, out var cached)) return cached;
    }
    var text = File.ReadAllText(filePath);
    lock (CacheLock) { Cache[key] = text; }
    return text;
  }

  private static string ApplyTemplate(string text, IReadOnlyDictionary<string, string> data) {
    if (data.Count == 0) return text;
    return Regex.Replace(text, @"\{\{(?<k>[A-Za-z0-9_]+)\}\}", m => {
      var k = m.Groups["k"].Value;
      return data.TryGetValue(k, out var val) ? val : m.Value;
    });
  }

  private static string LocaleToFileSuffix(PromptLocale locale) => locale switch {
    PromptLocale.En => ".en.md",
    PromptLocale.PtBr => ".pt-BR.md",
    _ => ".en.md"
  };

  public static string GetAiChatGateway(PromptLocale locale, AiChatGatewayParams data) {
    var fileBase = "AiChatGateway" + LocaleToFileSuffix(locale);
    var text = ReadFile(fileBase);
    var dict = new Dictionary<string, string> {
      ["PhoneNumber"] = data.PhoneNumber
    };
    return ApplyTemplate(text, dict);
  }

  public static string GetTransactionClassification(PromptLocale locale) {
    var fileBase = "TransactionClassification" + LocaleToFileSuffix(locale);
    return ReadFile(fileBase);
  }
}
