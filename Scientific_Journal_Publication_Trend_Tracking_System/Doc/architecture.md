
Use 
/src
│
├── Api
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   ├── Extensions/
│   ├── Program.cs
│   └── appsettings.json
│
├── Application
│   ├── Common/
│   │   ├── Behaviors/
│   │   ├── Exceptions/
│   │   ├── Interfaces/
│   │   ├── Models/
│   │
│   └── Features/
│       ├── Authentication/
│       │   ├── Commands/
│       │   ├── Queries/
│       │   ├── DTOs/
│       │   ├── Validators/
│       │   ├── Mappings/
│       │   └── Services/
│       │
│       ├── ResearchPapers/
│       │   ├── Commands/
│       │   ├── Queries/
│       │   ├── DTOs/
│       │   ├── Validators/
│       │   ├── Mappings/
│       │   └── Services/
│       │
│       ├── Trends/
│       ├── Bookmarks/
│       ├── Notifications/
│       ├── Users/
│       └── [Other Features]/
│
├── Domain
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   ├── Events/
│   ├── Interfaces/
│   └── Common/
│
├── Infrastructure
│   ├── Authentication/
│   ├── Persistence/
│   │   ├── Configurations/
│   │   ├── Migrations/
│   │   ├── Repositories/
│   │   └── AppDbContext.cs
│   │
│   ├── ExternalApis/
│   │   ├── SemanticScholar/
│   │   ├── OpenAlex/
│   │   └── Crossref/
│   │
│   ├── BackgroundJobs/
│   ├── Notifications/
│   ├── Common/
│   └── DependencyInjection.cs
│
└── Shared
    ├── Constants/
    ├── Behaviors/
    ├── Exception/
    ├── Results/
    └── Middleware/

Architecture Overview:
•	Presentation: Controllers layer (currently only AuthController)
•	Application: CQRS pattern with Commands/Handlers for Authentication and DTOs for features but not use this for other features yet. 
    Use layered architecture for other features (Services, Repositories, etc.).
•	Domain: Core entities and enums (13 entities covering auth, papers, trends, bookmarks, notifications, sync, and reports)
•	Infrastructure:
•	Authentication (JWT, password hashing, current user service)
•	Persistence (DbContext, configurations, repositories with Unit of Work pattern)
•	Shared: Cross-cutting concerns (validation, exceptions, middleware, constants, API responses)