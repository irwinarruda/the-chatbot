using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheChatbot.Dtos;
using TheChatbot.Resources;
using Google.Apis.Tasks.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase {
  readonly GoogleOAuthConfig googleConfig;
  readonly GoogleSheetsConfig googleSheetsConfig;
  readonly GoogleTokenResponse? userToken;
  readonly IMemoryCache cache;
  readonly IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;

  public GoogleController(IFinantialPlanningSpreadsheet _finantialPlanningSpreadsheet, IConfiguration configuration, IMemoryCache _cache) {
    finantialPlanningSpreadsheet = _finantialPlanningSpreadsheet;
    googleConfig = configuration.GetSection("GoogleOAuthConfig").Get<GoogleOAuthConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    cache = _cache;
    _cache.Set("UserGoogleToken", new GoogleTokenResponse {
      AccessToken = "",
      RefreshToken = null,
      TokenType = "Bearer",
      ExpiresIn = 3598,
      IdToken = "",
      Scope = "openid https://www.googleapis.com/auth/tasks https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/spreadsheets https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/drive"
    });
    _cache.TryGetValue("UserGoogleToken", out userToken);
  }

  [HttpGet("redirect")]
  public async Task<ActionResult> GetLogin([FromQuery] string code) {
    var tokenRequest = new Dictionary<string, string> {
      { "code", code },
      { "client_id", googleConfig.ClientId },
      { "client_secret", googleConfig.SecretClientKey },
      { "redirect_uri", googleConfig.RedirectUri },
      { "grant_type", "authorization_code" }
    };
    using var client = new HttpClient();
    var tokenResponse = await client.PostAsync(googleConfig.TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
    if (!tokenResponse.IsSuccessStatusCode) {
      return StatusCode((int)tokenResponse.StatusCode, "Failed to retrieve tokens.");
    }
    var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
    var cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(30));
    var token = JsonConvert.DeserializeObject<GoogleTokenResponse>(tokenResponseContent);
    cache.Set("UserGoogleToken", token, cacheOptions);
    var acconuts = await finantialPlanningSpreadsheet.GetBankAccount(new SheetConfigDTO {
      SheetId = googleSheetsConfig.MainId,
      SheetAccessToken = token!.AccessToken!,
    });
    return Ok(acconuts);
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect() {
    var scopes = new List<string> { TasksService.Scope.Tasks, SheetsService.Scope.Spreadsheets, SheetsService.Scope.Drive, "openid email profile" };
    var queryParams = new Dictionary<string, string> {
      { "client_id", googleConfig.ClientId },
      { "response_type", "code" },
      { "scope", string.Join(" ", scopes)},
      { "redirect_uri", googleConfig.RedirectUri },
      { "access_type", "offline" }  // RefreshToken
    };
    var url = $"{googleConfig.AuthorizationEndpoint}?{string.Join("&", queryParams.Select(item => $"{item.Key}={Uri.EscapeDataString(item.Value)}").ToList())}";
    return Redirect(url);
  }

  [HttpGet("tasks")]
  public async Task<ActionResult> GetTaskList() {
    try {
      if (userToken == null) {
        return Ok("User not authenticated");
      }
      var credential = GoogleCredential.FromAccessToken(userToken.AccessToken);
      var taskService = new TasksService(new BaseClientService.Initializer() {
        HttpClientInitializer = credential,
        ApplicationName = "TheChatbot"
      });
      var taskLists = await taskService.Tasklists.List().ExecuteAsync();
      var taskListId = taskLists.Items[1].Id;
      var tasks = await taskService.Tasks.List(taskListId).ExecuteAsync();
      return Ok(tasks);
    } catch (Exception ex) {
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
      var id = googleSheetsConfig.MainId;
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

