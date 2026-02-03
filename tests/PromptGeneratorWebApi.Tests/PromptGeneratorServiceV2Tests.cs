using PromptGeneratorWebApi.Application.Interfaces;
using PromptGeneratorWebApi.Infrastructure.Services;
using Xunit;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Unit tests for PromptGeneratorServiceV2.
/// Tests the V2 service logic in isolation.
/// </summary>
public class PromptGeneratorServiceV2Tests
{
    private readonly IPromptGeneratorService _service;

    public PromptGeneratorServiceV2Tests()
    {
        _service = new PromptGeneratorServiceV2();
    }

    [Fact]
    public async Task GenerateOptimizedPromptAsync_WithValidProblem_ReturnsVersion2()
    {
        // Arrange
        var problem = "How do I optimize database queries?";

        // Act
        var result = await _service.GenerateOptimizedPromptAsync(problem);

        // Assert
        Assert.Equal("Version2", result.optimizedPrompt);
    }

    [Theory]
    [InlineData("Test problem")]
    [InlineData("Another test")]
    [InlineData("Complex multi-line\nproblem statement")]
    public async Task GenerateOptimizedPromptAsync_WithAnyValidProblem_ReturnsVersion2(string problem)
    {
        // Act
        var result = await _service.GenerateOptimizedPromptAsync(problem);

        // Assert
        Assert.Equal("Version2", result.optimizedPrompt);
    }

    [Fact]
    public async Task GenerateOptimizedPromptAsync_WithEmptyProblem_ThrowsArgumentException()
    {
        // Arrange
        var problem = string.Empty;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateOptimizedPromptAsync(problem));
        
        Assert.Equal("problem", exception.ParamName);
        Assert.Contains("Problem cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GenerateOptimizedPromptAsync_WithWhitespaceProblem_ThrowsArgumentException()
    {
        // Arrange
        var problem = "   ";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateOptimizedPromptAsync(problem));
        
        Assert.Equal("problem", exception.ParamName);
    }

    [Fact]
    public async Task GenerateOptimizedPromptAsync_WithNullProblem_ThrowsArgumentException()
    {
        // Arrange
        string? problem = null;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GenerateOptimizedPromptAsync(problem!));
        
        Assert.Equal("problem", exception.ParamName);
    }
}
