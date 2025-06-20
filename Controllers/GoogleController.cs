using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TheChatbot.Dtos;

namespace TheChatbot.Controllers;

[ApiController]
[Route("[controller]")]
public class GoogleController : ControllerBase {
  readonly GoogleOAuthConfig googleConfig;

  public GoogleController(IConfiguration configuration) {
    googleConfig = configuration.GetSection("GoogleOAuthConfig").Get<GoogleOAuthConfig>()!;
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
    var tokens = JsonConvert.DeserializeObject<GoogleTokenResponse>(tokenResponseContent);
    return Ok(tokens);
  }

  [HttpGet("login")]
  public RedirectResult GetRedirect() {
    var queryParams = new Dictionary<string, string> {
      { "client_id", googleConfig.ClientId },
      { "response_type", "code" },
      { "scope", "openid email profile" },
      { "redirect_uri", googleConfig.RedirectUri },
    };
    var url = $"{googleConfig.AuthorizationEndpoint}?{string.Join("&", queryParams.Select(item => $"{item.Key}={Uri.EscapeDataString(item.Value)}").ToList())}";
    return Redirect(url);
  }
}
