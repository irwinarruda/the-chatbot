using Microsoft.AspNetCore.Mvc;
using TheChatbot.Resources;
using Google.Apis.Tasks.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;
using TheChatbot.Infra;
using Google.Apis.Oauth2.v2;
using Google.Apis.Auth.OAuth2.Flows;
using TheChatbot.Utils;
using Google.Apis.Auth.OAuth2.Responses;
using Google;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase {
  readonly GoogleConfig googleConfig;
  readonly GoogleSheetsConfig googleSheetsConfig;
  static TokenResponse? userToken;

  public GoogleController(IConfiguration configuration) {
    googleConfig = configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    Console.WriteLine(Printable.Make(userToken));
  }

  [HttpGet("redirect")]
  public async Task<ActionResult> GetLogin([FromQuery] string code) {
    var scopes = new[] { SheetsService.Scope.Spreadsheets, TasksService.Scope.Tasks, Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.Openid };
    var clientSecrets = new ClientSecrets {
      ClientId = googleConfig.ClientId,
      ClientSecret = googleConfig.SecretClientKey
    };
    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
      ClientSecrets = clientSecrets,
      Scopes = scopes,
    });
    userToken = await flow.ExchangeCodeForTokenAsync("user", code, googleConfig.RedirectUri, CancellationToken.None);
    return Ok(userToken);
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect() {
    var scopes = new[] { SheetsService.Scope.Spreadsheets, TasksService.Scope.Tasks, Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.Openid };
    var clientSecrets = new ClientSecrets {
      ClientId = googleConfig.ClientId,
      ClientSecret = googleConfig.SecretClientKey
    };
    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
      ClientSecrets = clientSecrets,
      Scopes = scopes,
    });
    var request = flow.CreateAuthorizationCodeRequest(googleConfig.RedirectUri);
    request.State = "";
    var uri = request.Build();
    return Redirect(uri.ToString());
  }

  [HttpGet("refresh")]
  public async Task<ActionResult> GetRefresh() {
    var scopes = new[] { SheetsService.Scope.Spreadsheets, TasksService.Scope.Tasks, Oauth2Service.Scope.UserinfoEmail, Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.Openid };
    var clientSecrets = new ClientSecrets {
      ClientId = googleConfig.ClientId,
      ClientSecret = googleConfig.SecretClientKey
    };
    var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
      ClientSecrets = clientSecrets,
      Scopes = scopes,
    });
    userToken = await flow.RefreshTokenAsync("user", userToken!.RefreshToken, CancellationToken.None);
    return Ok(userToken);
  }

  [HttpGet("tasks")]
  public async Task<ActionResult> GetTaskList() {
    try {
      var credential = GoogleCredential.FromAccessToken(userToken?.AccessToken ?? "teste");
      var taskService = new TasksService(new BaseClientService.Initializer() {
        HttpClientInitializer = credential,
        ApplicationName = "TheChatbot"
      });
      var taskLists = await taskService.Tasklists.List().ExecuteAsync();
      var taskListId = taskLists.Items[1].Id;
      var tasks = await taskService.Tasks.List(taskListId).ExecuteAsync();
      return Ok(tasks);
    } catch (Exception ex) {
      if (ex is GoogleApiException googleEx) {
        return StatusCode((int)googleEx.HttpStatusCode, googleEx.Error);
      }
      return StatusCode(500, $"Error accessing Google Tasks: {ex.Message}");
    }
  }

  [HttpGet("tasks/{id}")]
  public async Task<ActionResult> GetTask([FromRoute] string id) {
    try {
      if (userToken == null) {
        return Ok("User not authenticated");
      }
      var credential = GoogleCredential.FromAccessToken(userToken.AccessToken);
      var taskService = new TasksService(new BaseClientService.Initializer {
        HttpClientInitializer = credential,
        ApplicationName = "TheChatbot"
      });
      var taskLists = await taskService.Tasklists.List().ExecuteAsync();
      var taskListId = taskLists.Items[1].Id;
      var task = await taskService.Tasks.Get(taskListId, id).ExecuteAsync();
      return Ok(task);
    } catch (Exception ex) {
      return StatusCode(500, $"Error accessing Google Tasks: {ex.Message}");
    }
  }

  [HttpGet("sheet")]
  public async Task<ActionResult> GetSpreadSheet() {
    try {
      if (userToken == null) {
        return Ok("User not authenticated");
      }
      var credential = GoogleCredential.FromAccessToken(userToken.AccessToken);
      var sheetsService = new SheetsService(new BaseClientService.Initializer {
        HttpClientInitializer = credential,
        ApplicationName = "TheChatbot",
      });
      var query = "Diário!A:G";
      var id = googleSheetsConfig.TestSheetId;
      var tsheet = await sheetsService.Spreadsheets.Values.Get(id, query).ExecuteAsync();
      var count = tsheet.Values.Count + 1;
      query = $"Diário!B{count}";
      var valueRange = new ValueRange {
        Values = [["18/06/2025"]],
      };
      var request = sheetsService.Spreadsheets.Values.Update(valueRange, id, query);
      request.ValueInputOption = ValueInputOptionEnum.USERENTERED;
      await request.ExecuteAsync();
      query = $"Diário!D{count}:G{count}";
      valueRange = new ValueRange {
        Values = [["-5,42", "Outros Alimentação", "Transferência Cassiel", "NuConta"]],
      };
      request = sheetsService.Spreadsheets.Values.Update(valueRange, id, query);
      request.ValueInputOption = ValueInputOptionEnum.USERENTERED;
      await request.ExecuteAsync();
      return Ok(new { tsheet.Values.Count });
    } catch (Exception ex) {
      return Ok(ex.Message);
    }
  }
}

