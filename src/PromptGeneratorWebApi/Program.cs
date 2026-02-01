using PromptGeneratorWebApi.Models;
using PromptGeneratorWebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register HttpClient and PromptGeneratorService
builder.Services.AddHttpClient<IPromptGeneratorService, PromptGeneratorService>();

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();

// Minimal API Endpoints
app.MapPost("/api/generate-prompt", async (PromptRequest request, IPromptGeneratorService service) =>
{
    if (string.IsNullOrWhiteSpace(request.Problem))
    {
        return Results.BadRequest(new PromptResponse
        {
            Success = false,
            Error = "Problem cannot be empty"
        });
    }

    try
    {
        var (analysis, context, optimizedPrompt) = await service.GenerateOptimizedPromptAsync(request.Problem);

        return Results.Ok(new PromptResponse
        {
            Success = true,
            OptimizedPrompt = optimizedPrompt,
            Details = new PromptGenerationDetails
            {
                Analysis = analysis,
                Context = context
            }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            detail: ex.Message,
            statusCode: 500,
            title: "Error generating prompt"
        );
    }
})
.WithName("GeneratePrompt")
.WithOpenApi()
.WithDescription("Generates an optimized ChatGPT prompt from a user problem through a multi-stage AI pipeline");

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithOpenApi()
    .WithDescription("Returns the health status of the API");

app.Run();

// Make Program public for testing
public partial class Program { }
