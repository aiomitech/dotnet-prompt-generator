using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using PromptGeneratorWebApi.Services;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Custom WebApplicationFactory for testing the PromptGeneratorWebApi.
/// Sets up the test environment with test dependencies.
/// </summary>
public class WebApiTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Add any test-specific configuration here
    }
}
