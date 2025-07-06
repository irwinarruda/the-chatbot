using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using Shouldly;

using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class AuthServiceTest : IClassFixture<CustomWebApplicationFactory> {
  public CustomWebApplicationFactory factory;
  public AuthService authService;
  public AuthServiceTest(CustomWebApplicationFactory _factory) {
    factory = _factory;
    authService = _factory.authService;
  }

  [Fact]
  public void GetGoogleLoginUrl() {
    var uri = authService.GetGoogleLoginUrl();
    uri = Uri.UnescapeDataString(uri);
    uri.ShouldContain(factory.googleConfig.RedirectUri);
    uri.ShouldContain(factory.googleConfig.ClientId);
    uri.ShouldContain(SheetsService.Scope.Spreadsheets);
    uri.ShouldContain(TasksService.Scope.Tasks);
    uri.ShouldContain(Oauth2Service.Scope.UserinfoEmail);
    uri.ShouldContain(Oauth2Service.Scope.UserinfoProfile);
    uri.ShouldContain(Oauth2Service.Scope.Openid);
    uri.ShouldContain("accounts.google.com");
    uri.ShouldContain("https://");
  }

  [Fact]
  public async Task CreateUser() {
    await factory.ClearDatabase();
    await factory.RunPendingMigrations();
    var user = await authService.CreateUser("Irwin Arruda", "+5511984444444");
    user.Name.ShouldBe("Irwin Arruda");
    user.PhoneNumber.ShouldBe("+5511984444444");
    user.IsInactive.ShouldBeFalse();
    user.CreatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
    user.UpdatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
  }

  [Fact]
  public async Task GetUsers() {
    await factory.ClearDatabase();
    await factory.RunPendingMigrations();
    var user = await factory.CreateUser();
    var users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].Name.ShouldBe(user.Name);
    users[0].Id.ShouldBe(user.Id);
  }
}
