namespace PromptGeneratorWebApi.Services;

public interface IPromptGeneratorService
{
    Task<(string analysis, string context, string optimizedPrompt)> GenerateOptimizedPromptAsync(string problem);
}
