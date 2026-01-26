using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.AI;

using System.Threading.RateLimiting;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

using WhatsappBusiness.CloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
builder.Services.AddSingleton<IAiChatGateway, AiChatGateway>();
builder.Services.AddSingleton<IGoogleAuthGateway, GoogleAuthGateway>();
builder.Services.AddSingleton<IWhatsAppMessagingGateway, WhatsAppMessagingGateway>();
builder.Services.AddSingleton<IStorageGateway, R2StorageGateway>();
builder.Services.AddSingleton<ISpeechToTextGateway, GoogleSpeechToTextGateway>();
builder.Services.AddSingleton<IMediator, Mediator>();
builder.Services.AddSingleton<StatusService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<MessagingService>();
builder.Services.AddTransient<MigrationService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("McpConfig").Get<McpConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("AuthConfig").Get<AuthConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("R2Config").Get<R2Config>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("SummarizationConfig").Get<SummarizationConfig>()!);

builder.Services.AddRateLimiter(options => {
  options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
  options.AddPolicy("MigrationPolicy", context =>
    RateLimitPartition.GetFixedWindowLimiter(
      partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
      factory: _ => new() {
        PermitLimit = 5,
        Window = TimeSpan.FromMinutes(1),
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0
      }
    )
  );
});

var whatsAppConfig = builder.Configuration.GetSection("WhatsAppConfig").Get<WhatsAppConfig>()!;
builder.Services.AddSingleton(whatsAppConfig);
builder.Services.AddWhatsAppBusinessCloudApiService(new() {
  AppName = whatsAppConfig.AppName,
  Version = whatsAppConfig.Version,
  WebhookVerifyToken = whatsAppConfig.WebhookVerifyToken,
  WhatsAppBusinessAccountId = whatsAppConfig.WhatsAppBusinessAccountId,
  WhatsAppBusinessId = whatsAppConfig.WhatsAppBusinessId,
  WhatsAppBusinessPhoneNumberId = whatsAppConfig.WhatsAppBusinessPhoneNumberId,
  AccessToken = whatsAppConfig.AccessToken
});
var openAIConfig = builder.Configuration.GetSection("OpenAIConfig").Get<OpenAIConfig>()!;
builder.Services.AddSingleton(openAIConfig);
builder.Services.AddChatClient(_ => {
  var chatClient = new OpenAI.Chat.ChatClient(openAIConfig.Model, openAIConfig.ApiKey).AsIChatClient();
  var builder = new ChatClientBuilder(chatClient).UseFunctionInvocation();
  return builder.Build();
});

var app = builder.Build();

var mediator = app.Services.GetRequiredService<IMediator>();
mediator.Register("SaveUserByGoogleCredential", async (string phoneNumber) => {
  var messagingService = app.Services.GetRequiredService<MessagingService>();
  await messagingService.SendSignedInMessage(phoneNumber);
});
mediator.Register("DeleteUserByPhoneNumber", async (string phoneNumber) => {
  var messagingService = app.Services.GetRequiredService<MessagingService>();
  await messagingService.DeleteChat(phoneNumber);
});
mediator.Register("RespondToMessage", async (RespondToMessageEvent data) => {
  var messagingService = app.Services.GetRequiredService<MessagingService>();
  await messagingService.RespondToMessage(data.Chat, data.Message);
});

app.UseExceptionHandler(exception => exception.Run(Controller.HandleException));
app.UseStatusCodePages(Controller.HandleInternal);
app.UseHttpsRedirection();
app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
