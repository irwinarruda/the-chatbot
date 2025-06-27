using System.Text.Json;
using System.Text.Json.Serialization;

using TheChatbot.Infra;
using TheChatbot.Resources;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
  options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
  options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IFinantialPlanningSpreadsheet, GoogleFinantialPlanningSpreadsheet>();

builder.Services.Configure<GoogleConfig>(builder.Configuration.GetSection("GoogleConfig"));
builder.Services.Configure<GoogleSheetsConfig>(builder.Configuration.GetSection("GoogleSheetsConfig"));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
