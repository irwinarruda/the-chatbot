namespace TheChatbot.Infra;

public class GoogleConfig {
  public string ClientId { get; set; } = string.Empty;
  public string SecretClientKey { get; set; } = string.Empty;
  public string AuthorizationEndpoint { get; set; } = string.Empty;
  public string TokenEndpoint { get; set; } = string.Empty;
  public string UserInfoEndpoint { get; set; } = string.Empty;
  public string RedirectUri { get; set; } = string.Empty;
  public string LoginUri { get; set; } = string.Empty;
  public string ServiceAccountId { get; set; } = string.Empty;
  public string ServiceAccountPrivateKey { get; set; } = string.Empty;
  public string SpeechProjectId { get; set; } = string.Empty;
  public string SpeechRegion { get; set; } = string.Empty;
  public string ApplicationName { get; set; } = string.Empty;
}

public class GoogleSheetsConfig {
  public string TestSheetId { get; set; } = string.Empty;
}
