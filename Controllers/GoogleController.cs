using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheChatbot.Dtos;
using Google.Apis.Tasks.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Caching.Memory;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase {
  readonly GoogleOAuthConfig googleConfig;
  readonly GoogleTokenResponse? userToken;
  readonly IMemoryCache cache;

  public GoogleController(IConfiguration configuration, IMemoryCache _cache) {
    googleConfig = configuration.GetSection("GoogleOAuthConfig").Get<GoogleOAuthConfig>()!;
    cache = _cache;
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
    return Ok(token);
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect() {
    var queryParams = new Dictionary<string, string> {
      { "client_id", googleConfig.ClientId },
      { "response_type", "code" },
      { "scope", "openid email profile https://www.googleapis.com/auth/tasks" },
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
      var taskService = new TasksService(new BaseClientService.Initializer() {
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
}
