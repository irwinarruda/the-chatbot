using System.Web;

using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;
using Google.Apis.Tasks.v1;

using Shouldly;

using TheChatbot.Entities;
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
    var phoneNumber = "+5511984444444";
    var url = authService.GetGoogleLoginUrl(phoneNumber);
    var uri = new Uri(url);
    var queryParams = HttpUtility.ParseQueryString(uri.Query);
    var redirectUri = queryParams.Get("redirect_uri");
    redirectUri.ShouldBe(orquestrator.googleConfig.RedirectUri);
    var scope = queryParams.Get("scope");
    scope.ShouldNotBeNull();
    scope.ShouldContain(SheetsService.Scope.Spreadsheets);
    scope.ShouldContain(TasksService.Scope.Tasks);
    scope.ShouldContain(Oauth2Service.Scope.UserinfoEmail);
    scope.ShouldContain(Oauth2Service.Scope.UserinfoProfile);
    scope.ShouldContain(Oauth2Service.Scope.Openid);
    var clientId = queryParams.Get("client_id");
    clientId.ShouldBe(orquestrator.googleConfig.ClientId);
    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );
    var state = queryParams.Get("state");
    state.ShouldNotBeNull();
    encryption.Decrypt(state).ShouldBe(phoneNumber);
  }

  [Fact]
  public async Task CreateUser() {
    await orquestrator.ClearDatabase();
    await orquestrator.RunPendingMigrations();
    var phoneNumber = "+5511984444444";
    var user = await authService.CreateUser("Irwin Arruda", phoneNumber);
    user.Name.ShouldBe("Irwin Arruda");
    user.PhoneNumber.ShouldBe(phoneNumber);
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

  [Fact]
  public async Task SaveGoogleCredentials() {
    await orquestrator.ClearDatabase();
    await orquestrator.RunPendingMigrations();
    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );
    var phoneNumber = "+5511984444444";
    var wrongCode = "wrongCode";
    await Should.ThrowAsync<Exception>(() => authService.SaveGoogleCredentials(encryption.Encrypt(phoneNumber), wrongCode));
  }
}
