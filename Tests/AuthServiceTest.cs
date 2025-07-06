using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using Shouldly;

using TheChatbot.Services;

namespace Tests;

public class AuthServiceTest : IClassFixture<Orquestrator> {
  public Orquestrator orquestrator;
  public AuthService authService;
  public ITestOutputHelper output;
  public AuthServiceTest(Orquestrator _orquestrator, ITestOutputHelper _output) {
    orquestrator = _orquestrator;
    authService = _orquestrator.authService;
    output = _output;
  }

  [Fact]
  public void GetGoogleLoginUrl() {
    var uri = authService.GetGoogleLoginUrl();
    uri = Uri.UnescapeDataString(uri);
    uri.ShouldContain(orquestrator.googleConfig.RedirectUri);
    uri.ShouldContain(orquestrator.googleConfig.ClientId);
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
    await orquestrator.ClearDatabase();
    await orquestrator.RunPendingMigrations();
    var user = await authService.CreateUser("Irwin Arruda", "+5511984444444");
    user.Name.ShouldBe("Irwin Arruda");
    user.PhoneNumber.ShouldBe("+5511984444444");
    user.IsInactive.ShouldBeFalse();
    user.CreatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
    user.UpdatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
  }

  [Fact]
  public async Task GetUsers() {
    await orquestrator.ClearDatabase();
    await orquestrator.RunPendingMigrations();
    var user = await orquestrator.CreateUser();
    var users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].Id.ShouldBe(user.Id);
    users[0].Name.ShouldBe(user.Name);
    users[0].PhoneNumber.ShouldBe(user.PhoneNumber);
    users[0].IsInactive.ShouldBe(user.IsInactive);
    users[0].CreatedAt.ShouldBe(user.CreatedAt);
    users[0].UpdatedAt.ShouldBe(user.UpdatedAt);
  }
}
