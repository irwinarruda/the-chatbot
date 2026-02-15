using System.ClientModel;

using OpenAI.Audio;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class OpenAiSpeechToTextGateway : ISpeechToTextGateway {
  private readonly AudioClient audioClient;

  public OpenAiSpeechToTextGateway(OpenAiConfig openAiConfig) {
    audioClient = new AudioClient(openAiConfig.SpeechModel, openAiConfig.ApiKey);
  }

  public async Task<string> TranscribeAsync(TranscribeAudioDTO audio) {
    using var memoryStream = new MemoryStream();
    await audio.AudioStream.CopyToAsync(memoryStream);
    var audioBytes = memoryStream.ToArray();
    if (audioBytes.Length == 0) {
      return string.Empty;
    }
    if (audioBytes.Length > 25_000_000) {
      throw new ValidationException("Audio too large for transcription", "Use a shorter audio file (max 25 MB) and try again");
    }
    var extension = GetExtensionFromMimeType(audio.MimeType);
    var filename = $"audio.{extension}";
    using var stream = new MemoryStream(audioBytes);
    var options = new AudioTranscriptionOptions {
      ResponseFormat = AudioTranscriptionFormat.Text,
    };
    ClientResult<AudioTranscription> result = await audioClient.TranscribeAudioAsync(stream, filename, options);
    return result.Value.Text?.Trim() ?? string.Empty;
  }

  private static string GetExtensionFromMimeType(string mimeType) {
    return mimeType.ToLowerInvariant() switch {
      "audio/ogg" => "ogg",
      "audio/mpeg" => "mp3",
      "audio/mp3" => "mp3",
      "audio/mp4" => "mp4",
      "audio/m4a" => "m4a",
      "audio/x-m4a" => "m4a",
      "audio/wav" => "wav",
      "audio/wave" => "wav",
      "audio/x-wav" => "wav",
      "audio/webm" => "webm",
      "audio/mpga" => "mpga",
      _ => "ogg",
    };
  }
}
