using Bogus;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TheChatbot.Entities;
using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

namespace Tests;

public class Orquestrator : WebApplicationFactory<Program> {
  readonly public AuthService authService;
  readonly public MessagingService messagingService;
  readonly public StatusService statusService;
  readonly public CashFlowService cashFlowService;
  readonly public MigrationService migrationService;
  readonly public ICashFlowSpreadsheetGateway cashFlowSpreadsheetGateway;
  readonly public IWhatsAppMessagingGateway whatsAppMessagingGateway;
  readonly public IGoogleAuthGateway googleAuthGateway;
  readonly public IAiChatGateway aiChatGateway;
  readonly public IMediator mediator;
  readonly public IConfiguration configuration;
  readonly public ServiceProvider serviceProvider;
  readonly public EncryptionConfig encryptionConfig;
  readonly public DatabaseConfig databaseConfig;
  readonly public GoogleSheetsConfig googleSheetsConfig;
  readonly public GoogleConfig googleConfig;
  readonly public AiConfig aiConfig;
  readonly public AuthConfig authConfig;
  readonly public SummarizationConfig summarizationConfig;
  readonly public OpenAiConfig openAiConfig;

  public Orquestrator() {
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    googleConfig = configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    encryptionConfig = configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!;
    aiConfig = configuration.GetSection("AiConfig").Get<AiConfig>()!;
    authConfig = configuration.GetSection("AuthConfig").Get<AuthConfig>()!;
    summarizationConfig = configuration.GetSection("SummarizationConfig").Get<SummarizationConfig>()!;
    openAiConfig = configuration.GetSection("OpenAiConfig").Get<OpenAiConfig>()!;
    var services = new ServiceCollection();
    services.AddSingleton(databaseConfig);
    services.AddSingleton(googleSheetsConfig);
    services.AddSingleton(googleConfig);
    services.AddSingleton(encryptionConfig);
    services.AddSingleton(aiConfig);
    services.AddSingleton(authConfig);
    services.AddSingleton(summarizationConfig);
    services.AddSingleton(openAiConfig);

    if (googleSheetsConfig.TestSheetId != "TestSheetId") {
      services.AddSingleton<ICashFlowSpreadsheetGateway, GoogleCashFlowSpreadsheetGateway>();
    } else {
      services.AddSingleton<ICashFlowSpreadsheetGateway, TestCashFlowSpreadsheetGateway>();
    }

    services.AddSingleton<IGoogleAuthGateway, TestGoogleAuthGateway>();
    services.AddSingleton<IWhatsAppMessagingGateway, TestWhatsAppMessagingGateway>();
    services.AddSingleton<IAiChatGateway, TestAiChatGateway>();
    services.AddSingleton<IStorageGateway, TestStorageGateway>();
    services.AddSingleton<ISpeechToTextGateway, TestSpeechToTextGateway>();
    services.AddSingleton<IMediator, Mediator>();
    services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
    services.AddTransient<AuthService>();
    services.AddTransient<MessagingService>();
    services.AddTransient<StatusService>();
    services.AddTransient<CashFlowService>();
    services.AddTransient<MigrationService>();

    serviceProvider = services.BuildServiceProvider();
    cashFlowSpreadsheetGateway = serviceProvider.GetRequiredService<ICashFlowSpreadsheetGateway>();
    googleAuthGateway = serviceProvider.GetRequiredService<IGoogleAuthGateway>();
    whatsAppMessagingGateway = serviceProvider.GetRequiredService<IWhatsAppMessagingGateway>();
    aiChatGateway = serviceProvider.GetRequiredService<IAiChatGateway>();
    authService = serviceProvider.GetRequiredService<AuthService>();
    messagingService = serviceProvider.GetRequiredService<MessagingService>();
    statusService = serviceProvider.GetRequiredService<StatusService>();
    cashFlowService = serviceProvider.GetRequiredService<CashFlowService>();
    migrationService = serviceProvider.GetRequiredService<MigrationService>();
    mediator = serviceProvider.GetRequiredService<IMediator>();

    mediator.Register("DeleteUserByPhoneNumber", async (string phoneNumber) => {
      var messagingService = serviceProvider.GetRequiredService<MessagingService>();
      await messagingService.DeleteChat(phoneNumber);
    });
    mediator.Register("RespondToMessage", async (RespondToMessageEvent data) => {
      var messagingService = serviceProvider.GetRequiredService<MessagingService>();
      await messagingService.RespondToMessage(data.Chat, data.Message);
    });
  }

  public async Task WipeDatabase() {
    var database = serviceProvider.GetRequiredService<AppDbContext>();
    await database.Execute($"DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
  }

  public async Task ClearDatabase() {
    var database = serviceProvider.GetRequiredService<AppDbContext>();
    await database.Execute($"DROP SCHEMA public CASCADE; CREATE SCHEMA public;");
    await RunPendingMigrations();
  }

  public async Task RunPendingMigrations() {
    var database = serviceProvider.GetRequiredService<AppDbContext>();
    await database.Database.MigrateAsync();
  }

  public async Task<User> CreateUser(string? name = null, string? phoneNumber = null) {
    var faker = new Faker();
    var user = new User {
      Name = name ?? faker.Name.FullName(),
      PhoneNumber = phoneNumber ?? faker.Phone.PhoneNumber("55###########")
    };
    await authService.CreateUser(user);
    return user;
  }

  public async Task DeleteUser(string phoneNumber) {
    await authService.DeleteUserByPhoneNumber(phoneNumber);
  }
}
