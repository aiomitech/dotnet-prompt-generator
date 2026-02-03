using System.Text;
using System.Text.Json;

namespace PromptGeneratorWebApi.Services;

public class PromptGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private const string OpenAiUrl = "https://api.openai.com/v1/chat/completions";

    public PromptGeneratorService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("OpenAI API key is not configured. Please set it in User Secrets.");
        }
        
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public async Task<(string analysis, string context, string optimizedPrompt)> GenerateOptimizedPromptAsync(string problem)
    {
        if (string.IsNullOrWhiteSpace(problem))
        {
            throw new ArgumentException("Problem cannot be empty", nameof(problem));
        }

        // Step 1: Analyze the problem
        var analysis = await AnalyzeProblemAsync(problem);

        // Step 2: Generate context enrichment
        var context = await EnrichContextAsync(problem, analysis);

        // Step 3: Generate optimized prompt
        var optimizedPrompt = await GenerateOptimizedPromptAsync(problem, analysis, context);

        return (analysis, context, optimizedPrompt);
    }

    private async Task<string> AnalyzeProblemAsync(string problem)
    {
        var systemPrompt = @"You are an expert prompt engineer. Your task is to analyze user problems and identify:
                                1. The core problem or question
                                2. The domain/category (technical, creative, analytical, etc.)
                                3. The complexity level
                                4. Any missing information that might be needed
                                5. The likely intent or goal

                                Be concise and bullet-pointed in your response.";

        return await GetCompletionAsync(systemPrompt, $"Analyze this problem: {problem}");
    }

    private async Task<string> EnrichContextAsync(string problem, string analysis)
    {
        var systemPrompt = @"You are an expert at gathering context. Given a problem and its analysis, provide:
                                1. Relevant background information that would be helpful
                                2. Assumptions to clarify
                                3. Best practices in the relevant domain
                                4. Common pitfalls to avoid

                                Be practical and actionable.";

        return await GetCompletionAsync(systemPrompt, $@"Problem: {problem}

Analysis: {analysis}");
    }

    private async Task<string> GenerateOptimizedPromptAsync(string problem, string analysis, string context)
    {
        var systemPrompt = @"You are an expert prompt engineer specializing in creating clear, effective prompts for AI systems like ChatGPT.
                                Your task is to transform a user's problem statement into a highly effective prompt that will:
                                1. Be specific and unambiguous
                                2. Include relevant context
                                3. Specify the desired output format
                                4. Provide examples when helpful
                                5. Include any constraints or requirements
                                6. Be actionable and ready to copy-paste

                                Create a prompt that's professional and comprehensive.";

        return await GetCompletionAsync(systemPrompt, $@"Original Problem: {problem}
                                Analysis: {analysis}
                                Context: {context}
                                Please generate an optimized, production-ready prompt that I can copy and paste into ChatGPT.");
    }

    private async Task<string> GetCompletionAsync(string systemPrompt, string userMessage)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            temperature = 0.7,
            max_tokens = 2048
        };

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(OpenAiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(responseString);
            var root = jsonDoc.RootElement;

            if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var message) && 
                    message.TryGetProperty("content", out var result))
                {
                    return result.GetString() ?? "No response generated";
                }
            }

            return "Unable to parse response";
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"OpenAI API Error: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error processing request: {ex.Message}", ex);
        }
    }
}
