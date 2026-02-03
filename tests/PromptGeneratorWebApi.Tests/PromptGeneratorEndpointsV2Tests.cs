using System.Text;
using System.Text.Json;
using Moq;
using PromptGeneratorWebApi.Presentation.Models;
using Xunit;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Integration tests for the V2 PromptGeneratorWebApi endpoints.
/// Tests the /api/v2/generate-prompt endpoint with various scenarios.
/// </summary>
public class PromptGeneratorEndpointsV2Tests : IDisposable
{
    private readonly WebApiTestFactory _factory;
    private readonly HttpClient _client;

    public PromptGeneratorEndpointsV2Tests()
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
    public async Task GeneratePromptV2_WithValidInput_ReturnsVersion2()
    {
        // Arrange
        var request = new PromptRequest { Problem = "How do I optimize database queries?" };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v2/generate-prompt", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PromptResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Version2", result.OptimizedPrompt);
        Assert.Null(result.Details); // V2 doesn't return details yet
    }

    [Theory]
    [InlineData("Test problem")]
    [InlineData("Another test")]
    [InlineData("Complex multi-line\nproblem statement")]
    public async Task GeneratePromptV2_WithVariousInputs_ReturnsVersion2(string problem)
    {
        // Arrange
        var request = new PromptRequest { Problem = problem };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v2/generate-prompt", content);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PromptResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Equal("Version2", result.OptimizedPrompt);
    }

    [Fact]
    public async Task GeneratePromptV2_WithEmptyProblem_ReturnsBadRequest()
    {
        // Arrange
        var request = new PromptRequest { Problem = string.Empty };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v2/generate-prompt", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PromptResponse>(
            responseContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
        Assert.Contains("empty", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GeneratePromptV2_WithWhitespaceOnlyProblem_ReturnsBadRequest()
    {
        // Arrange
        var request = new PromptRequest { Problem = "   " };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/v2/generate-prompt", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheckV2_ReturnsHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/v2/health");

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        var content = await response.Content.ReadAsStringAsync();
        
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;
        
        Assert.True(root.TryGetProperty("status", out var status));
        Assert.Equal("healthy", status.GetString());
        
        Assert.True(root.TryGetProperty("version", out var version));
        Assert.Equal("v2", version.GetString());
        
        Assert.True(root.TryGetProperty("timestamp", out _));
    }
}
