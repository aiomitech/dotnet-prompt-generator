using Xunit;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Integration tests for the health check endpoint.
/// </summary>
public class HealthCheckEndpointTests : IDisposable
{
    private readonly WebApiTestFactory _factory;
    private readonly HttpClient _client;

    public HealthCheckEndpointTests()
    {
        _factory = new WebApiTestFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOkStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOkStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_ReturnsJsonContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Contains("application/json", response.Content.Headers.ContentType.ToString());
    }
}
