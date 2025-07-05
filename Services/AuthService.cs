using TheChatbot.Resources;

namespace TheChatbot.Services;

public class AuthService(IGoogleAuthGateway googleAuthGateway) {
  public string GetGoogleLoginUrl() {
    return googleAuthGateway.CreateAuthorizationCodeUrl();
  }
  public async Task<object?> SaveGoogleCredentials(string userId) {
    return await Task.FromResult("");
  }
  public async Task<string> GetThankYouPageHtmlString() {
    return await Task.FromResult("");
  }
}
