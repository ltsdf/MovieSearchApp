# MovieSearchApp
OMDb-powered movie search built with .NET 8 and Blazor.

![MovieSearchApp demo](Docs/Media/demo.gif)

A small example app that demonstrates:
- Searching movies by title (proxying OMDb)
- Paged search results
- Viewing extended details (poster, plot, IMDb rating, etc.)
- Preserving the 5 latest unique search queries (newest first)
- Unit tests for client, state, and UI

## Table of contents
- Features
- Architecture
- Prerequisites
- Configuration
- Run locally
- Testing
- Optional: Swagger
- CI (GitHub Actions)
- Notes

## Features
- Search movies by title through a server-side proxy to OMDb
- Paged search results with simple paging UI
- Detailed movie view (poster, plot, ratings, metadata)
- Client-side state: saves last 5 unique searches (newest first)
- Unit/integration tests covering client, state, and UI layers

## Architecture
- Backend: .NET 8 Minimal API with a typed HttpClient for OMDb
- Frontend: Blazor (Server or WASM depending on the repository variant)
- Abstractions: IOmdbClient + DTOs (PagedResult<T>, MovieSearchItem, MovieDetailsDto)
- Configuration: OmdbOptions bound via configuration. API key is kept server-side only.

## Prerequisites
- .NET 8 SDK installed (https://dotnet.microsoft.com/)
- OMDb API key (free: https://www.omdbapi.com/apikey.aspx)

## Tech decisions (short)
- Minimal APIs
	- Small surface, fast startup, and straightforward endpoint mapping; ideal for a thin proxy over OMDb.
- Typed `HttpClient` with Polly
	- Strongly-typed client improves testability and DI integration.
	- Polly adds resiliency (timeouts, limited retries with jitter, optional circuit breaker) to handle transient OMDb/network issues and rate limits gracefully.
- Debounce search input
	- Prevents spamming requests while the user types; reduces noise and load on OMDb, improves UX by firing after a short pause.

## Configuration
You can supply the OMDb API key either via appsettings.Development.json (recommended for local development) or environment variables.

Example appsettings.Development.json:
```json
{
 "Omdb": {
 "ApiKey": "YOUR_OMDB_API_KEY",
 "BaseUrl": "https://www.omdbapi.com/"
 }
}
```

Environment variables (alternative):
- Omdb__ApiKey (required)
- Omdb__BaseUrl (optional — default: https://www.omdbapi.com/)

Important: keep your OMDb API key secret. Do not commit real keys to source control.

## Run locally
From the repository root:

1. Restore and build:
```bash
dotnet restore
dotnet build
```

2. Run the API/project:
```bash
dotnet run --project src/MovieSearchApp
```

The app prints the listening URL(s) to the console (typically https://localhost:5001 and/or http://localhost:5000). Open the URL in your browser.

If the repo variant includes a Blazor client project, run that project or use the provided launch configuration — see the project README or the .sln for details.

## Swagger (optional)
If Swashbuckle (OpenAPI) is enabled in the project, a UI will be available at:
```
/swagger
```
(e.g. https://localhost:5001/swagger)

## Testing
Run all tests:
```bash
dotnet test
```

Client/UI tests (bUnit) — install test dependencies in the test project folder if you need to add bUnit tests:
```bash
dotnet add package Bunit --version 1.*
dotnet add package Microsoft.Extensions.Http --version 8.*
```

If you prefer Moq for mocking:
```bash
dotnet add package Moq
```
(Existing samples may use simple inline fakes instead of Moq.)

## CI (GitHub Actions)
A sample workflow to restore, build, and test on Ubuntu with .NET 8 — save as `.github/workflows/dotnet.yml`:

```yaml
name: .NET

on:
  push:
    branches: [ "main", "master" ]
  pull_request:
    branches: [ "main", "master" ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Restore
        run: dotnet restore
      - name: Build
        run: dotnet build --no-restore --configuration Release
      - name: Test
        run: dotnet test --no-build --verbosity normal
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results
          path: '**/TestResults/**'
```

Notes:
- For private API keys in CI, use repository secrets and set them as environment variables for the job when needed.
- Adjust the workflow to run matrix builds or to publish artifacts if you need that.

## Notes & troubleshooting
- If you see OMDb errors, ensure API key is valid and not rate-limited.
- The backend intentionally keeps the OMDb API key server-side. The frontend calls your API endpoints — not OMDb directly.
- If using Blazor WASM, ensure the backend is reachable or adapt the configuration for CORS.

---

If anything in your repository layout differs from these instructions (different project name or path), let me know the actual path/name and I will update the README accordingly.
