using Google.Apis.Oauth2.v2;
using Google.Apis.Sheets.v4;

namespace TheChatbot.Resources;

public static class GoogleAuthScopes {
  public const string Tasks = "https://www.googleapis.com/auth/tasks";

  public static string[] Build() {
    return [
      SheetsService.Scope.Spreadsheets,
      Tasks,
      Oauth2Service.Scope.Openid,
      Oauth2Service.Scope.UserinfoEmail,
      Oauth2Service.Scope.UserinfoProfile,
    ];
  }
}
