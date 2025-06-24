using Newtonsoft.Json;

namespace TheChatbot.Dtos;

public class GoogleOAuthConfig {
  public string ClientId { get; set; } = string.Empty;
  public string SecretClientKey { get; set; } = string.Empty;
  public string AuthorizationEndpoint { get; set; } = string.Empty;
  public string TokenEndpoint { get; set; } = string.Empty;
  public string UserInfoEndpoint { get; set; } = string.Empty;
  public string RedirectUri { get; set; } = string.Empty;
  public string ServiceAccountId { get; set; } = string.Empty;
  public string ServiceAccountPrivateKey { get; set; } = string.Empty;
}

public class GoogleSheetsConfig {
  public string MainId { get; set; } = string.Empty;
  public string MainToken { get; set; } = string.Empty;
}

public class GoogleTokenResponse {
  [JsonProperty("access_token")]
  public string? AccessToken { get; set; }
  [JsonProperty("refresh_token")]
  public string? RefreshToken { get; set; }
  [JsonProperty("token_type")]
  public string? TokenType { get; set; }
  [JsonProperty("expires_in")]
  public int ExpiresIn { get; set; }
  [JsonProperty("id_token")]
  public string? IdToken { get; set; }
  [JsonProperty("scope")]
  public string? Scope { get; set; }
}

