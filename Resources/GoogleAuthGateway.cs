
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class GoogleAuthGateway : IGoogleAuthGateway {
  public GoogleConfig googleConfig;
  public GoogleAuthorizationCodeFlow flow;
  public GoogleAuthGateway(GoogleConfig _googleConfig) {
    googleConfig = _googleConfig;
    var scopes = new string[] {
      SheetsService.Scope.Spreadsheets,
      TasksService.Scope.Tasks,
      Oauth2Service.Scope.Openid,
      Oauth2Service.Scope.UserinfoEmail,
      Oauth2Service.Scope.UserinfoProfile,
    }; var clientSecrets = new ClientSecrets {
      ClientId = googleConfig.ClientId,
      ClientSecret = googleConfig.SecretClientKey
    };
    flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
      ClientSecrets = clientSecrets,
      Scopes = scopes,
    });
  }
  public string CreateAuthorizationCodeUrl() {
    var uri = flow.CreateAuthorizationCodeRequest(googleConfig.RedirectUri).Build();
    return uri.ToString();
  }

  public Task<GoogleTokenResponse> ExchangeCodeForTokenAsync() {
    throw new NotImplementedException();
  }

  public Task<GoogleTokenResponse> RefreshToken(GoogleTokenResponse response) {
    throw new NotImplementedException();
  }
}
