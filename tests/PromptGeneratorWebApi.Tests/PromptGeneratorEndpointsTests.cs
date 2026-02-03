using System.Text;
using System.Text.Json;
using Moq;
using PromptGeneratorWebApi.Presentation.Models;
using Xunit;

namespace PromptGeneratorWebApi.Tests;

/// <summary>
/// Integration tests for the PromptGeneratorWebApi endpoints.
/// Tests the /api/v1/generate-prompt endpoint with various scenarios.
/// </summary>
public class PromptGeneratorEndpointsTests : IDisposable
{
	private readonly WebApiTestFactory _factory;
	private readonly HttpClient _client;

	public PromptGeneratorEndpointsTests()
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
	public async Task GeneratePrompt_WithValidInput_ReturnsSuccessResponse()
	{
		// Arrange
		var request = new PromptRequest { Problem = "How do I optimize database queries?" };
		var expectedAnalysis = "Database optimization involves indexing and query planning";
		var expectedContext = "Modern databases support various optimization techniques";
		var expectedPrompt = "Create a detailed guide on database optimization";

		_factory.MockPromptGeneratorService
			.Setup(s => s.GenerateOptimizedPromptAsync(request.Problem))
			.ReturnsAsync((expectedAnalysis, expectedContext, expectedPrompt));

		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.True(response.IsSuccessStatusCode);
		var responseContent = await response.Content.ReadAsStringAsync();
		var result = JsonSerializer.Deserialize<PromptResponse>(
			responseContent,
			new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

		Assert.NotNull(result);
		Assert.True(result.Success);
		Assert.NotNull(result.Details);
		Assert.Equal(expectedAnalysis, result.Details.Analysis);
		Assert.Equal(expectedContext, result.Details.Context);
		Assert.Equal(expectedPrompt, result.OptimizedPrompt);
	}

	[Fact]
	public async Task GeneratePrompt_WithEmptyProblem_ReturnsBadRequest()
	{
		// Arrange
		var request = new PromptRequest { Problem = string.Empty };
		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

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
	public async Task GeneratePrompt_WithWhitespaceOnlyProblem_ReturnsBadRequest()
	{
		// Arrange
		var request = new PromptRequest { Problem = "   " };
		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Fact]
	public async Task GeneratePrompt_WithNullProblem_ReturnsBadRequest()
	{
		// Arrange
		var request = new { Problem = (string?)null };
		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
	}

	[Theory]
	[InlineData("What is artificial intelligence?")]
	[InlineData("How do I debug C# code?")]
	[InlineData("Explain machine learning algorithms")]
	public async Task GeneratePrompt_WithVariousValidInputs_SucceedsForAll(string problem)
	{
		// Arrange
		var request = new PromptRequest { Problem = problem };
		_factory.MockPromptGeneratorService
			.Setup(s => s.GenerateOptimizedPromptAsync(It.IsAny<string>()))
			.ReturnsAsync(("Analysis", "Context", "Optimized Prompt"));

		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.True(response.IsSuccessStatusCode);
	}

	[Fact]
	public async Task GeneratePrompt_WhenServiceThrowsException_ReturnsInternalServerError()
	{
		// Arrange
		var request = new PromptRequest { Problem = "Test problem" };
		_factory.MockPromptGeneratorService
			.Setup(s => s.GenerateOptimizedPromptAsync(It.IsAny<string>()))
			.ThrowsAsync(new InvalidOperationException("Service error"));

		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
	}

	[Fact]
	public async Task GeneratePrompt_ResponseHasCorrectContentType()
	{
		// Arrange
		var request = new PromptRequest { Problem = "Test" };
		_factory.MockPromptGeneratorService
			.Setup(s => s.GenerateOptimizedPromptAsync(It.IsAny<string>()))
			.ReturnsAsync(("Analysis", "Context", "Prompt"));

		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		var response = await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		Assert.NotNull(response.Content.Headers.ContentType);
		Assert.Contains("application/json", response.Content.Headers.ContentType.ToString());
	}

	[Fact]
	public async Task GeneratePrompt_VerifiesServiceIsCalled()
	{
		// Arrange
		var testProblem = "Test problem for verification";
		var request = new PromptRequest { Problem = testProblem };
		_factory.MockPromptGeneratorService
			.Setup(s => s.GenerateOptimizedPromptAsync(testProblem))
			.ReturnsAsync(("Analysis", "Context", "Prompt"));

		var content = new StringContent(
			JsonSerializer.Serialize(request),
			Encoding.UTF8,
			"application/json");

		// Act
		await _client.PostAsync("/api/v1/generate-prompt", content);

		// Assert
		_factory.MockPromptGeneratorService.Verify(
			s => s.GenerateOptimizedPromptAsync(testProblem),
			Times.Once);
	}
}