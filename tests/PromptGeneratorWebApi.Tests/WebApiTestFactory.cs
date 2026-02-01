using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PromptGeneratorWebApi.Services;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Custom WebApplicationFactory for testing the PromptGeneratorWebApi.
/// Sets up the test environment with mocked dependencies.
/// </summary>
public class WebApiTestFactory : WebApplicationFactory<Program>
{
    public Mock<IPromptGeneratorService> MockPromptGeneratorService { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real PromptGeneratorService registration
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(IPromptGeneratorService));
            
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add mock service
            services.AddScoped(_ => MockPromptGeneratorService.Object);
        });
    }
}
