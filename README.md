# BookCatalog API

A modular, high-performance web service built with **.NET 10** and **C#**, designed for managing books, authors, users, and book lending operations. Architected around **Clean Architecture**, **CQRS with MediatR**, and **Domain-Driven Design (DDD)** principles.

---

## Architecture Overview

- **`BookCatalog.Domain`**: Core entities (`Book`, `Author`, `User`, `Loan`), value objects, and domain exceptions.
- **`BookCatalog.Application`**: CQRS Commands/Queries, MediatR handlers, FluentValidation rules, pipeline behaviors (Logging, Validation, Transactions), and AutoMapper profiles.
- **`BookCatalog.Infrastructure`**: EF Core persistence with PostgreSQL, repository implementations, concurrency conflict interpreters, and retrying execution strategies.
- **`BookCatalog.Api`**: RESTful API controllers, exception filters, Serilog structured logging, health checks, and startup configuration validation.

---

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for containerized PostgreSQL and integration tests)
- dotnet-ef CLI Tool
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Configuration & Environment Setup

### 1. Environment Variables (`.env`)
Create a `.env` file in the project root directory using the template:

```bash
cp .env.example .env
```

#### `.env` File Example:
```env
# ASP.NET Core Environment (Development, Production)
ASPNETCORE_ENVIRONMENT=Development

# PostgreSQL Database Configuration
POSTGRES_DATABASE=postgres_books
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password_here
POSTGRES_INTERNAL_PORT=5432

# MediatR License Key (Optional for local testing / development)
MEDIATR_LICENSE=
```

### 2. Configuration Files Explained

The application reads configuration hierarchically from `appsettings.json`, `appsettings.{Environment}.json`, and environment variables:

- **`ConnectionStrings:DbConnection`**: The PostgreSQL connection string. The application enforces **fail-fast startup validation** via `DatabaseOptions`—if this connection string is missing or empty, the application will immediately refuse to start with clear error diagnostics.
- **`Serilog`**: Configures structured log levels, console sinks, and daily rolling file sinks saved to `logs/bookcatalog-.log`. All log events are enriched with W3C `TraceId` for end-to-end request traceability.
- **`MediatRLicense`**: License key for MediatR (optional during local development and testing).

---

## Quick Start / Launching the API

### Option A: Run Everything via Docker Compose (Recommended)

To build and run both the API and the PostgreSQL database in containers:

```bash
# Start all containers in the background
docker compose --env-file .env up -d --build

# View container logs
docker compose logs -f
```

The API will be available at `http://localhost:8080` (Swagger at `http://localhost:8080/swagger`).

---

### Option B: Run API Locally with Dockerized PostgreSQL

If you prefer debugging and running the .NET API directly from your IDE (Rider / Visual Studio / VS Code):

#### 1. Start the PostgreSQL Container
```bash
docker compose --env-file .env up -d books.db
```

#### 2. Apply Database Migrations
```bash
dotnet ef database update --project src/BookCatalog.Infrastructure --startup-project src/BookCatalog.Api
```

#### 3. Run the API
```bash
dotnet run --project src/BookCatalog.Api
```

---

## Database Management & Migrations

All migrations reside in `BookCatalog.Infrastructure` and target PostgreSQL.

### Apply Pending Migrations
```bash
dotnet ef database update --project src/BookCatalog.Infrastructure --startup-project src/BookCatalog.Api
```

### Add a New Migration
```bash
dotnet ef migrations add <MigrationName> --project src/BookCatalog.Infrastructure --startup-project src/BookCatalog.Api
```

### Rollback / Remove the Last Migration
```bash
dotnet ef migrations remove --project src/BookCatalog.Infrastructure --startup-project src/BookCatalog.Api
```

---

## Testing

The solution includes a comprehensive suite of unit and integration tests powered by **NUnit 3**, **Moq**, and **Testcontainers**.

### Run All Tests
```bash
dotnet test
```

### Test Suite Highlights:
- **Zero-Setup Real DB Testing**: Integration tests automatically spin up an ephemeral PostgreSQL container via Docker (`Testcontainers.PostgreSql`) matching production configuration.
- **State Isolation & Garbage Cleanup**: Every integration test automatically resets database tables before execution (`IntegrationTestBase`), guaranteeing test repeatability and leaving no residual state behind.
- **Coverage**: Covers CQRS handlers, FluentValidation rules, pipeline behaviors, concurrency conflicts, graceful shutdown, fail-fast configuration validation, and 100% of controller endpoints.

---

## Key Endpoints & Observability

| Endpoint | Method | Description |
|---|---|---|
| `/swagger` | `GET` | Interactive OpenAPI / Swagger UI (in Development) |
| `/health/live` | `GET` | Liveness check (verifies process is running) |
| `/health/ready` | `GET` | Readiness check (probes PostgreSQL database connectivity) |
| `/api/authors` | `GET`, `POST`, `PUT`, `DELETE` | Author management & pagination |
| `/api/books` | `GET`, `POST`, `PUT`, `DELETE` | Book catalog & ISBN search |
| `/api/users` | `GET`, `POST`, `PUT`, `DELETE` | Registered user management |
| `/api/loans` | `GET`, `POST`, `PUT` | Book checkout, active loan tracking & return flows |

---

## Graceful Shutdown & Resilience

- **Graceful Shutdown**: `HostOptions.ShutdownTimeout` is configured to 30 seconds to allow active in-flight requests and database transactions to drain cleanly on `SIGTERM` / `SIGINT`.
- **Transient Failure Retries**: Npgsql execution strategy is configured with bounded retries (`maxRetryCount: 3`, exponential backoff up to 5s) to seamlessly absorb temporary network/database hiccups.
- **Structured Logs**: Written to console and rolling daily log files in `src/BookCatalog.Api/logs/bookcatalog-*.log`.
