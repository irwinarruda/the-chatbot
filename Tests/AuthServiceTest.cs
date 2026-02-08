using System.Web;

using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;

using Shouldly;

using TheChatbot.Entities;
using TheChatbot.Resources;
using TheChatbot.Services;

namespace Tests;

public class AuthServiceTest : IClassFixture<Orquestrator> {
  private readonly Orquestrator orquestrator;
  private readonly AuthService authService;
  public AuthServiceTest(Orquestrator _orquestrator) {
    orquestrator = _orquestrator;
    authService = _orquestrator.authService;
  }

  [Fact]
  public void GetGoogleLoginUrl() {
    var phoneNumber = "5511984444444";
    var url = authService.GetGoogleLoginUrl(phoneNumber);
    var uri = new Uri(url);
    var queryParams = HttpUtility.ParseQueryString(uri.Query);
    var redirectUri = queryParams.Get("redirect_uri");
    redirectUri.ShouldBe(orquestrator.googleConfig.RedirectUri);
    var scope = queryParams.Get("scope");
    scope.ShouldNotBeNull();
    scope.ShouldContain(SheetsService.Scope.Spreadsheets);
    scope.ShouldContain(GoogleAuthScopes.Tasks);
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
  public void GetThankYouPageHtmlString() {
    var template = authService.GetThankYouPageHtmlString();
    template.ShouldContain("<html ");
    template.ShouldContain("</html>");
  }

  [Fact]
  public void GetAlreadySignedInPageHtmlString() {
    var template = authService.GetAlreadySignedInPageHtmlString();
    template.ShouldContain("<html");
    template.ShouldContain("</html>");
  }

  [Fact]
  public async Task CreateUser() {
    await orquestrator.ClearDatabase();
    var phoneNumber = "5511984444444";
    var user = new User("Irwin Arruda", phoneNumber);
    await authService.CreateUser(user);
    user.Name.ShouldBe("Irwin Arruda");
    user.PhoneNumber.ShouldBe(phoneNumber);
    user.GoogleCredential.ShouldBeNull();
    user.IsInactive.ShouldBeFalse();
    user.CreatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
    user.UpdatedAt.ToString("yyyy-MM-dd").ShouldBe(DateTime.UtcNow.ToString("yyyy-MM-dd"));
  }

  [Fact]
  public async Task GetUsers() {
    await orquestrator.ClearDatabase();
    var user = await orquestrator.CreateUser();
    var users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].Id.ShouldBe(user.Id);
    users[0].Name.ShouldBe(user.Name);
    users[0].PhoneNumber.ShouldBe(user.PhoneNumber);
    users[0].IsInactive.ShouldBe(user.IsInactive);
    users[0].GoogleCredential.ShouldBeNull();
    users[0].CreatedAt.ShouldBe(user.CreatedAt);
    users[0].UpdatedAt.ShouldBe(user.UpdatedAt);
  }

  [Fact]
  public async Task SaveUserByGoogleCredential() {
    await orquestrator.ClearDatabase();
    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );
    var phoneNumber = "5511984444444";
    var wrongCode = "wrongCode";
    await Should.ThrowAsync<Exception>(() => authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber), wrongCode));
    var rightCode = "rightCode";
    await authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber), rightCode);
    var users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].Name.ShouldBe("Save Google Credentials User");
    users[0].PhoneNumber.ShouldBe("5511984444444");
    users[0].GoogleCredential.ShouldNotBeNull();
    users[0].GoogleCredential?.AccessToken.ShouldBe("ya29.a0ARrdaM9test_access_token_123456789");
    users[0].GoogleCredential?.RefreshToken.ShouldBe("1//0G_refresh_token_test_abcdefghijklmnopqrstuvwxyz");
    var createdAt = users[0].GoogleCredential?.CreatedAt!;
    var updatedAt = users[0].GoogleCredential?.UpdatedAt!;
    ((DateTimeOffset)createdAt).ToUnixTimeMilliseconds().ShouldBe(((DateTimeOffset)updatedAt).ToUnixTimeMilliseconds());
    await Should.NotThrowAsync(() => authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber), rightCode));
  }

  [Fact]
  public async Task RefreshGoogleCredential() {
    await orquestrator.ClearDatabase();
    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );
    await authService.SaveUserByGoogleCredential(encryption.Encrypt("5511984444444"), "rightCode");
    var users = await authService.GetUsers();
    users[0].GoogleCredential?.AccessToken.ShouldBe("ya29.a0ARrdaM9test_access_token_123456789");
    users[0].GoogleCredential?.RefreshToken.ShouldBe("1//0G_refresh_token_test_abcdefghijklmnopqrstuvwxyz");
    var refreshedUser = users[0];
    await authService.RefreshGoogleCredential(refreshedUser);
    refreshedUser.ShouldNotBeNull();
    refreshedUser.GoogleCredential?.AccessToken.ShouldBe("ya29.a0ARrdaM9refreshed_access_token_123456789");
    refreshedUser.GoogleCredential?.RefreshToken.ShouldBe("1//0G_refresh_token_refreshed_abcdefghijklmnopqrstuvwxyz");
    var createdAt = refreshedUser.GoogleCredential?.CreatedAt!;
    var updatedAt = refreshedUser.GoogleCredential?.UpdatedAt!;
    ((DateTimeOffset)createdAt).ToUnixTimeMilliseconds().ShouldNotBe(((DateTimeOffset)updatedAt).ToUnixTimeMilliseconds());
  }

  [Fact]
  public async Task HandleGoogleLogin() {
    await orquestrator.ClearDatabase();

    var phoneNumber1 = "5511984444444";
    var result = await authService.HandleGoogleLogin(phoneNumber1);
    result.IsRedirect.ShouldBeTrue();
    result.Content.ShouldContain("accounts.google.com");

    var user = await orquestrator.CreateUser();
    result = await authService.HandleGoogleLogin(user.PhoneNumber);
    result.IsRedirect.ShouldBeTrue();
    result.Content.ShouldContain("accounts.google.com");

    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );
    var phoneNumber3 = "5511999888777";
    await authService.SaveUserByGoogleCredential(encryption.Encrypt(phoneNumber3), "rightCode");
    result = await authService.HandleGoogleLogin(phoneNumber3);
    result.IsRedirect.ShouldBeFalse();
    result.Content.ShouldContain("<html");
    result.Content.ShouldContain("</html>");
  }

  [Fact]
  public async Task HandleGoogleRedirect() {
    await orquestrator.ClearDatabase();
    var encryption = new Encryption(
      orquestrator.encryptionConfig.Text32Bytes,
      orquestrator.encryptionConfig.Text16Bytes
    );

    var phoneNumber1 = "5511984444444";
    var state = encryption.Encrypt(phoneNumber1);
    var result = await authService.HandleGoogleRedirect(state, "rightCode");
    result.ShouldContain("<html");
    result.ShouldContain("</html>");
    var users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].GoogleCredential.ShouldNotBeNull();

    var phoneNumber2 = "5511987654321";
    var state2 = encryption.Encrypt(phoneNumber2);
    await Should.ThrowAsync<Exception>(() => authService.HandleGoogleRedirect(state2, "wrongCode"));

    result = await authService.HandleGoogleRedirect(state, "rightCode");
    result.ShouldContain("<html");
    result.ShouldContain("</html>");
    users = await authService.GetUsers();
    users.Count.ShouldBe(1);
    users[0].GoogleCredential.ShouldNotBeNull();
  }
}
