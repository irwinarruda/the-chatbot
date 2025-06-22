using TheChatbot.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMemoryCache();

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

// using TheChatbot.Dtos;

// // Make Program class public and partial so it can be extended for testing
// public partial class Program {
//   // This method is required to support WebApplicationFactory in integration tests
//   public static void Main(string[] args) {
//     var builder = WebApplication.CreateBuilder(args);

//     // Add services to the container.
//     builder.Services.AddControllers();
//     builder.Services.AddMemoryCache();

//     // Configure Google Cloud Tasks
//     builder.Services.Configure<GoogleOAuthConfig>(builder.Configuration.GetSection("GoogleOAuthConfig"));

//     var app = builder.Build();

//     // Configure the HTTP request pipeline.

//     app.UseHttpsRedirection();

//     app.UseAuthorization();

//     app.MapControllers();

//     // Important: Create the app for integration testing but only run it in normal execution
//     if (args.Length == 0) // Don't call Run() method during integration tests
//     {
//       app.Run();
//     }
//   }
// }

