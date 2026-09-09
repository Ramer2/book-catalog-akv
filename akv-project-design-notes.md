# BookCatalog - Architectural & System Design Notes

## 1. Executive Summary & Overview

`BookCatalog` is a modular, high-performance web service built with C# and .NET, designed to manage books, authors, registered users, and book lending operations. The application is architected around **Clean Architecture** principles and the **CQRS (Command Query Responsibility Segregation)** pattern, providing clear boundaries between domain logic, persistence, and external presentation layers.

---

## 2. Architectural Blueprint & Layering

The codebase is organized into four distinct projects, enforcing strict unidirectional dependencies:

```mermaid
    API[BookCatalog.Api] --> App[BookCatalog.Application]
    Infra[BookCatalog.Infrastructure] --> App
    Infra --> Domain[BookCatalog.Domain]
    App --> Domain
```

1. **Domain Layer (`BookCatalog.Domain`)**
   - Contains enterprise entities (`Book`, `Author`, `User`, `Loan`), domain exceptions (`EntityNotFoundException`, `BookAlreadyBorrowedException`), and common pagination/search base models (`BaseSearchModelPagedQuery`, `BaseSearchModelPagedResponse<T>`).
   - Free of external framework dependencies, maintaining pure core business rules.

2. **Application Layer (`BookCatalog.Application`)**
   - Implements CQRS request objects (`CreateBookCommand`, `GetAuthorByIdQuery`, etc.) and MediatR request handlers.
   - Houses application services (`BookService`, `AuthorService`, `UserService`, `LoanService`, `IsbnService`) and interface abstractions (`IAuthorRepository`, `ITransactionProvider`, etc.).
   - Orchestrates cross-cutting pipeline behaviors (`LoggingBehavior`, `ValidationBehavior`, `TransactionBehavior`) and DTO mapping profiles (`AutoMapper`).

3. **Infrastructure Layer (`BookCatalog.Infrastructure`)**
   - Implements data persistence via **EF Core** with PostgreSQL.
   - Contains entity configurations (`IEntityTypeConfiguration<T>`), repository implementations leveraging a generic `IRepository<T, TSearchModel>` base template, database exception interpreters (`NpgsqlDbExceptionInterpreter`), and EF migrations.

4. **API Layer (`BookCatalog.Api`)**
   - Exposes RESTful endpoints via ASP.NET Core controllers (`BooksController`, `AuthorsController`, `UsersController`, `LoansController`).
   - Configures Dependency Injection in `ServicesCollectionExtension` and manages HTTP error responses via specialized API Exception Filters.

---

## 3. Data Access & Infrastructure Strategy

### Environment & Database Setup
- **PostgreSQL Containerization**: The runtime environment is fully dockerized using `compose.yaml`, pairing the ASP.NET API service with a PostgreSQL database instance (`books.db`).
- **Configuration Management**: Non-sensitive defaults reside in `appsettings.Development.json`, while production database credentials and MediatR configuration are injected via environment variables and a git-ignored `appsettings.json`.

### EF Core Migration Strategy
- During early prototyping, SQLite in-memory storage was briefly evaluated. Because SQLite mapped `DateTime` fields to text types-which risked carrying non-idiomatic column mappings over to PostgreSQL-the migration history was cleanly reset and re-scaffolded directly against Npgsql/PostgreSQL.
- Production migrations (`SyncPostgresMigration`, `AddUsersAndLoans`, `AddLoanActiveUniqueIndex`, `AddAuthors`) reflect a clean, Postgres-native schema history.

---

## 4. Request Flow, Validation & Cross-Cutting Concerns

### MediatR Pipeline & CQRS
All incoming write commands and read queries pass through a deterministic MediatR pipeline chain:

$$\text{Request} \longrightarrow \text{LoggingBehavior} \longrightarrow \text{ValidationBehavior} \longrightarrow \text{TransactionBehavior} \longrightarrow \text{Handler}$$

- **Logging**: `LoggingBehavior` logs execution context and timing for all incoming MediatR requests.
- **Automated Validation**: `ValidationBehavior` executes FluentValidation rules before requests reach handlers or open database connections.
- **Domain Validations**: Services such as `IIsbnService`, `IAuthorService`, and `IUserService` perform async availability checks (e.g., verifying ISBN uniqueness, author existence, or email/phone availability) inside command validators.

### Exception Handling via API Filters
Instead of global middleware, error handling is delegated to ASP.NET Core API Filters (`UnhandledExceptionFilter`, `NotFoundExceptionFilter`, `ValidationExceptionFilter`, `BookAlreadyBorrowedExceptionFilter`). 
- Domain exceptions are automatically mapped to standard HTTP status codes:
  - `EntityNotFoundException` $\rightarrow$ **404 Not Found**
  - `ValidationException` $\rightarrow$ **400 Bad Request**
  - `BookAlreadyBorrowedException` $\rightarrow$ **409 Conflict**
  - Unhandled exceptions $\rightarrow$ **500 Internal Server Error**
- Every intercepted error generates a structured log trace.

---

## 5. Concurrency, Lending Logic & Transactions

### Lending Rules & Race Condition Mitigation
The loaning system enforces that a book cannot be actively borrowed by multiple users concurrently. To guarantee data integrity under high concurrent load:

1. **Transaction Pipeline Scoping**: Commands marked with the `ITransactionalCommand` interface are automatically wrapped in a database transaction by `TransactionBehavior`.
2. **Database Level Guarantee**: `LoanConfiguration` defines a partial unique index on active loans (`UX_Loan_BookId_Active`) filtered by `ReturnedAt IS NULL`:
   ```csharp
   builder.HasIndex(x => x.BookId)
          .IsUnique()
          .HasDatabaseName("UX_Loan_BookId_Active")
          .HasFilter("\"ReturnedAt\" IS NULL");
   ```
3. **Graceful Exception Translation**: If two concurrent requests pass the initial availability check, PostgreSQL rejects the second insertion via index violation. `NpgsqlDbExceptionInterpreter` intercepts the Npgsql exception and translates it into a domain-level `BookAlreadyBorrowedException`, triggering an immediate HTTP 409 response.

---

## 6. Pagination, Search & Quality Assurance

### Standardized Search & Paging
Read operations utilize standardized pagination and filtering interfaces (`BaseSearchModelPagedQuery` and `BaseSearchModelPagedResponse<T>`). Repositories dynamically apply `Where` predicate filtering, sorting dictionary lookups, and `Skip`/`Take` windowing.

### Automated Testing Strategy
The codebase includes comprehensive unit test coverage built with **NUnit 3**, **Moq**, and **FluentValidation.TestHelper**:
- **Validator Tests**: Verify all required fields, field lengths, regex rules, and async service lookups.
- **Service & Handler Tests**: Validate entity state mutations, repository persistence calls, and mapping to response DTOs.
- **Pipeline & Behavior Tests**: Ensure transactions roll back correctly on error and logging behaviors capture execution telemetry.