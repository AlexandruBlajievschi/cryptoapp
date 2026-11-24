
# CryptoApp

A cryptocurrency application with a .NET backend and frontend.

## Project Structure

- `backend/` - ASP.NET Core API
- `backend.Test/` - Backend unit tests, integration tests and HTTP test doubles
- `frontend/` - Frontend application
- `docs/` - Project documentation

## Requirements

- .NET 11 SDK (11.0.100 or a newer patch in the same feature band)
- Node.js (if required by your frontend)

## Backend

See the [backend development guide](backend/README.md) for API routes,
architecture, validation, error handling and test instructions.
Manual API requests are in [docs/api](docs/api).

Navigate to the backend project:

```bash
cd backend
```

Restore dependencies:

```bash
dotnet restore
```

Run the application:

```bash
dotnet run
```

## Frontend

Navigate to the frontend:

```bash
cd frontend
```

Install dependencies:

```bash
npm install
```

Start the development server:

```bash
npm run dev
```
