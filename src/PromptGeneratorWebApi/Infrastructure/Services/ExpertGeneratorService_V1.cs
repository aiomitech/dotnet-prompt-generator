using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PromptGeneratorWebApi.Infrastructure.Services;

public sealed class ExpertGeneratorService_V1
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    // Prefer Responses API for GPT-5.x (when available), but use Chat Completions as fallback
    private const string OpenAiResponsesUrl = "https://api.openai.com/v1/responses";
    private const string OpenAiChatUrl = "https://api.openai.com/v1/chat/completions";

    private readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public ExpertGeneratorService_V1(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;

        var apiKey = _configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Please set it in User Secrets.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <summary>
    /// Returns:
    /// - analysis: Step 1 canonical JSON (ExpertDesignResponse)
    /// - context:  Step 2 canonical JSON (MethodologyExecutionResponse)
    /// - optimizedPrompt: Step 3 canonical JSON (OptimizedPromptResponse)
    /// </summary>
    public async Task<(string analysis, string context, string optimizedPrompt)> GenerateOptimizedPromptAsync(string problem)
    {
        if (string.IsNullOrWhiteSpace(problem))
            throw new ArgumentException("Problem cannot be empty", nameof(problem));

        // Step 1: Persona Architect (Canonical JSON)
        var expertJson = await DesignExpertAsync(problem);
        var expert = DeserializeOrThrow<ExpertDesignResponse>(expertJson, "Step1 ExpertDesignResponse");

        // Step 2: Methodology Executor (Canonical JSON) - only if Step 1 ok
        string contextJson;
        if (!expert.TaskState.Equals("ok", StringComparison.OrdinalIgnoreCase))
        {
            // Return a blocked step2 result in canonical form
            var blocked = MethodologyExecutionResponse.Blocked(
                summary: "Cannot apply expertise because expert design did not complete successfully.",
                error: $"Step1 task_state={expert.TaskState}. Resolve Step1 first.",
                assumptions: expert.Assumptions
            );
            contextJson = JsonSerializer.Serialize(blocked, _jsonOpts);
        }
        else
        {
            contextJson = await ApplyExpertiseAsync(problem, expertJson);
        }

        var context = DeserializeOrThrow<MethodologyExecutionResponse>(contextJson, "Step2 MethodologyExecutionResponse");

        // Step 3: Prompt Optimizer (Canonical JSON)
        var optimizedPromptJson = await GenerateOptimizedPromptAsync(problem, expertJson, contextJson);

        return (expertJson, contextJson, optimizedPromptJson);
    }

    // -----------------------------
    // STEP 1: Persona Architect
    // -----------------------------
    private async Task<string> DesignExpertAsync(string problem)
    {
        var systemPrompt = CanonicalExpertDesignerSystemPrompt;

        var userPrompt = $"""
Design a hyper-specific expert for the following problem.

PROBLEM:
{problem}
""";

        return await GetStructuredJsonAsync(
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            jsonSchema: ExpertDesignJsonSchema,
            schemaName: "expert_design_response"
        );
    }

    // -----------------------------
    // STEP 2: Methodology Executor
    // -----------------------------
    private async Task<string> ApplyExpertiseAsync(string problem, string expertJson)
    {
        // Extract only expert_profile for chain-safety and token control
        var expert = DeserializeOrThrow<ExpertDesignResponse>(expertJson, "Step1 ExpertDesignResponse");
        var expertProfileJson = JsonSerializer.Serialize(expert.Output.ExpertProfile, _jsonOpts);

        var systemPrompt = CanonicalMethodologyExecutorSystemPrompt;

        var userPrompt = $"""
You will be given:
1) The problem
2) The expert profile (JSON)

Use ONLY the provided expert profile. Do not invent credentials or experience beyond it.

PROBLEM:
{problem}

EXPERT_PROFILE_JSON:
{expertProfileJson}
""";

        return await GetStructuredJsonAsync(
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            jsonSchema: MethodologyExecutionJsonSchema,
            schemaName: "methodology_execution_response"
        );
    }

    // -----------------------------
    // STEP 3: Prompt Optimizer
    // -----------------------------
    private async Task<string> GenerateOptimizedPromptAsync(string problem, string expertJson, string contextJson)
    {
        var systemPrompt = CanonicalPromptOptimizerSystemPrompt;

        // Keep these as JSON strings; downstream model is instructed to use them as canonical inputs
        var userPrompt = $"""
Create a production-ready prompt to paste into ChatGPT that solves the user's problem.

INPUTS (canonical JSON):
- expert_design_json: {expertJson}
- methodology_execution_json: {contextJson}

USER_PROBLEM:
{problem}
""";

        return await GetStructuredJsonAsync(
            systemPrompt: systemPrompt,
            userPrompt: userPrompt,
            jsonSchema: OptimizedPromptJsonSchema,
            schemaName: "optimized_prompt_response"
        );
    }

    // =========================================================
    // Core call: Chat Completions with strict JSON mode
    // =========================================================
    private async Task<string> GetStructuredJsonAsync(
        string systemPrompt,
        string userPrompt,
        object jsonSchema,
        string schemaName)
    {
        var model = _configuration["OpenAI:Model"] ?? "gpt-4o";

        var requestBody = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            response_format = new
            {
                type = "json_object"
            },
            temperature = 0.2,
            max_tokens = 4096
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOpts);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(OpenAiChatUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);

            // Chat Completions API: extract the text output from choices[0].message.content
            if (doc.RootElement.TryGetProperty("choices", out var choicesArr) && choicesArr.ValueKind == JsonValueKind.Array)
            {
                var firstChoice = choicesArr.EnumerateArray().FirstOrDefault();
                if (firstChoice.ValueKind != JsonValueKind.Undefined &&
                    firstChoice.TryGetProperty("message", out var messageEl) &&
                    messageEl.TryGetProperty("content", out var contentEl))
                {
                    var jsonText = contentEl.GetString();
                    if (!string.IsNullOrWhiteSpace(jsonText))
                    {
                        return jsonText;
                    }
                }
            }

            return "{}";
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

    private T DeserializeOrThrow<T>(string json, string label)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<T>(json, _jsonOpts);
            if (obj is null) throw new InvalidOperationException($"{label} deserialized as null.");
            return obj;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"{label} is not valid JSON for type {typeof(T).Name}. Raw:\n{json}", ex);
        }
    }

    // =========================================================
    // Prompts (System)
    // =========================================================

    private const string CanonicalExpertDesignerSystemPrompt = """
You are an expert Knowledge Graph Architect operating as a deterministic step in a multi-stage LLM pipeline.

Your task is to design a hyper-specific expert persona tailored precisely to the user’s stated problem.
The expert must be purpose-built for the problem — not a generic role.

## Expert Design Requirements
The expert MUST include:
1) Name and professional title
2) Educational background (degrees, certifications)
3) Years of experience and specific specialization
4) Methodology, framework, or operating model they use
5) Notable achievements or demonstrated track record
6) Core principles that guide their work

All attributes must be concrete, realistic, and internally consistent.
If key details are missing to specialize the expert, do NOT guess; set task_state="needs_clarification" and list missing items in assumptions.

## Output Contract (MANDATORY)
Return a single valid JSON object and NOTHING else. No markdown. No prose.
Do NOT reveal chain-of-thought.
Follow the provided JSON schema exactly.
""";

    private const string CanonicalMethodologyExecutorSystemPrompt = """
You are a deterministic step in a multi-stage LLM pipeline.

You will receive a problem and an expert_profile JSON object.
Assume the identity of that expert and apply ONLY the provided methodology to the provided problem.

## Requirements
Using the expert’s methodology, produce:
1) Initial assessment using the methodology/framework
2) Step-by-step recommendations
3) Specific tactics to implement
4) Metrics to track
5) Common pitfalls to avoid (based on the expert profile)

Do NOT invent new credentials, employers, publications, or experience beyond the provided expert_profile.

If the problem lacks details required to proceed, set task_state="needs_clarification" and list what is missing in assumptions.

## Output Contract (MANDATORY)
Return a single valid JSON object and NOTHING else. No markdown. No prose.
Do NOT reveal chain-of-thought.
Follow the provided JSON schema exactly.
""";

    private const string CanonicalPromptOptimizerSystemPrompt = """
You are a deterministic step in a multi-stage LLM pipeline.

Your task is to produce an optimized, production-ready prompt that the user can paste into ChatGPT to solve their problem.
You will be given canonical JSON outputs from prior steps. Treat them as source-of-truth inputs.

## Requirements for the optimized prompt
- Be specific and unambiguous
- Include relevant context (only what helps)
- Specify desired output format clearly
- Include constraints/requirements
- Include examples only if they materially improve correctness
- Be actionable and copy-paste ready

If the inputs indicate missing information, set task_state="needs_clarification" and include the clarification questions in output.clarifying_questions.

## Output Contract (MANDATORY)
Return a single valid JSON object and NOTHING else. No markdown. No prose.
Do NOT reveal chain-of-thought.
Follow the provided JSON schema exactly.
""";

    // =========================================================
    // JSON Schemas (strict)
    // NOTE: keep these stable; bump schema_version when changing shapes
    // =========================================================

    private static readonly object ExpertDesignJsonSchema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "schema_version", "task_state", "summary", "output", "assumptions", "next_actions", "warnings", "errors" },
        properties = new
        {
            schema_version = new { type = "string" },
            task_state = new { type = "string", @enum = new[] { "ok", "needs_clarification", "blocked" } },
            summary = new { type = "string" },
            output = new
            {
                type = "object",
                additionalProperties = false,
                required = new[] { "expert_profile" },
                properties = new
                {
                    expert_profile = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[] { "name", "title", "education", "experience", "methodology", "notable_achievements", "guiding_principles" },
                        properties = new
                        {
                            name = new { type = "string" },
                            title = new { type = "string" },
                            education = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    additionalProperties = false,
                                    required = new[] { "degree_or_certification", "institution" },
                                    properties = new
                                    {
                                        degree_or_certification = new { type = "string" },
                                        institution = new { type = "string" }
                                    }
                                }
                            },
                            experience = new
                            {
                                type = "object",
                                additionalProperties = false,
                                required = new[] { "years", "specialization" },
                                properties = new
                                {
                                    years = new { type = "number" },
                                    specialization = new { type = "string" }
                                }
                            },
                            methodology = new { type = "string" },
                            notable_achievements = new { type = "array", items = new { type = "string" } },
                            guiding_principles = new { type = "array", items = new { type = "string" } }
                        }
                    }
                }
            },
            assumptions = new { type = "array", items = new { type = "string" } },
            next_actions = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "action", "priority", "parameters" },
                    properties = new
                    {
                        action = new { type = "string" },
                        priority = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                        parameters = new { type = "object" }
                    }
                }
            },
            warnings = new { type = "array", items = new { type = "string" } },
            errors = new { type = "array", items = new { type = "string" } }
        }
    };

    private static readonly object MethodologyExecutionJsonSchema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "schema_version", "task_state", "summary", "output", "assumptions", "next_actions", "warnings", "errors" },
        properties = new
        {
            schema_version = new { type = "string" },
            task_state = new { type = "string", @enum = new[] { "ok", "needs_clarification", "blocked" } },
            summary = new { type = "string" },
            output = new
            {
                type = "object",
                additionalProperties = false,
                required = new[] { "analysis" },
                properties = new
                {
                    analysis = new
                    {
                        type = "object",
                        additionalProperties = false,
                        required = new[]
                        {
                            "initial_assessment",
                            "step_by_step_recommendations",
                            "tactics",
                            "metrics",
                            "pitfalls_to_avoid"
                        },
                        properties = new
                        {
                            initial_assessment = new { type = "string" },
                            step_by_step_recommendations = new { type = "array", items = new { type = "string" } },
                            tactics = new { type = "array", items = new { type = "string" } },
                            metrics = new { type = "array", items = new { type = "string" } },
                            pitfalls_to_avoid = new { type = "array", items = new { type = "string" } }
                        }
                    }
                }
            },
            assumptions = new { type = "array", items = new { type = "string" } },
            next_actions = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "action", "priority", "parameters" },
                    properties = new
                    {
                        action = new { type = "string" },
                        priority = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                        parameters = new { type = "object" }
                    }
                }
            },
            warnings = new { type = "array", items = new { type = "string" } },
            errors = new { type = "array", items = new { type = "string" } }
        }
    };

    private static readonly object OptimizedPromptJsonSchema = new
    {
        type = "object",
        additionalProperties = false,
        required = new[] { "schema_version", "task_state", "summary", "output", "assumptions", "next_actions", "warnings", "errors" },
        properties = new
        {
            schema_version = new { type = "string" },
            task_state = new { type = "string", @enum = new[] { "ok", "needs_clarification", "blocked" } },
            summary = new { type = "string" },
            output = new
            {
                type = "object",
                additionalProperties = false,
                required = new[] { "optimized_prompt", "clarifying_questions", "recommended_system_prompt" },
                properties = new
                {
                    optimized_prompt = new { type = "string" },
                    recommended_system_prompt = new { type = "string" },
                    clarifying_questions = new { type = "array", items = new { type = "string" } }
                }
            },
            assumptions = new { type = "array", items = new { type = "string" } },
            next_actions = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    additionalProperties = false,
                    required = new[] { "action", "priority", "parameters" },
                    properties = new
                    {
                        action = new { type = "string" },
                        priority = new { type = "string", @enum = new[] { "low", "medium", "high" } },
                        parameters = new { type = "object" }
                    }
                }
            },
            warnings = new { type = "array", items = new { type = "string" } },
            errors = new { type = "array", items = new { type = "string" } }
        }
    };

    // =========================================================
    // Strongly typed models
    // =========================================================

    private sealed record ExpertDesignResponse
    {
        [JsonPropertyName("schema_version")] public string SchemaVersion { get; init; } = "1.0.0";
        [JsonPropertyName("task_state")] public string TaskState { get; init; } = "ok";
        [JsonPropertyName("summary")] public string Summary { get; init; } = "";
        [JsonPropertyName("output")] public ExpertDesignOutput Output { get; init; } = new();
        [JsonPropertyName("assumptions")] public List<string> Assumptions { get; init; } = new();
        [JsonPropertyName("next_actions")] public List<NextAction> NextActions { get; init; } = new();
        [JsonPropertyName("warnings")] public List<string> Warnings { get; init; } = new();
        [JsonPropertyName("errors")] public List<string> Errors { get; init; } = new();
    }

    private sealed record ExpertDesignOutput
    {
        [JsonPropertyName("expert_profile")] public ExpertProfile ExpertProfile { get; init; } = new();
    }

    private sealed record ExpertProfile
    {
        [JsonPropertyName("name")] public string Name { get; init; } = "";
        [JsonPropertyName("title")] public string Title { get; init; } = "";
        [JsonPropertyName("education")] public List<EducationItem> Education { get; init; } = new();
        [JsonPropertyName("experience")] public Experience Experience { get; init; } = new();
        [JsonPropertyName("methodology")] public string Methodology { get; init; } = "";
        [JsonPropertyName("notable_achievements")] public List<string> NotableAchievements { get; init; } = new();
        [JsonPropertyName("guiding_principles")] public List<string> GuidingPrinciples { get; init; } = new();
    }

    private sealed record EducationItem
    {
        [JsonPropertyName("degree_or_certification")] public string DegreeOrCertification { get; init; } = "";
        [JsonPropertyName("institution")] public string Institution { get; init; } = "";
    }

    private sealed record Experience
    {
        [JsonPropertyName("years")] public decimal Years { get; init; }
        [JsonPropertyName("specialization")] public string Specialization { get; init; } = "";
    }

    private sealed record MethodologyExecutionResponse
    {
        [JsonPropertyName("schema_version")] public string SchemaVersion { get; init; } = "1.0.0";
        [JsonPropertyName("task_state")] public string TaskState { get; init; } = "ok";
        [JsonPropertyName("summary")] public string Summary { get; init; } = "";
        [JsonPropertyName("output")] public MethodologyExecutionOutput Output { get; init; } = new();
        [JsonPropertyName("assumptions")] public List<string> Assumptions { get; init; } = new();
        [JsonPropertyName("next_actions")] public List<NextAction> NextActions { get; init; } = new();
        [JsonPropertyName("warnings")] public List<string> Warnings { get; init; } = new();
        [JsonPropertyName("errors")] public List<string> Errors { get; init; } = new();

        public static MethodologyExecutionResponse Blocked(string summary, string error, List<string>? assumptions = null) => new()
        {
            TaskState = "blocked",
            Summary = summary,
            Errors = new List<string> { error },
            Assumptions = assumptions ?? new List<string>()
        };
    }

    private sealed record MethodologyExecutionOutput
    {
        [JsonPropertyName("analysis")] public MethodologyAnalysis Analysis { get; init; } = new();
    }

    private sealed record MethodologyAnalysis
    {
        [JsonPropertyName("initial_assessment")] public string InitialAssessment { get; init; } = "";
        [JsonPropertyName("step_by_step_recommendations")] public List<string> StepByStepRecommendations { get; init; } = new();
        [JsonPropertyName("tactics")] public List<string> Tactics { get; init; } = new();
        [JsonPropertyName("metrics")] public List<string> Metrics { get; init; } = new();
        [JsonPropertyName("pitfalls_to_avoid")] public List<string> PitfallsToAvoid { get; init; } = new();
    }

    private sealed record NextAction
    {
        [JsonPropertyName("action")] public string Action { get; init; } = "";
        [JsonPropertyName("priority")] public string Priority { get; init; } = "medium";
        [JsonPropertyName("parameters")] public Dictionary<string, object> Parameters { get; init; } = new();
    }
}
