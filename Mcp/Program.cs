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
var openAIConfig = builder.Configuration.GetSection("OpenAIConfig").Get<OpenAIConfig>()!;
builder.Services.AddChatClient(_ => {
  var chatClient = new OpenAI.Chat.ChatClient("gpt-4o-mini", openAIConfig.ApiKey).AsIChatClient();
  var builder = new ChatClientBuilder(chatClient).UseFunctionInvocation();
  return builder.Build();
});

builder.Services
  .AddMcpServer()
  .WithStdioServerTransport()
  .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
