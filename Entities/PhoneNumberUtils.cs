using System.Text.RegularExpressions;

namespace TheChatbot.Entities;

public static class PhoneNumberUtils {
  public static string AddDigitNine(string number) {
    number = Sanitize(number);
    var ddiAndDdd = number.Length >= 4 ? number[..4] : number;
    if (!ddiAndDdd.StartsWith("55")) {
      return number;
    }
    if (number.Length == 13) {
      return number;
    }
    return string.Concat(ddiAndDdd, "9", number.AsSpan(4));
  }
  public static string Sanitize(string number) {
    return Regex.Replace(number, @"\D", "");
  }
  public static bool IsValid(string number) {
    return number.Length >= 8 && number.Length <= 15;
  }
}
