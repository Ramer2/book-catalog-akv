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

### Additional tools
AutoMapper - reduces the burden of mapping from one class to another (for example, with DTOs).
