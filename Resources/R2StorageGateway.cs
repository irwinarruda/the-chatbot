using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

using TheChatbot.Infra;

namespace TheChatbot.Resources;

public class R2StorageGateway : IStorageGateway {
  private readonly AmazonS3Client s3Client;
  private readonly R2Config r2Config;

  public R2StorageGateway(R2Config _config) {
    r2Config = _config;
    var credentials = new BasicAWSCredentials(r2Config.AccessKeyId, r2Config.SecretAccessKey);
    s3Client = new AmazonS3Client(credentials, new AmazonS3Config() {
      ServiceURL = r2Config.ServiceUrl,
      ForcePathStyle = true
    });
  }

  public async Task<string> UploadFileAsync(UploadFileDTO file) {
    var request = new PutObjectRequest {
      BucketName = r2Config.BucketName,
      Key = file.Key,
      InputStream = file.Content,
      ContentType = file.ContentType,
      DisablePayloadSigning = true
    };
    await s3Client.PutObjectAsync(request);
    return $"{r2Config.PublicUrl}/{file.Key}";
  }
}
