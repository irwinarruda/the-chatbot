namespace TheChatbot.Resources;

public class TranscribeAudioDTO {
  public required Stream AudioStream { get; set; }
  public required string MimeType { get; set; }
}

public interface ISpeechToTextGateway {
  Task<string> TranscribeAsync(TranscribeAudioDTO audio);
}
