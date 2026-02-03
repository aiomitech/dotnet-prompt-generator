using PromptGeneratorWebApi.Presentation.Extensions;
using PromptGeneratorWebApi.Application.Interfaces;
using PromptGeneratorWebApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddHttpClient<PromptGeneratorService>();
builder.Services.AddScoped<IPromptGeneratorService>(sp => sp.GetRequiredService<PromptGeneratorService>());
builder.Services.AddScoped<PromptGeneratorServiceV2>();

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
