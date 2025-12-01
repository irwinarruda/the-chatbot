namespace TheChatbot.Resources;

public class TestSpeechToTextGateway : ISpeechToTextGateway {
  public Task<string> TranscribeAsync(TranscribeAudioDTO audio) {
    return Task.FromResult("This is a mock transcript for testing purposes.");
  }
}
