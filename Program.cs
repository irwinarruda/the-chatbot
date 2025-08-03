using System.Text.Json;
using System.Text.Json.Serialization;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;
using TheChatbot.Utils;

using WhatsappBusiness.CloudApi.Configurations;
using WhatsappBusiness.CloudApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
builder.Services.AddSingleton<IMediator, Mediator>();
builder.Services.AddSingleton<IFinantialPlanningSpreadsheet, GoogleFinantialPlanningSpreadsheet>();
builder.Services.AddSingleton<IGoogleAuthGateway, GoogleAuthGateway>();
builder.Services.AddSingleton<IWhatsAppMessagingGateway, WhatsAppMessagingGateway>();
builder.Services.AddSingleton<StatusService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<MessagingService>();
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!);
var whatsAppConfig = builder.Configuration.GetSection("WhatsAppConfig").Get<WhatsAppBusinessCloudApiConfig>()!;
builder.Services.AddSingleton(whatsAppConfig);
builder.Services.AddWhatsAppBusinessCloudApiService(whatsAppConfig);

var app = builder.Build();

var mediator = app.Services.GetRequiredService<IMediator>();
mediator.Register("SaveUserByGoogleCredential", async (string phoneNumber) => {
  var messagingService = app.Services.GetRequiredService<MessagingService>();
  await messagingService.SendSignedInMessage(phoneNumber);
});

app.UseExceptionHandler(errorApp => errorApp.Run(Controller.HandleException));
app.UseStatusCodePages(Controller.HandleInternal);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
