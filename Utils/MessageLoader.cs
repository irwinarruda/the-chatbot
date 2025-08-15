using System.Text.RegularExpressions;

using TheChatbot.Infra;

namespace TheChatbot.Utils;

public enum MessageTemplate { SignedIn, ThankYou }
public enum MessageLocale { En, PtBr }

public record MessageParams(string? LoginUrl = null);

public static class MessageLoader {
  private static readonly string MessagesRoot = Path.Combine(AppContext.BaseDirectory, "Templates", "Messages");
  private static readonly object CacheLock = new();
  private static readonly Dictionary<string, string> Cache = [];

  private static string ReadFile(string fileName) {
    var filePath = Path.Combine(MessagesRoot, fileName);
    if (!File.Exists(filePath))
      throw new NotFoundException($"Message template file not found: {filePath}");
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

  private static string TemplateToBaseName(MessageTemplate template) => template switch {
    MessageTemplate.SignedIn => "SignedInMessage",
    MessageTemplate.ThankYou => "ThankYouMessage",
    _ => throw new ArgumentException($"Unknown message template: {template}")
  };

  private static string LocaleToFileSuffix(MessageLocale locale) => locale switch {
    MessageLocale.En => ".en.txt",
    MessageLocale.PtBr => ".pt-BR.txt",
    _ => ".en.txt"
  };

  public static string GetMessage(MessageTemplate template, MessageParams? data = null, MessageLocale locale = MessageLocale.PtBr) {
    var baseName = TemplateToBaseName(template);
    var fileName = baseName + LocaleToFileSuffix(locale);
    string text;
    try {
      text = ReadFile(fileName);
    } catch (NotFoundException) {
      // Fallback to English
      text = ReadFile(baseName + LocaleToFileSuffix(MessageLocale.PtBr));
    }
    if (data == null) return text;
    var dict = new Dictionary<string, string>();
    if (data.LoginUrl != null) dict["LoginUrl"] = data.LoginUrl;
    return ApplyTemplate(text, dict);
  }
}
