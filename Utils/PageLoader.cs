using System.Text.RegularExpressions;

using TheChatbot.Infra;

namespace TheChatbot.Utils;

public enum PageTemplate { ThankYou, AlreadySignedIn, Welcome, PrivacyPolicy }
public enum PageLocale { En, PtBr }

public record PageParams(string? UserName = null);

public static class PageLoader {
  private static readonly string PagesRoot = Path.Combine(AppContext.BaseDirectory, "Templates", "Pages");
  private static readonly object CacheLock = new();
  private static readonly Dictionary<string, string> Cache = [];

  private static string ReadFile(string fileName) {
    var filePath = Path.Combine(PagesRoot, fileName);
    if (!File.Exists(filePath))
      throw new NotFoundException($"Page template file not found: {filePath}");
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

  private static string TemplateToBaseName(PageTemplate template) => template switch {
    PageTemplate.ThankYou => "ThankYouPage",
    PageTemplate.AlreadySignedIn => "AlreadySignedIn",
    PageTemplate.Welcome => "Welcome",
    PageTemplate.PrivacyPolicy => "PrivacyPolicy",
    _ => throw new ArgumentException($"Unknown page template: {template}")
  };

  private static string LocaleToFileSuffix(PageLocale locale) => locale switch {
    PageLocale.En => ".en.html",
    PageLocale.PtBr => ".pt-BR.html",
    _ => ".en.html"
  };

  public static string GetPage(PageTemplate template, PageParams? data = null, PageLocale locale = PageLocale.PtBr) {
    var baseName = TemplateToBaseName(template);
    var fileName = baseName + LocaleToFileSuffix(locale);
    string text;
    try {
      text = ReadFile(fileName);
    } catch (NotFoundException) {
      text = ReadFile(baseName + LocaleToFileSuffix(PageLocale.PtBr));
    }
    if (data == null) return text;
    var dict = new Dictionary<string, string>();
    if (data.UserName != null) dict["UserName"] = data.UserName;
    return ApplyTemplate(text, dict);
  }
}
