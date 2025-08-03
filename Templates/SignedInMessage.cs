namespace TheChatbot.Templates;

public static class SignedInMessage {
  public static string Get() {
    return $@"
🔑 You have logged in with Google.
✅ You can now use *The Chatbot*!
".Trim();
  }
}
