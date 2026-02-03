using PromptGeneratorWebApi.Presentation.Extensions;
using PromptGeneratorWebApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddHttpClient<PromptGeneratorService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the API (middleware and endpoints)
app.ConfigureApi();

app.Run();

// Make Program public for testing
public partial class Program { }
