using PromptGeneratorWebApi.Application.Interfaces;

namespace PromptGeneratorWebApi.Infrastructure.Services;

public class PromptGeneratorServiceV2 : IPromptGeneratorService
{
    public Task<(string analysis, string context, string optimizedPrompt)> GenerateOptimizedPromptAsync(string problem)
    {
        if (string.IsNullOrWhiteSpace(problem))
        {
            throw new ArgumentException("Problem cannot be empty", nameof(problem));
        }

        return Task.FromResult((string.Empty, string.Empty, "Version2"));
    }
}
