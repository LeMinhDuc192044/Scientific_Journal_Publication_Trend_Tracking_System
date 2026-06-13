Prompt — Backend System Architecture & API Design

You are a senior backend architect building a scalable backend system for a SaaS platform called Scientific Journal Publication Trend Tracking System using:

ASP.NET Core Web API (.NET 8)
Entity Framework Core + PostgreSQL (Supabase)
Clean Architecture + CQRS + Repository Pattern
JWT Authentication & Authorization
FluentValidation
MediatR
Hangfire or BackgroundService for scheduled synchronization jobs
Swagger/OpenAPI
Serilog Logging
Redis (optional caching)
Docker-ready deployment

Your task is to design and implement a production-ready backend architecture.

PART 1 — System Overview

Build a backend system that supports the following features:

Core Features
- Authentication & Authorization
User registration and login
JWT authentication
Refresh token support
Role-based authorization (Admin, Researcher, User)
Password hashing and security best practices
- Research Paper Management
Search research papers by:
Keyword
Author
Journal
Research topic
View paper metadata:
Title
Abstract
Keywords
Publication year
Authors
Journal
Citation count
Pagination, sorting, and filtering
Publication Trend Analysis
- Track publication trends by:
Keyword
Research topic
Journal
Year
Aggregate publication statistics
Generate dashboard metrics
Provide chart-ready analytical data
Trending Topics
- Detect and display trending research topics
- Rank topics by publication growth
- Support historical comparisons
- Bookmark & Follow System
- Save bookmarks for:
   Research papers
   Keywords
   Research topics
   Follow journals or research topics
Manage saved content
Notification System
Notify users about:
Newly published papers
Trending topics
Followed journal updates
Support in-app notifications
Reporting & Analytics
Generate simple analytical reports
Export statistical summaries
Provide dashboard KPIs
External API Synchronization
Synchronize metadata from external academic APIs:
Semantic Scholar
OpenAlex
Crossref
Scheduled synchronization jobs
API retry handling
Logging synchronization history
Administration
Manage users
Manage API data sources
Manage synchronization settings
Manage system configurations
Monitor synchronization logs
PART 2 — System Assumptions & Constraints

The backend system must follow these assumptions:

Data Source Constraints
The system uses publicly available academic metadata from:
Semantic Scholar API
OpenAlex API
Crossref API
Only metadata is collected, including:
Title
Abstract
Keywords
Publication year
Authors
Journal
The system does NOT process full-text research papers due to:
Copyright restrictions
Storage limitations
Data Assumptions
Third-party API data is assumed to:
Be valid
Have consistent structure
Be continuously available
The system only analyzes selected academic fields such as:
Computer Science
Artificial Intelligence

This scope limitation reduces complexity and improves performance.

Synchronization Constraints
Data synchronization is periodic:
Daily
Weekly
Real-time synchronization is NOT required.
Synchronization jobs should support:
Retry policies
Failure logging
Incremental synchronization
PART 3 — Clean Architecture Structure

Design the backend using Clean Architecture.

Required layers:

src/
 ├── API/
 ├── Application/
 ├── Domain/
 ├── Infrastructure/
 └── Shared/
Domain Layer

Include:

Entities
Enums
Value Objects
Domain Interfaces
Domain Events

Core entities:

User
ResearchPaper
Author
Journal
Keyword
ResearchTopic
PublicationTrend
Bookmark
Notification
DashboardReport
ApiDataSource
SyncJob
SyncLog
RefreshTokenHistory
PART 4 — Database Design

Design PostgreSQL schema using Entity Framework Core.

Requirements:

Proper relationships
Foreign keys
Composite indexes
Query optimization
Soft delete support
Audit fields:
CreatedAt
UpdatedAt
Seed data support

Generate:

DbContext
Entity configurations
Fluent API mappings
Migration strategy
PART 5 — CQRS + MediatR

Implement CQRS pattern.

Requirements:

Commands

Examples:

RegisterUserCommand
LoginCommand
CreateBookmarkCommand
FollowTopicCommand
SyncAcademicDataCommand
Queries

Examples:

SearchResearchPapersQuery
GetTrendingTopicsQuery
GetPublicationTrendQuery
GetDashboardStatisticsQuery
GetNotificationsQuery

Each feature must include:

Request DTO
Response DTO
Validator
Handler
Mapping profile
PART 6 — REST API Design

Design RESTful endpoints.

Authentication
POST /api/auth/register
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
Research Papers
GET /api/papers
GET /api/papers/{id}
GET /api/papers/search
Trends
GET /api/trends/topics
GET /api/trends/publications
GET /api/trends/dashboard
Bookmarks
POST /api/bookmarks
GET /api/bookmarks
DELETE /api/bookmarks/{id}
Notifications
GET /api/notifications
PUT /api/notifications/{id}/read
Admin
GET /api/admin/users
GET /api/admin/sync-logs
POST /api/admin/sync/run

Requirements:

API versioning
Pagination response model
Consistent error handling
Swagger documentation
JWT authorization policies
PART 7 — Background Synchronization

Implement scheduled synchronization services.

Requirements:

Use Hangfire or BackgroundService
Fetch metadata from academic APIs
Normalize external data
Avoid duplicate records
Log synchronization history
Handle API rate limits
Retry failed jobs

Generate:

Synchronization service interfaces
API client services
Retry strategies
Logging structure
PART 8 — Security & Validation

Requirements:

JWT authentication
Refresh tokens
Role-based authorization
Password hashing with BCrypt
FluentValidation for all requests
Global exception handling middleware
Rate limiting (optional)
CORS configuration
PART 9 — Performance & Scalability

Requirements:

Use async/await everywhere
Add caching for dashboard queries
Optimize database queries
Use projection queries instead of loading full entities
Add indexes for:
Keywords
PublicationYear
JournalId
TopicId

Optional:

Redis caching
Response compression
PART 10 — DevOps & Deployment

Requirements:

Environment-based configuration
appsettings.Development.json
appsettings.Production.json
Health checks
CI/CD-ready structure

Provide:


Environment variable examples
PART 11 — Deliverables

Generate:

Complete backend architecture
Folder structure
Entity models
DbContext configuration
Repository interfaces
API controllers
DTOs
Validators
JWT authentication flow
Background synchronization services
PostgreSQL migration strategy
Swagger configuration
Error handling middleware
Sample API responses
Best practices for scalability and maintainability

All code should follow:

SOLID principles
Clean code practices
Dependency injection
Production-ready standards
