using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PromptGeneratorWebApi.Services;
using Xunit;

namespace PromptGeneratorWebApi.Tests;

public class PromptGeneratorServiceTests
{
	private const string ApiKey = "test-api-key";

	[Fact]
	public void Constructor_Throws_WhenApiKeyMissing()
	{
		var configuration = new ConfigurationBuilder().Build();
		var client = new HttpClient(new TestHttpMessageHandler());

		var ex = Assert.Throws<InvalidOperationException>(
			() => new PromptGeneratorService(client, configuration));

		Assert.Contains("API key", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	public async Task GenerateOptimizedPromptAsync_Throws_WhenProblemEmpty(string problem)
	{
		var service = CreateServiceWithResponses(
			CreateJsonResponse("Analysis"));

		var ex = await Assert.ThrowsAsync<ArgumentException>(
			() => service.GenerateOptimizedPromptAsync(problem));

		Assert.Contains("Problem cannot be empty", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GenerateOptimizedPromptAsync_ReturnsExpectedValues_OnSuccess()
	{
		var service = CreateServiceWithResponses(
			CreateJsonResponse("Analysis"),
			CreateJsonResponse("Context"),
			CreateJsonResponse("Optimized Prompt"));

		var (analysis, context, optimizedPrompt) =
			await service.GenerateOptimizedPromptAsync("Test problem");

		Assert.Equal("Analysis", analysis);
		Assert.Equal("Context", context);
		Assert.Equal("Optimized Prompt", optimizedPrompt);
	}

	[Fact]
	public async Task GenerateOptimizedPromptAsync_ReturnsUnableToParse_WhenChoicesMissing()
	{
		var service = CreateServiceWithResponses(
			CreateJsonResponseWithoutChoices(),
			CreateJsonResponseWithoutChoices(),
			CreateJsonResponseWithoutChoices());

		var (_, _, optimizedPrompt) =
			await service.GenerateOptimizedPromptAsync("Test problem");

		Assert.Equal("Unable to parse response", optimizedPrompt);
	}

	[Fact]
	public async Task GenerateOptimizedPromptAsync_ReturnsNoResponseGenerated_WhenContentIsNull()
	{
		var service = CreateServiceWithResponses(
			CreateJsonResponseWithNullContent(),
			CreateJsonResponseWithNullContent(),
			CreateJsonResponseWithNullContent());

		var (_, _, optimizedPrompt) =
			await service.GenerateOptimizedPromptAsync("Test problem");

		Assert.Equal("No response generated", optimizedPrompt);
	}

	[Fact]
	public async Task GenerateOptimizedPromptAsync_Throws_WhenApiReturnsError()
	{
		var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
		{
			Content = new StringContent("{\"error\":\"unauthorized\"}")
		};
		var service = CreateServiceWithResponses(response);

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(
			() => service.GenerateOptimizedPromptAsync("Test problem"));

		Assert.Contains("OpenAI API Error", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public async Task GenerateOptimizedPromptAsync_Throws_WhenResponseIsInvalidJson()
	{
		var response = new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("not-json", Encoding.UTF8, "application/json")
		};
		var service = CreateServiceWithResponses(response);

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(
			() => service.GenerateOptimizedPromptAsync("Test problem"));

		Assert.Contains("Error processing request", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	private static PromptGeneratorService CreateServiceWithResponses(params HttpResponseMessage[] responses)
	{
		var handler = new TestHttpMessageHandler(responses);
		var client = new HttpClient(handler);
		var configuration = new ConfigurationBuilder()
			.AddInMemoryCollection(new Dictionary<string, string?>
			{
				["OpenAI:ApiKey"] = ApiKey
			})
			.Build();

		return new PromptGeneratorService(client, configuration);
	}

	private static HttpResponseMessage CreateJsonResponse(string content)
	{
		var response = new
		{
			choices = new[]
			{
				new
				{
					message = new
					{
						content
					}
				}
			}
		};
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(
				JsonSerializer.Serialize(response),
				Encoding.UTF8,
				"application/json")
		};
	}

	private static HttpResponseMessage CreateJsonResponseWithoutChoices()
	{
		var response = new { foo = "bar" };
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(
				JsonSerializer.Serialize(response),
				Encoding.UTF8,
				"application/json")
		};
	}

	private static HttpResponseMessage CreateJsonResponseWithoutContent()
	{
		var response = new
		{
			choices = new[]
			{
				new
				{
					message = new { }
				}
			}
		};
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(
				JsonSerializer.Serialize(response),
				Encoding.UTF8,
				"application/json")
		};
	}

	private static HttpResponseMessage CreateJsonResponseWithNullContent()
	{
		var response = new
		{
			choices = new[]
			{
				new
				{
					message = new
					{
						content = (string?)null
					}
				}
			}
		};
		return new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent(
				JsonSerializer.Serialize(response),
				Encoding.UTF8,
				"application/json")
		};
	}

	private sealed class TestHttpMessageHandler : HttpMessageHandler
	{
		private readonly Queue<HttpResponseMessage> _responses;

		public TestHttpMessageHandler(params HttpResponseMessage[] responses)
		{
			_responses = new Queue<HttpResponseMessage>(responses);
		}

		protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request,
			CancellationToken cancellationToken)
		{
			if (_responses.Count == 0)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent("{\"choices\":[{\"message\":{\"content\":\"fallback\"}}]}")
				});
			}

			return Task.FromResult(_responses.Dequeue());
		}
	}
}