namespace PromptGeneratorWebApi.Presentation.Models;

public class PromptRequest
{
    public string Problem { get; set; } = string.Empty;
}

public class PromptResponse
{
    public bool Success { get; set; }
    public string? OptimizedPrompt { get; set; }
    public PromptGenerationDetails? Details { get; set; }
    public string? Error { get; set; }
}

public class PromptGenerationDetails
{
    public string Analysis { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty;
}
