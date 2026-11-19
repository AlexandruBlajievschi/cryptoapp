## Backend development

Requires .NET 11 SDK. Run these commands from the repository root.

### Run the API

```powershell
dotnet run --project backend/backend.csproj --launch-profile http
```

Health endpoint: http://localhost:5123/api/health

Expected response: HTTP 200 with `{"status":"healthy"}`.

### Run backend tests

```powershell
dotnet test backend.Test/backend.Test.csproj
```

Integration tests start the API in a test host; you do not need
to run the API separately.

### Local secrets

No secrets are required for the current health endpoint.

When database or external-provider credentials are introduced,
store them using ASP.NET Core user-secrets. Never commit actual
credentials to configuration files.

User-secrets is intended for local development and is not encrypted.
