namespace TheChatbot.Entities;

public static class DatePrecision {
  public static DateTime SixDigitPrecisionUtcNow => TruncateToMicroseconds(DateTime.UtcNow);

  public static DateTime TruncateToMicroseconds(this DateTime dateTime) {
    return new DateTime(dateTime.Ticks / 10 * 10, dateTime.Kind);
  }

  public static DateTime TruncateToMicrosecondsUtc(this DateTime dateTime) {
    var truncated = new DateTime(dateTime.Ticks / 10 * 10, DateTimeKind.Utc);
    return truncated;
  }
}
