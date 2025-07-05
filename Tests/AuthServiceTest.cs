using System.Threading.Tasks;

using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using Microsoft.AspNetCore.Mvc.Routing;

using Shouldly;

using TheChatbot.Infra;
using TheChatbot.Services;

namespace Tests;

public class AuthServiceTest : IClassFixture<CustomWebApplicationFactory> {
  public CustomWebApplicationFactory factory;
  public AuthService authService;
  public GoogleConfig googleConfig;
  public AuthServiceTest(CustomWebApplicationFactory _factory) {
    factory = _factory;
    authService = _factory.authService;
    googleConfig = _factory.googleConfig;
  }

  [Fact]
  public void GetGoogleLoginUrl() {
    var uri = authService.GetGoogleLoginUrl();
    uri = Uri.UnescapeDataString(uri);
    uri.ShouldContain(googleConfig.RedirectUri);
    uri.ShouldContain(googleConfig.ClientId);
    uri.ShouldContain(SheetsService.Scope.Spreadsheets);
    uri.ShouldContain(TasksService.Scope.Tasks);
    uri.ShouldContain(Oauth2Service.Scope.UserinfoEmail);
    uri.ShouldContain(Oauth2Service.Scope.UserinfoProfile);
    uri.ShouldContain(Oauth2Service.Scope.Openid);
    uri.ShouldContain("accounts.google.com");
    uri.ShouldContain("https://");
  }
}
