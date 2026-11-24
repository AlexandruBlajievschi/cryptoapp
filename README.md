
# CryptoApp

A cryptocurrency application with a .NET backend and frontend.

## Project Structure

- `backend/` - ASP.NET Core API
- `backend.Test/` - Backend unit tests, integration tests and HTTP test doubles
- `frontend/` - Frontend application
- `docs/` - Project documentation
- `docker-compose.yml` - Local PostgreSQL service and persistent volume

## Requirements

- .NET 11 SDK (11.0.100 or a newer patch in the same feature band)
- Node.js (if required by your frontend)
- Docker Desktop with Docker Compose and Linux containers (for local PostgreSQL)

## Local database

Follow [Local PostgreSQL setup](docs/Local_PostgreSQL.md) to configure the ignored
`.env`, start the database, configure the API connection string and apply EF Core
migrations. The initial schema contains lessons and glossary terms.

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
