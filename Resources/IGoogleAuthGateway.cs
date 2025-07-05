using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;

using Newtonsoft.Json;

namespace TheChatbot.Resources;

public class GoogleTokenResponse {
  private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  internal const int TokenRefreshWindowSeconds = 60 * 3 + 45;
  internal const int TokenInvalidWindowSeconds = 60;

  [JsonProperty("access_token")]
  public string AccessToken { get; set; } = string.Empty;

  [JsonProperty("token_type")]
  public string TokenType { get; set; } = string.Empty;

  [JsonProperty("expires_in")]
  public long? ExpiresInSeconds { get; set; }

  [JsonProperty("refresh_token")]
  public string RefreshToken { get; set; } = string.Empty;

  [JsonProperty("scope")]
  public string Scope { get; set; } = string.Empty;

  [JsonProperty("id_token")]
  public string IdToken { get; set; } = string.Empty;

  [Obsolete("Use IssuedUtc instead")]
  [JsonProperty(Order = 1)] // Serialize this before IssuedUtc, so that IssuedUtc takes priority when deserializing
  public DateTime Issued {
    get { return IssuedUtc.ToLocalTime(); }
    set { IssuedUtc = value.ToUniversalTime(); }
  }

  [JsonProperty(Order = 2)]
  public DateTime IssuedUtc { get; set; }

  [JsonIgnore]
  public bool IsStale => ShouldBeRefreshed(SystemClock.Default);

  [JsonIgnore]
  internal DateTime? RefreshWindowStartUtc => ExpiresInSeconds.HasValue
? IssuedUtc.AddSeconds(ExpiresInSeconds.Value - TokenRefreshWindowSeconds)
: null;

  [JsonIgnore]
  internal DateTime? ExpiryWindowStartUtc => ExpiresInSeconds.HasValue
? IssuedUtc.AddSeconds(ExpiresInSeconds.Value - TokenInvalidWindowSeconds)
: null;

  [Obsolete("Please use the TokenResponse.IsStale property instead.")]
  public bool IsExpired(IClock clock) => ShouldBeRefreshed(clock);

  internal bool ShouldBeRefreshed(IClock clock) => !MayBeUsed(clock) || clock.UtcNow >= RefreshWindowStartUtc;

  internal bool MayBeUsed(IClock clock) => (AccessToken is not null || IdToken is not null) &&
    ExpiresInSeconds.HasValue &&
    clock.UtcNow < ExpiryWindowStartUtc;
}

public interface IGoogleAuthGateway {
  string CreateAuthorizationCodeUrl();
  Task<GoogleTokenResponse> ExchangeCodeForTokenAsync();
  Task<GoogleTokenResponse> RefreshToken(GoogleTokenResponse response);
}
