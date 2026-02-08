using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V2;
using Google.Protobuf;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class GoogleSpeechToTextGateway : ISpeechToTextGateway {
  private readonly SpeechClient speechClient;
  private readonly GoogleConfig googleConfig;

  public GoogleSpeechToTextGateway(GoogleConfig googleConfig) {
    this.googleConfig = googleConfig;
    var initializer = new ServiceAccountCredential.Initializer(googleConfig.ServiceAccountId) {
      Scopes = ["https://www.googleapis.com/auth/cloud-platform"]
    }.FromPrivateKey(googleConfig.ServiceAccountPrivateKey);
    var credential = GoogleCredential.FromServiceAccountCredential(new ServiceAccountCredential(initializer));
    speechClient = new SpeechClientBuilder {
      Credential = credential,
      Endpoint = googleConfig.SpeechEndpoint,
    }.Build();
  }

  public async Task<string> TranscribeAsync(TranscribeAudioDTO audio) {
    using var memoryStream = new MemoryStream();
    await audio.AudioStream.CopyToAsync(memoryStream);
    var audioBytes = memoryStream.ToArray();
    if (audioBytes.Length == 0) {
      return string.Empty;
    }
    if (audioBytes.Length > 9_500_000) {
      throw new ValidationException("Audio too large for synchronous recognition", "Use a shorter audio file and try again");
    }
    var recognizer = $"projects/{googleConfig.SpeechProjectId}/locations/{googleConfig.SpeechRegion}/recognizers/{googleConfig.SpeechRecognizerId}";
    var request = new RecognizeRequest {
      Recognizer = recognizer,
      Config = new RecognitionConfig {
        AutoDecodingConfig = new AutoDetectDecodingConfig(),
        Model = googleConfig.SpeechModel,
        LanguageCodes = { googleConfig.SpeechLanguageCodes },
        Features = new() { EnableAutomaticPunctuation = true },
      },
      Content = ByteString.CopyFrom(audioBytes),
    };
    var response = await speechClient.RecognizeAsync(request);
    var transcript = string.Join(" ", response.Results
      .SelectMany((r) => r.Alternatives)
      .Select((a) => a.Transcript)
      .Where((t) => !string.IsNullOrWhiteSpace(t)));
    return transcript;
  }
}
