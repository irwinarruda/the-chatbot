using System.Text;

namespace TheChatbot.Entities.Extensions;

public static class HttpRequestExtensions {
  public static async Task<string> GetRawBodyAsync(this HttpRequest request) {
    request.EnableBuffering();
    request.Body.Position = 0;
    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
    var body = await reader.ReadToEndAsync();
    request.Body.Position = 0;
    return body;
  }
}

