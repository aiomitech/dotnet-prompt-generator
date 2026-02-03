namespace PromptGeneratorWebApi.Application.Interfaces;

public interface IPromptGeneratorService
{
    Task<(string analysis, string context, string optimizedPrompt)> GenerateOptimizedPromptAsync(string problem);
}
