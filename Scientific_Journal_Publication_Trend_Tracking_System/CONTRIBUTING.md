# Contributing Guidelines

## Project Architecture

This project follows **Clean Architecture** with **CQRS Pattern** using **MediatR**.

### Technology Stack
- **Framework**: ASP.NET Core 8
- **Database**: PostgreSQL (via Supabase)
- **ORM**: Entity Framework Core
- **Validation**: FluentValidation
- **Authentication**: JWT Bearer Tokens
- **Background Jobs**: Hangfire + BackgroundService
- **Logging**:  ASP.NET Core Logging Abstraction (ILogger<T>)
- **Mapping**: AutoMapper
- **API Documentation**: Swagger/OpenAPI

### Folder Structure

```
src/
??? Api/                          # Presentation Layer
?   ??? Controllers/              # API endpoints
?   ??? Middleware/               # Custom middleware
?   ??? Filters/                  # Action filters
?   ??? Extensions/               # Extension methods
??? Application/                  # Application Layer (Business Logic)
?   ??? Features/                 # Feature-based organization
?   ??? Common/                   # Shared application services
?   ??? Mapping/                  # AutoMapper profiles
?   ??? DependencyInjection.cs    # DI registration
??? Domain/                       # Domain Layer (Entities & Interfaces)
?   ??? Entities/                 # Domain entities
?   ??? ValueObjects/             # Value objects
?   ??? Enums/                    # Domain enums
?   ??? Interfaces/               # Domain contracts
??? Infrastructure/               # Infrastructure Layer
?   ??? Persistence/              # EF Core + Supabase
?   ??? ExternalApis/             # Academic API integrations
?   ??? BackgroundJobs/           # Scheduled sync jobs
?   ??? Authentication/           # JWT implementation
?   ??? Notifications/            # Email/in-app notifications
?   ??? Caching/                  # Caching strategies
?   ??? DependencyInjection.cs    # DI registration
??? Shared/                       # Shared Utilities
    ??? Constants/                # Application constants
    ??? Exceptions/               # Custom exceptions
    ??? Results/                  # Result/Response wrappers
```

### Feature-Based Organization

Each feature under `/src/Application/Features/{FeatureName}/` must follow this structure:

```
Features/
??? ResearchPaperManagement/
?   ??? Queries/                  # Read operations
?   ?   ??? SearchPapersQuery.cs
?   ?   ??? GetPaperDetailsQuery.cs
?   ??? Commands/                 # Write operations
?   ?   ??? CreatePaperCommand.cs
?   ??? DTOs/                     # Data transfer objects
?   ?   ??? ResearchPaperDto.cs
?   ?   ??? CreatePaperDto.cs
?   ??? Validators/               # FluentValidation validators
?   ?   ??? SearchPapersValidator.cs
?   ??? Mappings/                 # AutoMapper profiles
?   ?   ??? PaperMappingProfile.cs
?   ??? Services/                 # Feature-specific services (if needed)
?   ??? Responses/                # Feature-specific response models
```

### Naming Conventions

- **Commands**: `{Action}{Entity}Command` (e.g., `CreateUserCommand`)
- **Queries**: `Get{Entity}Query` or `Search{Entity}Query`
- **DTOs**: `{Entity}Dto`, `Create{Entity}Dto`, `Update{Entity}Dto`
- **Handlers**: `{Command/Query}Handler`
- **Validators**: `{Command/Query}Validator`
- **Services**: `I{ServiceName}Service` (interface), `{ServiceName}Service` (implementation)
- **Controllers**: `{Feature}Controller` (e.g., `ResearchPapersController`)

### Code Standards

#### Classes & Methods
- Use **PascalCase** for class and method names
- Use **camelCase** for method parameters and local variables
- Use **_camelCase** for private fields
- Avoid abbreviations except for well-known acronyms (JWT, API, ORM)

#### DTOs & Validation
- DTOs must be immutable (use init properties)
- Include `record` types for simple data structures
- FluentValidation validators must implement `AbstractValidator<T>`
- All DTOs must have corresponding validators

#### Entity Framework
- Entity names should be singular and descriptive
- Foreign keys should follow naming pattern: `{NavigationPropertyName}Id`
- Use shadow properties for sensitive data when appropriate
- Configure entity relationships in `OnModelCreating` using fluent API

#### Error Handling
- Throw domain-specific exceptions from Application layer
- Return standardized `ApiResponse<T>` from controllers
- All exceptions must be caught and logged via Serilog

#### Async/Await
- All I/O operations must be async
- Methods performing async work must return `Task` or `Task<T>`
- Method names should end with `Async` (optional, but preferred)
- Use `ConfigureAwait(false)` in library code

### Logging Standards

Use Serilog with structured logging:

```csharp
_logger.LogInformation("Processing research paper sync from {ApiSource}", apiName);
_logger.LogError(exception, "Failed to fetch papers from {ApiSource} for keyword {Keyword}", 
    apiName, keyword);
```

### Database

- All entities must have a `CreatedAt` and `UpdatedAt` timestamp
- Use PostgreSQL-specific features (JSON columns, arrays) when beneficial
- Include audit logging for critical operations
- Soft deletes recommended for Bookmark, Notification, User entities

### API Versioning

- Use URL-based versioning: `/api/v1/...`
- Controllers must be attributed: `[ApiVersion("1.0")]`
- Support backward compatibility across major versions

### Testing

- Unit tests for Application layer services
- Integration tests for Infrastructure services
- Controller tests for API endpoints
- Minimum 70% code coverage for critical paths

### Documentation

- Include XML documentation comments on public API members
- Document complex algorithms and business logic
- Update README.md when adding new features
- Include API documentation in Swagger/OpenAPI

### Git & Commit

- Use descriptive commit messages
- Follow conventional commits: `feat:`, `fix:`, `refactor:`, `docs:`
- Create feature branches: `feature/{feature-name}`
- Require PR reviews before merging to main