using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class GoogleAuthGateway : IGoogleAuthGateway {
  private readonly GoogleConfig googleConfig;
  private readonly GoogleAuthorizationCodeFlow flow;
  public GoogleAuthGateway(GoogleConfig _googleConfig) {
    googleConfig = _googleConfig;
    var scopes = new string[] {
      SheetsService.Scope.Spreadsheets,
      TasksService.Scope.Tasks,
      Oauth2Service.Scope.Openid,
      Oauth2Service.Scope.UserinfoEmail,
      Oauth2Service.Scope.UserinfoProfile,
    };
    var clientSecrets = new ClientSecrets {
      ClientId = googleConfig.ClientId,
      ClientSecret = googleConfig.SecretClientKey
    };
    flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
      ClientSecrets = clientSecrets,
      Scopes = scopes,
    });
  }
  public string CreateAuthorizationCodeUrl(string? state = null) {
    var request = flow.CreateAuthorizationCodeRequest(googleConfig.RedirectUri);
    if (state != null) {
      request.State = state;
    }
    var uri = request.Build();
    return uri.ToString();
  }

  public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code) {
    var tokenResponse = await flow.ExchangeCodeForTokenAsync("user", code, googleConfig.RedirectUri, CancellationToken.None);
    return tokenResponse;
  }

  public async Task<Userinfo> GetUserinfo(TokenResponse userToken) {
    var credential = GoogleCredential.FromAccessToken(userToken.AccessToken);
    var oauth2Service = new Oauth2Service(new BaseClientService.Initializer {
      HttpClientInitializer = credential,
      ApplicationName = "TheChatbot"
    });
    var userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();
    return userInfo;
  }

  public async Task<TokenResponse> RefreshToken(string accessToken, string refreshToken) {
    var tokenResponse = await flow.RefreshTokenAsync(accessToken, refreshToken, CancellationToken.None);
    return tokenResponse;
  }
}
