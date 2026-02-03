using PromptGeneratorWebApi.Presentation.Models;
using PromptGeneratorWebApi.Infrastructure.Services;

namespace PromptGeneratorWebApi.Presentation.Extensions;

/// <summary>
/// Extension methods for configuring the API middleware and endpoints.
/// </summary>
public static class ApiConfigurationExtensions
{
    private const string ApiVersion = "v1";
    private const string ApiBasePath = "/api/" + ApiVersion;

    /// <summary>
    /// Configures the API by setting up middleware pipeline and registering endpoints.
    /// </summary>
    /// <param name="app">The WebApplication instance to configure.</param>
    /// <returns>The WebApplication instance for chaining.</returns>
    public static WebApplication ConfigureApi(this WebApplication app)
    {
        ConfigureMiddleware(app);
        MapEndpoints(app);
        return app;
    }

    /// <summary>
    /// Configures the HTTP request pipeline middleware.
    /// </summary>
    private static void ConfigureMiddleware(WebApplication app)
    {
        // Configure Swagger in development environment
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseCors();
    }

    /// <summary>
    /// Maps all API endpoints.
    /// </summary>
    private static void MapEndpoints(WebApplication app)
    {
        var api = app.MapGroup(ApiBasePath);
        MapPromptGenerationEndpoint(api);
        MapExpertGenerationEndpoint(api);
        MapHealthCheckEndpoint(api);
    }

    /// <summary>
    /// Maps the POST /api/v1/generate-prompt endpoint.
    /// </summary>
    private static void MapPromptGenerationEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/generate-prompt", async (PromptRequest request, PromptGeneratorService service) =>
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
    }

    /// <summary>
    /// Maps the POST /api/v1/expert-generate-prompt endpoint.
    /// Uses the advanced ExpertGeneratorService_V1 with three-stage canonical JSON pipeline.
    /// </summary>
    private static void MapExpertGenerationEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/expert-generate-prompt", async (PromptRequest request, ExpertGeneratorService_V1 service) =>
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

                return Results.Ok(new
                {
                    success = true,
                    data = new
                    {
                        optimizedPrompt,
                        details = new
                        {
                            expertDesign = analysis,
                            methodologyExecution = context,
                            optimizedResponse = optimizedPrompt
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: 500,
                    title: "Error generating expert prompt"
                );
            }
        })
        .WithName("ExpertGeneratePrompt")
        .WithOpenApi()
        .WithDescription("Advanced three-stage expert prompt generation using canonical JSON schemas and strict response formatting");
    }

    /// <summary>
    /// Maps the GET /api/v1/health endpoint.
    /// </summary>
    private static void MapHealthCheckEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
            .WithName("HealthCheck")
            .WithOpenApi()
            .WithDescription("Returns the health status of the API");
    }
}
