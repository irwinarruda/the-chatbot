using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2.Data;

namespace TheChatbot.Resources;

public interface IGoogleAuthGateway {
  string CreateAuthorizationCodeUrl(string? state = null);
  Task<Userinfo> GetUserinfo(TokenResponse userToken);
  Task<TokenResponse> ExchangeCodeForTokenAsync(string code);
  Task<TokenResponse> RefreshToken(string accessToken, string refreshToken);
}
