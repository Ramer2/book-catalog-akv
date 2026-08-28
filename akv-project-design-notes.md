# Week 1
### Project structure
Project includes default Clean Architecture structure, consisting of four layers (projects):
1. API layer - configs, endpoints, error handling (future)
2. Application layer - services, handlers, DTOs, interfaces
3. Infrastructure layer - `DbContext`, repository implementation, external services
4. Domain - domain entities

I did this to improve and futureproof future development.

### Configs
`appsettings.json` include the actual configs, while `appsettings.Development.json` includes non-critical, example configs.

`appsettings.json` is included into `.gitignore`.

### Data Storage
Temporarily I am using Sqlite in-memory database. I intend to use Postgres when Docker is going to be set up.

Repositories use a common `IRepository` interface as a generic base template.

### Request Flow
For managing request flows I will use CQRS pattern using MediatR. The main advantage for me is ability to integrate automatic validation checks into the MediatR's pipeline.

### Validation
Validation is performed using a FluentValidation library, with validators being built-into the MediatR's pipeline using Behavior.

Added an ISBN service to validate ISBN uniqueness in books.

### Logging
Added `GlobalExceptionHandler` to handle all the errors and for easier logging of said errors.
Also, requests are logged using a `LoggingBehavior` Behavior in the MediatR pipeline.

### Additional tools
AutoMapper - reduces the burden of mapping from one class to another (for example, with DTOs).

### Things to improve
I would love to use Docker for easier environment management and not having to deal with "oh, you have two dotnet versions on your PC, and I can see only one. good luck".

Database is also a bit lackluster, PostgreSQL would be easier to interact with.

For error handling I think I will move to using filters, although I am still considering this.

### Unit Tests
I use NUnit3 and Moq to test validation errors, services, handlers, business logic.

### Exception handling
Is done using API filters. Logs are created on every error caught.

### Pagination and filtering
All pagination and filtering are done using base classes: `BaseSearchModelPagedQuery` and `BaseSearchModelPagedResponse` and search models, such as `BookSearchModel`. They contain key properties, which interact with repositories and handlers.