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
  readonly public IFinantialPlanningSpreadsheet finantialPlanningSpreadsheet;
  readonly public IWhatsAppMessagingGateway whatsAppMessagingGateway;
  readonly public IGoogleAuthGateway googleAuthGateway;
  readonly public IConfiguration configuration;
  readonly public IServiceProvider serviceProvider;
  readonly public EncryptionConfig encryptionConfig;
  readonly public DatabaseConfig databaseConfig;
  readonly public GoogleSheetsConfig googleSheetsConfig;
  readonly public GoogleConfig googleConfig;

  public Orquestrator() {
    configuration = Configurable.Make();
    databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!;
    googleSheetsConfig = configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!;
    googleConfig = configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!;
    encryptionConfig = configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!;
    var services = new ServiceCollection();
    services.AddSingleton(databaseConfig);
    services.AddSingleton(encryptionConfig);
    services.AddSingleton(googleConfig);
    services.AddSingleton(googleSheetsConfig);

    if (googleSheetsConfig.TestSheetId != "TestSheetId") {
      services.AddSingleton<IFinantialPlanningSpreadsheet, GoogleFinantialPlanningSpreadsheet>();
    } else {
      services.AddSingleton<IFinantialPlanningSpreadsheet, TestFinantialPlanningSpreadsheet>();
    }

    services.AddSingleton<IGoogleAuthGateway, TestGoogleAuthGateway>();
    services.AddSingleton<IWhatsAppMessagingGateway, TestWhatsAppMessagingGateway>();
    services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
    services.AddTransient<AuthService>();
    services.AddTransient<MessagingService>();
    services.AddTransient<StatusService>();

    serviceProvider = services.BuildServiceProvider();

    finantialPlanningSpreadsheet = serviceProvider.GetRequiredService<IFinantialPlanningSpreadsheet>();
    googleAuthGateway = serviceProvider.GetRequiredService<IGoogleAuthGateway>();
    whatsAppMessagingGateway = serviceProvider.GetRequiredService<IWhatsAppMessagingGateway>();
    authService = serviceProvider.GetRequiredService<AuthService>();
    messagingService = serviceProvider.GetRequiredService<MessagingService>();
    statusService = serviceProvider.GetRequiredService<StatusService>();
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
}
