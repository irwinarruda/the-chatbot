using Google.Apis.Auth.OAuth2;
using Google.Apis.Speech.v1;
using Google.Apis.Speech.v1.Data;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class GoogleSpeechToTextGateway : ISpeechToTextGateway {
  private readonly SpeechService speechService;

  public GoogleSpeechToTextGateway(GoogleConfig googleConfig) {
    var initializer = new ServiceAccountCredential.Initializer(googleConfig.ServiceAccountId) {
      Scopes = [SpeechService.Scope.CloudPlatform]
    }.FromPrivateKey(googleConfig.ServiceAccountPrivateKey);
    var credential = new ServiceAccountCredential(initializer);
    speechService = new SpeechService(new() {
      HttpClientInitializer = credential,
      ApplicationName = googleConfig.ApplicationName
    });
  }

  public async Task<string> TranscribeAsync(TranscribeAudioDTO audio) {
    using var memoryStream = new MemoryStream();
    await audio.AudioStream.CopyToAsync(memoryStream);
    var audioBytes = memoryStream.ToArray();

    var encoding = GetEncoding(audio.MimeType);
    var request = new RecognizeRequest {
      Audio = new() {
        Content = Convert.ToBase64String(audioBytes)
      },
      Config = new() {
        Encoding = encoding,
        SampleRateHertz = 16000,
        LanguageCode = "pt-BR",
        Model = "latest_long",
        AlternativeLanguageCodes = ["en-US"],
        EnableAutomaticPunctuation = true
      }
    };

    var response = await speechService.Speech.Recognize(request).ExecuteAsync();
    if (response.Results == null) {
      return string.Empty;
    }
    var transcript = string.Join(" ", response.Results
      .Where(r => r.Alternatives != null)
      .SelectMany(r => r.Alternatives!)
      .Select(a => a.Transcript));

    return transcript;
  }

  private static string GetEncoding(string mimeType) {
    return mimeType.ToLowerInvariant() switch {
      "audio/ogg" or "audio/ogg; codecs=opus" => "OGG_OPUS",
      "audio/flac" => "FLAC",
      "audio/wav" or "audio/wave" => "LINEAR16",
      "audio/mp3" or "audio/mpeg" => "MP3",
      "audio/webm" or "audio/webm; codecs=opus" => "WEBM_OPUS",
      _ => "ENCODING_UNSPECIFIED"
    };
  }
}
