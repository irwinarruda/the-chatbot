using TheChatbot.Dtos;
using TheChatbot.Resources;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IFinantialPlanningSpreadsheet, GoogleFinantialPlanningSpreadsheet>();

// Configure Google Cloud Tasks
builder.Services.Configure<GoogleOAuthConfig>(builder.Configuration.GetSection("GoogleOAuthConfig"));
builder.Services.Configure<GoogleSheetsConfig>(builder.Configuration.GetSection("GoogleSheetsConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

if (args.Length == 0) {
  app.Run();
}

public partial class Program { }
