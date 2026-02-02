# Changelog

## [1.2.0] - 2026-02-02

### Added
- Next.js web frontend (promptgenerator-web) for prompt generation
- Frontend setup instructions in the README

### Updated
- Documentation commands to PowerShell-friendly examples

## [1.1.0] - 2026-01-31

### Added
- Web API unit tests for `PromptGeneratorService`
- Web API integration tests for `/api/generate-prompt` and `/api/health`
- Updated documentation for Web API usage and testing

### Fixed
- Corrected documentation references to OpenAI integration to match HttpClient-based implementation

## [1.0.0] - 2026-01-31

### Added
- Initial release of .NET Console Prompt Generator
- Multi-stage prompt optimization pipeline (Analysis → Enrichment → Optimization)
- OpenAI API integration using HttpClient
- Problem analysis system with domain and complexity detection
- Context enrichment with best practices and pitfall identification
- Production-ready prompt generation
- User Secrets integration for secure credential management
- Comprehensive README with setup instructions
- Example usage documentation

### Technical Details
- Built with .NET 8.0
- Uses HttpClient + OpenAI Chat Completions API
- Implements three-stage prompt generation pipeline
- Console-based user interface

### Known Limitations
- Single prompt generation per session
- Manual copy-paste of generated prompt
- No persistent prompt history
- No multi-provider support yet
