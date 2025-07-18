using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class TestGoogleAuthGateway : IGoogleAuthGateway {
  private readonly GoogleConfig googleConfig;
  private readonly GoogleAuthorizationCodeFlow flow;
  public TestGoogleAuthGateway(GoogleConfig _googleConfig) {
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

  public Task<TokenResponse> ExchangeCodeForTokenAsync(string code) {
    if (code != "rightCode") {
      throw new DeveloperException("Wrong code testing variable!");
    }
    var scopes = new string[] {
      SheetsService.Scope.Spreadsheets,
      TasksService.Scope.Tasks,
      Oauth2Service.Scope.Openid,
      Oauth2Service.Scope.UserinfoEmail,
      Oauth2Service.Scope.UserinfoProfile,
    };
    var tokenResponse = new TokenResponse {
      AccessToken = "ya29.a0ARrdaM9test_access_token_123456789",
      TokenType = "Bearer",
      ExpiresInSeconds = 3600, // 1 hour
      RefreshToken = "1//0G_refresh_token_test_abcdefghijklmnopqrstuvwxyz",
      Scope = string.Join(" ", scopes),
      IdToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.test_id_token_payload.test_signature",
      IssuedUtc = DateTime.UtcNow
    };
    return Task.FromResult(tokenResponse);
  }

  public Task<Userinfo> GetUserinfo(TokenResponse tokenResponse) {
    var userinfo = new Userinfo {
      Email = "savegooglecredentials@example.com",
      Name = "Save Google Credentials User",
      GivenName = "Test",
      FamilyName = "User",
      Picture = "https://example.com/avatar.jpg",
      Locale = "en-US",
      VerifiedEmail = true,
      Id = "123456789012345678901",
      Link = "https://plus.google.com/123456789012345678901",
      Gender = "male",
      Hd = "example.com"
    };
    return Task.FromResult(userinfo);
  }

  public Task<TokenResponse> RefreshToken(string accessToken, string refreshToken) {
    throw new NotImplementedException();
  }
}
