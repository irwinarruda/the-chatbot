using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util;

using Newtonsoft.Json;

namespace TheChatbot.Resources;

public class GoogleTokenResponse {
  internal const int TokenRefreshWindowSeconds = 60 * 3 + 45;
  internal const int TokenInvalidWindowSeconds = 60;
  public string AccessToken { get; set; } = string.Empty;
  public string TokenType { get; set; } = string.Empty;
  public long? ExpiresInSeconds { get; set; }
  public string RefreshToken { get; set; } = string.Empty;
  public string Scope { get; set; } = string.Empty;
  public string IdToken { get; set; } = string.Empty;
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
  public static GoogleTokenResponse FromTokenResponse(TokenResponse tokenResponse) {
    return new GoogleTokenResponse {
      AccessToken = tokenResponse.AccessToken,
      RefreshToken = tokenResponse.RefreshToken,
      ExpiresInSeconds = tokenResponse.ExpiresInSeconds,
      IdToken = tokenResponse.IdToken,
      IssuedUtc = tokenResponse.IssuedUtc,
      Scope = tokenResponse.Scope,
      TokenType = tokenResponse.TokenType,
    };
  }
}

public interface IGoogleAuthGateway {
  string CreateAuthorizationCodeUrl();
  Task<GoogleTokenResponse> ExchangeCodeForTokenAsync(string code);
  Task<GoogleTokenResponse> RefreshToken(GoogleTokenResponse response);
}
