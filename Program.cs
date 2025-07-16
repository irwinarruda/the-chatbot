using System.Text.Json;
using System.Text.Json.Serialization;

using TheChatbot.Infra;
using TheChatbot.Resources;
using TheChatbot.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);
builder.Services.AddSingleton<StatusService>();
builder.Services.AddSingleton<IFinantialPlanningSpreadsheet, GoogleFinantialPlanningSpreadsheet>();
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleConfig").Get<GoogleConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("GoogleSheetsConfig").Get<GoogleSheetsConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>()!);
builder.Services.AddSingleton(builder.Configuration.GetSection("EncryptionConfig").Get<EncryptionConfig>()!);

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(Controller.HandleException));
app.UseStatusCodePages(Controller.HandleInternal);
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
