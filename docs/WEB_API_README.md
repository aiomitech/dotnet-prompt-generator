# Prompt Generator Web API

A production-ready .NET 8.0 Minimal API that transforms user problems into optimized ChatGPT prompts through a multi-stage AI pipeline using OpenAI's GPT-3.5-turbo model.

## Features

- **Multi-Stage Prompt Optimization Pipeline**:
  1. **Analysis Stage**: Identifies core problem, domain, complexity, and missing information
  2. **Context Enrichment Stage**: Gathers relevant background, best practices, and common pitfalls
  3. **Optimization Stage**: Generates production-ready, optimized ChatGPT prompts

- **RESTful API** with Swagger/OpenAPI documentation
- **Dependency Injection** architecture
- **User Secrets** for secure API key management
- **CORS** enabled for cross-origin requests
- **Health Check** endpoint for monitoring

## Project Structure

```
src/
├── PromptGenerator/              # Console application version
└── PromptGeneratorWebApi/        # Web API version
    ├── Models/
    │   └── PromptModels.cs       # Request/Response DTOs
    ├── Services/
    │   └── PromptGeneratorService.cs  # Core business logic
    ├── Program.cs                 # API configuration and endpoints
    ├── appsettings.json
    └── PromptGeneratorWebApi.csproj
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- OpenAI API Key ([Get one here](https://platform.openai.com/api-keys))

## Setup

### 1. Clone the Repository

```powershell
git clone https://github.com/aiomitech/dotnet-console-prompt-generator.git
cd dotnet-console-prompt-generator
```

### 2. Configure OpenAI API Key

Navigate to the Web API project directory:

```powershell
cd src/PromptGeneratorWebApi
```

Set your OpenAI API key using User Secrets:

```powershell
dotnet user-secrets set "OpenAI:ApiKey" "your-actual-openai-api-key"
```

### 3. Build the Project

```powershell
dotnet build
```

### 4. Run the API

```powershell
dotnet run
```

The API will start on:
- **HTTP**: `http://localhost:5000`
- **HTTPS** (if configured): `https://localhost:5001`

### 5. Access Swagger UI

Once the API is running, open your browser and navigate to:

```
http://localhost:5000/swagger
```

## API Endpoints

### Generate Optimized Prompt

**Endpoint**: `POST /api/generate-prompt`

**Request Body**:
```json
{
  "problem": "I need to build a REST API with authentication"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "optimizedPrompt": "...[optimized prompt]...",
  "details": {
    "analysis": "...[problem analysis]...",
    "context": "...[context enrichment]..."
  }
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "error": "Problem cannot be empty"
}
```

**Error Response** (500 Internal Server Error):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
  "title": "Error generating prompt",
  "status": 500,
  "detail": "OpenAI API Error: ..."
}
```

### Health Check

**Endpoint**: `GET /api/health`

**Response**:
```json
{
  "status": "healthy",
  "timestamp": "2026-01-31T12:34:56.789Z"
}
```

## Testing with PowerShell

```powershell
# Generate optimized prompt
$body = @{
  problem = "I need to build a REST API with authentication"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/generate-prompt" `
  -Method Post `
  -Body $body `
  -ContentType "application/json"

# Health check
Invoke-RestMethod -Uri "http://localhost:5000/api/health"
```

## Architecture

### Dependency Injection

The API uses .NET's built-in dependency injection:

- `IPromptGeneratorService`: Interface for the core prompt generation service
- `HttpClient`: Injected via `AddHttpClient<T>` for OpenAI API communication
- `IConfiguration`: Provides access to User Secrets and configuration

### Service Layer

`PromptGeneratorService` implements the three-stage pipeline:

1. **AnalyzeProblemAsync**: Analyzes the user's problem using a specialized system prompt
2. **EnrichContextAsync**: Gathers relevant context based on the analysis
3. **GenerateOptimizedPromptAsync**: Creates the final optimized prompt

Each stage makes an independent call to OpenAI's Chat Completions API.

### Error Handling

- **Validation**: Empty or null problems return 400 Bad Request
- **API Errors**: OpenAI API errors are caught and returned as 500 Internal Server Error
- **Exception Details**: Detailed error messages in development mode

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### User Secrets (Development)

```json
{
  "OpenAI:ApiKey": "sk-..."
}
```

## Dependencies

- **Microsoft.AspNetCore.OpenApi** (8.0.0): OpenAPI support
- **Swashbuckle.AspNetCore** (6.5.0): Swagger UI
- **System.Net.Http.Json** (8.0.0): HTTP JSON operations

## Development

### Running in Development Mode

```powershell
dotnet run --environment Development
```

### Building for Production

```powershell
dotnet publish -c Release -o ./publish
```

### Docker Support (Optional)

Create a `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/PromptGeneratorWebApi/PromptGeneratorWebApi.csproj", "src/PromptGeneratorWebApi/"]
RUN dotnet restore "src/PromptGeneratorWebApi/PromptGeneratorWebApi.csproj"
COPY . .
WORKDIR "/src/src/PromptGeneratorWebApi"
RUN dotnet build "PromptGeneratorWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PromptGeneratorWebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PromptGeneratorWebApi.dll"]
```

Build and run:

```powershell
docker build -t prompt-generator-api .
docker run -p 5000:80 -e OpenAI__ApiKey="your-key" prompt-generator-api
```

## Security Considerations

1. **Never commit API keys** to version control
2. **Use User Secrets** for local development
3. **Use Environment Variables** or Azure Key Vault for production
4. **Enable HTTPS** in production environments
5. **Implement rate limiting** for public APIs
6. **Add authentication/authorization** if exposing publicly

## Performance

- Each prompt generation requires **3 sequential API calls** to OpenAI
- Average latency: **3-5 seconds** (depends on OpenAI response times)
- Consider implementing **caching** for repeated problems
- Use **async/await** throughout for non-blocking I/O

## Troubleshooting

### "OpenAI API key is not configured"

Make sure you've set the API key in User Secrets:

```powershell
cd src/PromptGeneratorWebApi
dotnet user-secrets set "OpenAI:ApiKey" "your-key"
```

### "Unable to resolve service for type 'IPromptGeneratorService'"

Ensure the service is registered in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IPromptGeneratorService, PromptGeneratorService>();
```

### HTTPS Certificate Errors (Development)

Trust the development certificate:

```powershell
dotnet dev-certs https --trust
```

## Testing

```powershell
dotnet test tests/PromptGeneratorWebApi.Tests/
```

This project includes:
- Unit tests for `PromptGeneratorService`
- Integration tests for `/api/generate-prompt` and `/api/health`

## Future Enhancements

- [ ] Add authentication/authorization (JWT, API Keys)
- [ ] Implement rate limiting
- [ ] Add caching layer for repeated problems
- [ ] Support for multiple AI providers (Anthropic Claude, etc.)
- [ ] Batch processing endpoint
- [ ] WebSocket support for streaming responses
- [ ] Telemetry and Application Insights integration
-- [ ] Add rate limiting metrics and structured logging

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please open an issue or submit a pull request.

## Repository

[https://github.com/aiomitech/dotnet-console-prompt-generator](https://github.com/aiomitech/dotnet-console-prompt-generator)

---

**Note**: This Web API version complements the console application version in `src/PromptGenerator`. Both share the same core logic but are packaged differently for different use cases.
