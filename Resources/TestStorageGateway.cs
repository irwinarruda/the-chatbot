namespace TheChatbot.Resources;

public class TestStorageGateway : IStorageGateway {
  public Task<string> UploadFileAsync(UploadFileDTO file) {
    return Task.FromResult($"https://test-storage.example.com/{file.Key}");
  }
}
