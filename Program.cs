using TheChatbot.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

// Configure Google Cloud Tasks
builder.Services.Configure<GoogleOAuthConfig>(builder.Configuration.GetSection("GoogleOAuthConfig"));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
