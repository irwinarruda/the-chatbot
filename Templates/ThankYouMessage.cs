namespace TheChatbot.Templates;

public static class ThankYouMessage {
  public static string Get(string loginUrl) {
    return $@"
✅ Thank you for using *The Chatbot*
Please *🔒 login with Google* before using the chat.

{loginUrl}
".Trim();
  }
}
