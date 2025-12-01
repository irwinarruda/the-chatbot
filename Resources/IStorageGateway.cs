namespace TheChatbot.Resources;

public class UploadFileDTO {
  public required string Key { get; set; }
  public required Stream Content { get; set; }
  public required string ContentType { get; set; }
}

public interface IStorageGateway {
  Task<string> UploadFileAsync(UploadFileDTO file);
}
