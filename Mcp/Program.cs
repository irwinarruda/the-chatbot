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
builder.Services.AddSingleton<IMediator, Mediator>();
builder.Services.AddSingleton<CashFlowService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!);

builder.Services
  .AddMcpServer()
  .WithStdioServerTransport()
  .WithToolsFromAssembly();

var app = builder.Build();
await app.RunAsync();
