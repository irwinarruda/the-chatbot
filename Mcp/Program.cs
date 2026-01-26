using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

var builder = Host.CreateApplicationBuilder(args);
Configurable.Enhance(builder.Configuration);

builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
builder.Services.AddSingleton<ICashFlowSpreadsheetGateway, GoogleCashFlowSpreadsheetGateway>();
builder.Services.AddSingleton<IGoogleAuthGateway, GoogleAuthGateway>();
builder.Services.AddSingleton<IAiChatGateway, AiChatGateway>();
builder.Services.AddSingleton<IMediator, Mediator>();
builder.Services.AddSingleton<CashFlowService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("McpConfig").Get<McpConfig>()!);
var aiConfig = builder.Configuration.GetSection("AiConfig").Get<AiConfig>()!;
builder.Services.AddChatClient(_ => {
  var chatClient = aiConfig.Provider == "anthropic"
    ? new Anthropic.AnthropicClient() { ApiKey = aiConfig.ApiKey }.AsIChatClient(aiConfig.Model)
    : new OpenAI.Chat.ChatClient(aiConfig.Model, aiConfig.ApiKey).AsIChatClient();
  var builder = new ChatClientBuilder(chatClient).UseFunctionInvocation();
  return builder.Build();
});

builder.Services
  .AddMcpServer()
  .WithStdioServerTransport()
  .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
